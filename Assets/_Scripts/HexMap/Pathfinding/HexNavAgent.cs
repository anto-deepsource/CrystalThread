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
	public class HexNavAgent : MonoBehaviour {

		#region Public Properties

		public float moveSpeed = 8;

		public float radius = 0.5f;

		private Vector3 _destination;
		public Vector3 Destination {
			get { return _destination; }
			set {
				if( _destination!=value ) {
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

		/// <summary>
		/// Returns the remaining positions in the list of current global path points
		/// </summary>
		public List<Vector2Int> GlobalPathPoints {
			get {
				if (!HasRemainingWayPoints()) {
					return new List<Vector2Int>();
				}
				int index = currentGlobalPathIndex;
				int count = currentFullGlobalPath.Count - currentGlobalPathIndex;
				return currentFullGlobalPath.GetRange(index,count);
			}
		}

		

		#endregion

		#region Private Members

		private bool pathNeedsRecalculate = false;

		private List<Vector2Int> currentFullGlobalPath;
		
		private int currentGlobalPathIndex = -1;

		/// <summary>
		/// The axial coordinates of the tile the agent is currently in/on.
		/// </summary>
		private Vector2Int startWorldCoords;

		/// <summary>
		/// The world position scaled/reduced by the HexMap's tilesize.
		/// This is used for distance costs, it was either tell each node about the hexmap's tilesize or scale the input.
		/// </summary>
		private Vector3 startUnitWorldPos;

		private Vector2Int destinationWorldCoords;
		/// <summary>
		/// The world position scaled/reduced by the HexMap's tilesize.
		/// This is used for distance costs, it was either tell each node about the hexmap's tilesize or scale the input.
		/// </summary>
		private Vector3 destinationUnitWorldPos;

		private HexMap map;

		private WorkSet<Vector2Int> openList = new WorkSet<Vector2Int>();
		private WorkSet<Vector2Int> closedList = new WorkSet<Vector2Int>();

		#endregion

		// Use this for initialization
		void Start() {
			map = HexNavMeshManager.GetHexMap();
			NextPosition = transform.position;
		}

		// Update is called once per frame
		void Update() {
			//if ( HasRemainingWayPoints() ) {
			//	currentWorldPos = map.WorldPositionToAxialCoords(transform.position);
			//	if ( !currentFullGlobalPath.Contains(currentWorldPos)) {
			//		pathNeedsRecalculate = true;
			//		currentUnitWorldPos = ToUnitWorldPosition(transform.position);
			//	}
			//}
			

			if ( pathNeedsRecalculate ) {
				// The path needing recalculation could be one of several scenarios:
				// -This is the very first time we've been given destination
				// -The start position has changed
				// -The destination position has changed
				// -Something about the terrain and previous path has changed

				// We can leverage some of these different things to be more efficient
				// but for now let's just dump anything we previously did
				ClearPath();
				// and preprocess everytime
				ProcessPath();

				//switch (Status) {
				//	case PathStatus.None:
				//		// There was no path previously, this is the first time we've been given a destination
				//		ClearPath();
				//		ProcessPath();
				//		break;
				//	case PathStatus.Processing:
				//		break;
				//	case PathStatus.Failed:
				//		break;
				//	case PathStatus.Succeeded:
				//		break;
				//}

				pathNeedsRecalculate = false;
			}

			if ( Status == PathStatus.Succeeded  ) {

				if (HasRemainingWayPoints()) {
					TryToSkipWayPoint();

					Vector2Int nextWaypoint = currentFullGlobalPath[currentGlobalPathIndex];
					Vector3 pos = map.AxialCoordsToWorldPosition(nextWaypoint);
					Vector3 vector = pos - DropY(transform.position);
					if (vector.magnitude < 0.9f) {
						NextWayPoint();
					} else {
						MoveVector = vector.normalized;
						NextPosition = transform.position + vector.normalized * moveSpeed * Time.deltaTime;

					}
				}
				
				
			}
		}

		#region Private Helper Methods

		private Vector3 DropY( Vector3 value ) {
			return new Vector3(value.x, 0, value.z);
		}

		private bool HasRemainingWayPoints() {
			if (currentFullGlobalPath == null ||
					currentFullGlobalPath.Count == 0 ||
					currentGlobalPathIndex == -1 ||
					currentGlobalPathIndex >= currentFullGlobalPath.Count) {
				return false;
			}
			return true;
		}

		private void NextWayPoint() {
			currentGlobalPathIndex++;
		}

		private void TryToSkipWayPoint() {

			// If this is the last waypoint then no way to skip
			if (currentGlobalPathIndex + 1 >= currentFullGlobalPath.Count) {
				return;
			}

			//// Use a capsule cast to determine if there are walls i the way
			//{
			//	Vector2Int nextWaypoint = currentFullGlobalPath[currentGlobalPathIndex + 1];
			//	Vector3 pos = map.AxialCoordsToWorldPositionWithHeight(nextWaypoint);

			//	Vector3 vector = pos - transform.position;

			//	RaycastHit hit;
			//	if (!Physics.CapsuleCast(transform.position, transform.position, radius, vector.normalized,
			//			out hit, vector.magnitude)) {
			//		NextWayPoint();
			//	}
			//}

			// I'm not a fan of this way simply because its costly and if there are any hills in the way it won't work

			// We can basically accomplish the same thing by simply checking whether we are on that tile
			Vector2Int nextWaypoint = currentFullGlobalPath[currentGlobalPathIndex];

			Vector2Int currentCoords = map.WorldPositionToAxialCoords(transform.position);
			if ( nextWaypoint == currentCoords ) {
				NextWayPoint();
			}
		}

		private void InvalidatePath() {
			pathNeedsRecalculate = true;
		}

		private void ClearPath() {
			currentFullGlobalPath?.Clear();
			startWorldCoords = map.WorldPositionToAxialCoords(transform.position);
			startUnitWorldPos = ToUnitWorldPosition(transform.position);
			destinationWorldCoords = map.WorldPositionToAxialCoords(Destination);
			destinationUnitWorldPos = ToUnitWorldPosition(Destination);
			openList.Clear();
			closedList.Clear();
			NextPosition = transform.position;
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
			// is surrounded by walls
		}

		private IEnumerator _ProcessPath() {
			// Initialize the process
			Status = PathStatus.Processing;

			// The lists should be cleared

			// Start by adding the starting tile
			{
				GlobalNode startNode = map.GetGlobalNode(startWorldCoords);
				var newWork = new NodeWork<Vector2Int>(startNode, null);
				Vector3 nodePos = HexUtils.PositionFromCoordinates(startNode.Position, 1);
				newWork.Cost = (nodePos - startUnitWorldPos).magnitude;
				//newWork.Heuristic = GlobalHeuristic(nodePos, destinationUnitWorldPos);
				newWork.Heuristic = HexUtils.Distance(startNode.Position, destinationWorldCoords);
				openList.Add(newWork);
			}

			// As long as we're still Processing and we have at least 1 node process:
			while ( Status == PathStatus.Processing && openList.Count > 0 ) {
				// Pop the most promising next node work
				NodeWork<Vector2Int> work = openList.PopBest();

				// If the tile we end up in after this is the destination tile
				// complete success
				if ( work.Node.Position == destinationWorldCoords ) {
					CompletePathSuccess(work);
					continue;
				}

				// Process each of the edges for this node
				foreach( var edge in work.Node.Edges() ) {
					// each edge should have the start as this node
					// get the node that corresponds to the end
					GlobalNode endNode = map.GetGlobalNode(edge.End);
					// We'll have to calculate the work so do that now
					NodeWork<Vector2Int> newWork = NewGlobalNodeWork(endNode, work, edge );

					// check whether the end node for this edge is already in our open list
					NodeWork<Vector2Int> openWork;
					if ( openList.TryGet(endNode, out openWork ) ) {
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
					NodeWork<Vector2Int> closedWork;
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

				closedList.Add(work);

				yield return null;
			}
			
			// If we haven't successfully found a path yet then there isn't one
			// This will only happen if the agent is surrounded by walls relative to the destination
			if (Status == PathStatus.Processing) {
				CompletePathFailure();
			}
		}

		private void CompletePathSuccess(NodeWork<Vector2Int> work) {
			Status = PathStatus.Succeeded;

			// we need to do a few things to the path in order for the agent to then use it
			currentFullGlobalPath = new List<Vector2Int>();
			NodeWork<Vector2Int> currentWork = work;
			while ( currentWork!=null) {
				// add them to the list at the beginning because we're moving through the path from end to start
				currentFullGlobalPath.Insert(0, currentWork.Node.Position);
				currentWork = currentWork.Parent;
			}

			currentGlobalPathIndex = 0;
		}

		private void CompletePathFailure() {
			Debug.Log("Failed to find path");

		}

		private NodeWork<Vector2Int> NewGlobalNodeWork(GlobalNode node, NodeWork<Vector2Int> cameFrom, Edge<Vector2Int> edge) {
			NodeWork<Vector2Int> newWork = new NodeWork<Vector2Int>(node, cameFrom);
			Vector3 nodePos = HexUtils.PositionFromCoordinates(node.Position);
			//if (cameFrom == null) {
			//	// This is a start node/edge
			//	// set the starting cost of this edge as the distance from the unit's current 'world' position to the center of the edge
			//	newWork.Cost = (nodeUWP - currentUnitWorldPos).magnitude;
			//	newWork.Heuristic = GlobalHeuristic(nodeUWP, destinationUnitWorldPos);
			//} else {
			newWork.Cost = cameFrom.Cost + edge.Cost();
			//newWork.Heuristic = GlobalHeuristic(nodePos, destinationUnitWorldPos);
			newWork.Heuristic = HexUtils.Distance(node.Position, destinationWorldCoords);
			return newWork;

		}

		/// <summary>
		/// The world position scaled/reduced by the HexMap's tilesize. Also drops the y or height.
		/// This is used for distance costs, it was either tell each node about the hexmap's tilesize or scale the input.
		/// </summary>
		private Vector3 ToUnitWorldPosition( Vector3 position ) {
			return new Vector3(position.x / map.Metrics.tileSize, 0, position.z / map.Metrics.tileSize);
		}

		/// <summary>
		/// Returns the X-Z manhatten distance from the start position to the end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		private float GlobalHeuristic( Vector3 start, Vector3 end ) {
			return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);
		}

		#endregion
	}
}