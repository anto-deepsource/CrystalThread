
using Poly2Tri;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	/// <summary>
	/// Acts similiar to Unity's NavMeshAgent, but performs on HexMaps.
	/// Can continuously take new destination parameters and efficiently recalculates a correct new path.
	/// Can determine whether a path exists to the destination at all.
	/// Can determine if, while on a path that theoretically works, the path is not realistically working and adapt.
	/// Opens up a few gettables: next position, List of path points.
	/// Steers around obstacles and cuts corners where applicable.
	/// </summary>
	public class HexNavLocalAgent : MonoBehaviour {

		#region Public Properties

		public float moveSpeed = 8;

		public float stopDistance = 1;

		public float radius = 0.5f;

		private Vector3 _destination;
		public Vector3 Destination {
			get { return _destination; }
			set {
				if (!approxDestPosition.ApproxEquals( value, stopDistance ) ) {
					approxDestPosition = _destination;
					InvalidatePath();
				}
				_destination = value;

			}
		}

		/// <summary>
		/// Calculates the position for the agent considering:
		///		-the current path
		///		-its move speed
		///		-one frame step at a time
		/// </summary>
		public Vector3 NextPosition { get; private set; }

		/// <summary>
		/// A magnitude-1 vector that represents the direction the agent
		/// currently needs to move on the x-z plane.
		/// </summary>
		public Vector3 MoveVector { get; private set; }

		public PathStatus Status { get; private set; }

		private List<Vector3> path = new List<Vector3>();

		public List<Vector3> Path { get { return path; } }

		#endregion

		#region Private Members

		private bool pathNeedsRecalculate = false;

		private List<Vector3> currentFullLocalPath;

		private int currentLocalPathIndex = -1;

		/// <summary>
		/// The world position scaled/reduced by the HexMap's tilesize.
		/// This is used for distance costs, it was either tell each node about the hexmap's tilesize or scale the input.
		/// </summary>
		private Vector3 startPosition;

		private Vector2Int currentAxialCoords;

		private HexTile currentTile;

		/// <summary>
		/// The world position scaled/reduced by the HexMap's tilesize.
		/// This is used for distance costs, it was either tell each node about the hexmap's tilesize or scale the input.
		/// </summary>
		private Vector3 destinationPosition;

		private Vector3 lastPosition;

		private Vector3 approxDestPosition;

		private HexMap map;

		private WorkSet<DelaunayTriangle> openList = new WorkSet<DelaunayTriangle>();
		private WorkSet<DelaunayTriangle> closedList = new WorkSet<DelaunayTriangle>();

		private NodeWork<DelaunayTriangle> bestWorkSoFar = null;

		private LocalNode currentStartNode;
		private LocalNode currentEndNode;
		private LocalNode currentActualNode;

		private NodeWork<DelaunayTriangle> completedWork = null;

		#endregion

		void Start() {
			map = HexNavMeshManager.GetHexMap();
			NextPosition = transform.position;
			lastPosition = transform.position;
		}
		
		void Update() {

			Vector2Int coords = map.WorldPositionToAxialCoords(transform.position);
			currentActualNode = map.GetLocalNodeByPosition(transform.position);

			if (currentTile == null || coords != currentAxialCoords) {
				currentTile = map.GetTile(coords);
				currentAxialCoords = coords;
			}

			// if somehow we end up on a node that is not on our path we should recalculate
			if ( Status== PathStatus.Succeeded && !completedWork.Contains( currentActualNode) ) {
				Debug.Log("Got off path");
				InvalidatePath();
			}
			// TODO: figure out some way to check whether we need to recalculate the path
			// if we are still processing and our current actual node changes,
			// but avoiding false positives like following the current best path

			if (pathNeedsRecalculate) {
				// The path needing recalculation could be one of several scenarios:
				// -This is the very first time we've been given destination
				// -The start position has changed
				// -The destination position has changed
				// -Something about the terrain and previous path has changed

				switch (Status) {
					case PathStatus.None:
						// There was no path previously, this is the first time we've been given a destination
						ClearPath();
						ProcessPath();
						break;
					case PathStatus.Processing:
					case PathStatus.Partial:
					default:
						//if ( !SalvagePathMaybe() ) {
							ClearPath();
							ProcessPath();
						//}
						break;
					//case PathStatus.Failed:
					//	ClearPath();
					//	ProcessPath();
					//	break;
					//case PathStatus.Succeeded:
					//	ClearPath();
					//	ProcessPath();
					//	break;
				}

				pathNeedsRecalculate = false;
			}

			//if (Status == PathStatus.Succeeded || Status == PathStatus.Partial) {
			//while (TryToSkipWayPoint()) { }
			TryToSkipWayPoint();

			Vector3 newMoveVector = MoveVector;

			if (HasRemainingWayPoints()) {
					
				Vector3 pos = currentFullLocalPath[currentLocalPathIndex];
				Vector3 vector = pos - DropY(transform.position);
				if (vector.magnitude < 0.9f) {
					NextWayPoint();
				} else {
					newMoveVector = vector.normalized;
					//NextPosition = transform.position + vector.normalized * moveSpeed * Time.deltaTime;

				}
			} else {
				// we can simply move straight towards the destination point
				Vector3 pos = Destination.DropY();
				Vector3 vector = pos - DropY(transform.position);
				if (vector.magnitude < 2.9f) {
					// stop
					newMoveVector = Vector3.zero;
					//NextPosition = transform.position;
				} else {
					newMoveVector = vector.normalized;
					//NextPosition = transform.position + vector.normalized * moveSpeed * Time.deltaTime;

				}
			}
			MoveVector = MoveVector * 0.9f + newMoveVector * 0.1f;
			//}

			lastPosition = transform.position;
		}

		#region Private Helper Methods

		private Vector3 DropY(Vector3 value) {
			return new Vector3(value.x, 0, value.z);
		}

		private bool HasRemainingWayPoints() {
			if (currentFullLocalPath == null ||
					currentFullLocalPath.Count == 0 ||
					currentLocalPathIndex == -1 ||
					currentLocalPathIndex >= currentFullLocalPath.Count) {
				return false;
			}
			return true;
		}

		private void NextWayPoint() {
			currentLocalPathIndex++;
			ValidatePath();
		}

		private bool TryToSkipWayPoint() {

			// if there's no hextile data then there's not much we can do
			if( currentTile== null || !HasRemainingWayPoints()) {
				return false;
			}

			// Use a cast to determine if there are walls in the way
			{
				Vector3 pos;
				if ( currentLocalPathIndex + 1 < currentFullLocalPath.Count ) {
					pos = currentFullLocalPath[currentLocalPathIndex + 1];
				} else {
					pos = destinationPosition;
				}
				
				// make a quad/box that is our hypothetical projected path from here to the target point
				Vector2[] projectionQuad = ColDet.CreateQuad(pos.JustXZ(), transform.position.JustXZ(), radius * 2.0f);
				
				if ( !currentTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad) ) {
					// There MAY still be obstacles if the way point is outside the current tile
					Vector2Int coords = map.WorldPositionToAxialCoords(pos);
					if ( !(coords == currentAxialCoords) ) {
						HexTile nextTile = map.GetTile(coords);
						if (!nextTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad)) {
							NextWayPoint();
							return true;
						}
					} else {
						NextWayPoint();
						return true;
					}

					
				}
				
			}

			return false;
		}

		private void InvalidatePath() {
			pathNeedsRecalculate = true;
		}

		private void ClearPath() {
			path.Clear();
			currentFullLocalPath?.Clear();
			startPosition = DropY( transform.position );
			destinationPosition = DropY(Destination);
			openList.Clear();
			closedList.Clear();
			//bestWorkSoFar = null;
			completedWork = null;
			NextPosition = transform.position;
		}

		/// <summary>
		/// Evaluates the previously processed word against what will be needed for the new path
		/// and returns true if some or all of the path can be reused, and false if not.
		/// </summary>
		/// <returns></returns>
		private bool SalvagePathMaybe() {
			Vector3 newStartPos = DropY(transform.position);
			Vector3 newDesPos = DropY(Destination);

			//LocalNode currentActualNode = map.GetLocalNodeByPosition(newStartPos);
			LocalNode newEndNode = map.GetLocalNodeByPosition(newDesPos);

			// maybe neither of the nodes have been changed and we can not change anything and keep looking
			if (currentActualNode == currentStartNode && newEndNode == currentEndNode) {
				//Debug.Log("Kept on the same path");
				return true;
			}

			// if the start nodes are still the same but the new destination node is different
			if (currentActualNode == currentStartNode && newEndNode != currentEndNode) {
				//	we may have already found a path to that node (ie: if it has moved toward us)
				NodeWork<DelaunayTriangle> closedWork;
				if (closedList.TryGet(newEndNode, out closedWork)) {
					CompletePathSuccess(closedWork);
					//Debug.Log("New dest was on the way");
					return true;
				}

				// can't think of any other ways to salvage
			}

			// if the dest nodes are the same but the new start node is different
			if (currentActualNode != currentStartNode && newEndNode == currentEndNode) {
				// if we've already processed the new start node then
				// that's not a save all but it at least means
				// that the new node is on the way and we may have processed some of it already
				NodeWork<DelaunayTriangle> closedWork;
				if (closedList.TryGet(newEndNode, out closedWork)) {
					// set this work to be the new end/root
					closedWork.Emancipate();
					// we can go through the children of this work and start our path search from there
					// first clear the path
					ClearPath();
					IterateThroughChildrenAndAddToOpenList(closedWork);

					// if somehow we still have nothing in the openlist then we failed to salvage anything
					if (openList.Count == 0) {
						return false;
					}

					// if we have anything in the open list then we at least have the one start node
					// queued up ready to process, possibly a few things already processed
					// we don't need/want to restart the process
					//Debug.Log("New start was on the way");
					return true;
				}
			}

			return false;
		}

		private void IterateThroughChildrenAndAddToOpenList( NodeWork<DelaunayTriangle> work ) {
			// if it has no children then it hasn't been processed yet and can go back on the open list
			if ( work.Children.Count == 0 ) {
				openList.Add(work);
			} else {
				// go through each of the children
				foreach (var child in work.Children) {
					IterateThroughChildrenAndAddToOpenList(child);
				}
			}
		}

		private void ProcessPath() {
			// we only want/need one process coroutine at a time
			StopAllCoroutines();
			StartCoroutine(_ProcessPath());
			// TODO: run a parallel coroutine that attempts to find the path from end to start
			//		If either of the coroutines succeeds -> success
			//		If either of the coroutines fail -> fail
			// This will sometimes help find paths sooner, but mostly it will allow the path to fail
			// instead of processing endlessly in the case that our destination is unreachable because it
			// is surrounded by walls.
			//	This isn't so much an issue with Local since the nav mesh only exists for as far as the tiles are generated.
		}

		private IEnumerator _ProcessPath() {
			// Initialize the process
			Status = PathStatus.Processing;

			// The lists should already be cleared by here

			// Start by adding the starting tile
			{
				currentStartNode = map.GetLocalNodeByPosition(startPosition);
				var newWork = new NodeWork<DelaunayTriangle>(currentStartNode, null);
				newWork.Cost = (currentStartNode.WorldPos - startPosition).magnitude;
				//newWork.Heuristic = GlobalHeuristic(nodePos, destinationUnitWorldPos);
				newWork.Heuristic = Heuristic(currentStartNode.WorldPos, destinationPosition);
				openList.Add(newWork);
			}

			currentEndNode = map.GetLocalNodeByPosition(destinationPosition);

			// As long as we're still Processing and we have at least 1 node process:
			while ((Status == PathStatus.Processing || Status == PathStatus.Partial) && openList.Count > 0) {
				// Pop the most promising next node work
				NodeWork<DelaunayTriangle> work = openList.PopBest();

				// If the tile we end up in after this is the destination tile
				// complete success
				if ( work.Node == currentEndNode) {
					CompletePathSuccess(work);
					continue;
				}

				// Process each of the edges for this node
				foreach (var edge in work.Node.Edges()) {
					// each edge should have the start as this node
					// get the node that corresponds to the end
					LocalNode endNode = map.GetLocalNodeByTriangle(edge.End);
					// We'll have to calculate the work so do that now
					NodeWork<DelaunayTriangle> newWork = NewLocalNodeWork(endNode, work, edge);

					// check whether the end node for this edge is already in our open list
					NodeWork<DelaunayTriangle> openWork;
					if (openList.TryGet(endNode, out openWork)) {
						// this node is already on the open list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < openWork.TotalEstimatedCost) {
							// lets remove the one that's already there and replace it with this one
							openList.Remove(openWork);
							openList.Add(newWork);
						}
						// Either way skip to the next edge
						continue;
					} // end of check whether in our open list

					// check if this node is already on our closed list
					NodeWork<DelaunayTriangle> closedWork;
					if (closedList.TryGet(endNode, out closedWork)) {
						// this node is already on the closed list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < closedWork.TotalEstimatedCost) {
							// lets remove the one that's already there and put the new one back on the open list
							closedList.Remove(closedWork);
							openList.Add(newWork);
						}
						// Either way skip to the next edge
						continue;
					} // end of check if this node is already on our closed list

					// if we got here then just add it to the open list
					openList.Add(newWork);
				}

				SetBestBathSoFar(work);

				closedList.Add(work);

				yield return null;
			}

			// If we haven't successfully found a path yet then there isn't one
			// This will only happen if the agent is surrounded by walls relative to the destination
			if (Status == PathStatus.Processing) {
				CompletePathFailure();
			}
		}

		private void CompletePathSuccess(NodeWork<DelaunayTriangle> work) {
			Status = PathStatus.Succeeded;
			completedWork = work;

			// we need to do a few things to the path in order for the agent to then use it
			currentFullLocalPath = new List<Vector3>();
			NodeWork<DelaunayTriangle> currentWork = work;
			while (currentWork != null) {
				// add them to the list at the beginning because we're moving through the path from end to start
				var center = currentWork.Node.Position.Centroid();
				var point = new Vector3(center.Xf, 0, center.Yf);
				currentFullLocalPath.Insert(0, point);
				currentWork = currentWork.Parent;
			}

			currentLocalPathIndex = 0;
			ValidatePath();
		}

		private void CompletePathFailure() {
			Debug.Log("Failed to find path");
			Status = PathStatus.Failed;
		}

		private void SetBestBathSoFar(NodeWork<DelaunayTriangle> work) {

			// take either:
			// -the first work
			// -the lesser estimated cost work
			// -any child of the current best work (essentially chose some path and stick with it)

			bool takeThisWork = bestWorkSoFar == null ||
				work.TotalEstimatedCost < bestWorkSoFar.TotalEstimatedCost
				//||
				//work.Contains(bestWorkSoFar.Node) 
				;
			Status = PathStatus.Partial;
			if (takeThisWork) {

				

				// we need to do a few things to the path in order for the agent to then use it
				currentFullLocalPath = new List<Vector3>();
				NodeWork<DelaunayTriangle> currentWork = work;
				while (currentWork != null) {
					// add them to the list at the beginning because we're moving through the path from end to start
					var center = currentWork.Node.Position.Centroid();
					var point = new Vector3(center.Xf, 0, center.Yf);
					currentFullLocalPath.Insert(0, point);
					currentWork = currentWork.Parent;
				}

				if (bestWorkSoFar == null ) {
					currentLocalPathIndex = 0;
				} else
				if ( currentLocalPathIndex >= currentFullLocalPath.Count) {

					currentLocalPathIndex = currentFullLocalPath.Count-1;
				}
				ValidatePath();

				bestWorkSoFar = work;
			}
		}

		private void ValidatePath() {
			path.Clear();
			for( int i = currentLocalPathIndex; i < currentFullLocalPath.Count; i ++ ) {
				path.Add(currentFullLocalPath[i]);
			}
			path.Add(Destination.DropY());
		}

		private NodeWork<DelaunayTriangle> NewLocalNodeWork(LocalNode node, 
				NodeWork<DelaunayTriangle> cameFrom, Edge<DelaunayTriangle> edge) {
			NodeWork<DelaunayTriangle> newWork = new NodeWork<DelaunayTriangle>(node, cameFrom);

			newWork.Cost = cameFrom.Cost + edge.Cost();
			newWork.Heuristic = Heuristic(node.WorldPos, destinationPosition);
			return newWork;

		}
		
		private float Heuristic(Vector3 start, Vector3 end) {

			//// X-Z manhatten distance from the start position to the end.
			//return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);

			//// Euclidean distance (way the crow flys)
			//return (end - start).magnitude;

			// Move along the legs of a right triangle between start and end
			// Sort of if manhattan distance rotated to be the worst case from any point to any other
			// hyp = euclidean distance, a = b, h^2 = b^2 + a^2
			// 2a^2 = h^2
			// a^2 = h^2/2
			// a = sqrt( h^2/2 )
			float h2 = (end - start).sqrMagnitude;
			float a = Mathf.Sqrt(h2 * 0.5f);
			return a * 2;
		}

		#endregion
	}
}