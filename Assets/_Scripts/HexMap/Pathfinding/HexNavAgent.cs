
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
	public class HexNavAgent : MonoBehaviour {

		#region Public Properties

		public float stopDistance = 1;

		public float radius = 0.5f;
		public float height = 2;

		//private Vector3 _destination;
		public Vector3 Destination {
			get {
				return globalPath.Destination;
			}
			set {
				InitializeMaybe();
				globalPath.Destination = value;
				Vector2Int coords = map.WorldPositionToAxialCoords(value);
				if (coords != currentDestinationAxialCoords) {
					currentDestinationAxialCoords = coords;
					globalPath.Valid = false;
					globalPathNeedsRestart = true;
				}
			}
		}

		/// <summary>
		/// A magnitude-1 vector that represents the direction the agent
		/// currently needs to move on the x-z plane.
		/// </summary>
		public Vector3 MoveVector { get; private set; }

		#endregion

		#region Private Members

		public PathWalker<DelaunayTriangle> LocalPath { get { return localPath; } }
		private PathWalker<DelaunayTriangle> localPath;

		public GlobalPathWalker GlobalPath { get { return globalPath; } }
		private GlobalPathWalker globalPath;

		private Vector2Int currentAxialCoords;

		private Vector2Int currentDestinationAxialCoords;

		private HexTile currentTile;

		private HexMap map;

		public float axialCoordUpdateDelay = 0.03f;
		private float axialCoordUpdateDelayTimer = 0.0f;

		private bool running = false;

		private bool globalPathNeedsRestart = false;

		private PathJobResult<Vector2Int> currentGlobalPathResult;

		//private HashSet<string> debugFlags = new HashSet<string>();

		#endregion

		public Vector3 Center {
			get { return transform.position + Vector3.up * height * 0.5f; }
		}

		void Start() {
			InitializeMaybe();
		}

		/// <summary>
		/// Can be used by other components to stop the agent from processing.
		/// </summary>
		public void Stop() {
			StopAllCoroutines();
			running = false;
		}

		private void InitializeMaybe() {
			if (localPath == null || globalPath == null) {
				map = HexNavMeshManager.GetHexMap();

				PathJob<DelaunayTriangle> localJob = HexNavMeshManager.NewLocalJob();

				localPath = new PathWalker<DelaunayTriangle>(localJob, stopDistance);
				localPath.SetStart(transform.position);

				currentAxialCoords = map.WorldPositionToAxialCoords(transform.position);

				//PathJob<Vector2Int> globalJob = HexNavMeshManager.NewGlobalJob();
				globalPath = new GlobalPathWalker(stopDistance);
				//globalJob.SetStart(transform.position);
				currentGlobalPathResult = null;
				globalPathNeedsRestart = true;
			}
			running = true;
		}

		void Update() {

			if (!running) {
				return;
			}
			//debugFlags.Clear();
			//globalPath.ResetDebugFlags();
			//localPath.ResetDebugFlags();

			axialCoordUpdateDelayTimer += Time.deltaTime;
			if (axialCoordUpdateDelayTimer > axialCoordUpdateDelay) {
				axialCoordUpdateDelayTimer = 0;
				Vector2Int coords = map.WorldPositionToAxialCoords(transform.position);

				HexTile coordsTile = map.GetTile(coords);

				if (coordsTile!= currentTile || coords != currentAxialCoords) {

					if (currentGlobalPathResult==null ||
							!currentGlobalPathResult.UsesPosition(currentAxialCoords) ) {
						globalPathNeedsRestart = true;
					}
					

					currentTile = coordsTile;

					if (currentTile == null) {
						SetBodyIntangible();
					} else {
						SetBodyCorporeal();
					}

					currentAxialCoords = coords;
				}

				//if (globalPath.Valid) {
				//	CheckStillOnGlobalPath();
				//}
				localPath.SetStart(transform.position);
				//globalPath.SetStart(transform.position);
				//globalPath.Flag("Update axial coords");
			}

			if (globalPathNeedsRestart) {
				//globalPath.SetStart(transform.position);
				//globalPath.Flag("Restart coroutine");
				ProcessGlobalPath();
			}

			//if ( globalPath.DebugThatFuckingBug() ) {
			//globalPath.Flag("Next best work already in closed list");
			//}

			//globalPath.UpdateJob();

			TryToSkipGlobalWayPoint();

			if (globalPath.Valid) {

				// ------Local ------------
				localPath.Destination = globalPath.GetCurrentWayPoint();

				if (localPath.NeedsRestart) {
					localPath.SetStart(transform.position);
					ProcessLocalPath();
				}

				TryToSkipLocalWayPoint();

				MoveVector = globalPath.MoveVector(transform.position);

				if (localPath.Valid) {
					MoveVector = localPath.MoveVector(transform.position);

					// we can either have them pause and wait or at least run straight toward the global point
					// this smooths out some problems and paths but ends up running them in the wrong direction other times
					//} else {
					//	MoveVector = Vector3.zero;
				}

			} else {
				MoveVector = Vector3.zero;
			}

			SteerAwayFromObstacles();
		}

		#region Private Helper Methods

		private void SetBodyIntangible() {
			Collider myCollider = GetComponent<Collider>();
			if (myCollider != null) {
				myCollider.enabled = false;
			}
		}

		private void SetBodyCorporeal() {
			Collider myCollider = GetComponent<Collider>();
			if (myCollider != null) {
				myCollider.enabled = true;
			}
			
		}

		private void CheckStillOnGlobalPath() {
			GlobalNode currentActualNode = map.GetGlobalNode(currentAxialCoords);

			// if somehow we end up on a node that is not on our path we should recalculate
			//if (!globalPath.PathContainsNode(currentActualNode)) {
			globalPathNeedsRestart = true;
			//}
		}

		private void TryToSkipLocalWayPoint() {

			// if there's no hextile data then there's not much we can do
			if (currentTile == null) {
				return;
			}



			if (!localPath.HasRemainingWayPoints()) {
				//// try to skip the current global way point
				//Vector3 pos = globalPath.GetCurrentWayPoint();
				//// make a quad/box that is our hypothetical projected path from here to the target point
				//Vector2[] projectionQuad = ColDet.CreateQuad(pos.JustXZ(), transform.position.JustXZ(), radius * 2.0f);
				//// If the quad doesn't collide with anything on this tile then we can skip this waypoint
				//if (!CheckQuadAgainstCurrentTileAndTileWithPosition(projectionQuad, pos)) {
				//	globalPath.QueueNextWayPoint();
				return;
				//}
			} else

			// Use a cast to determine if there are walls in the way
			{
				Vector3 pos = localPath.GetNextWayPoint();
				
				// make a quad/box that is our hypothetical projected path from here to the target point
				Vector2[] projectionQuad = ColDet.CreateQuad(pos.JustXZ(), transform.position.JustXZ(), radius * 2.0f);

				// If the quad doesn't collide with anything on this tile then we can skip this waypoint
				if ( !CheckQuadAgainstCurrentTileAndTileWithPosition( projectionQuad, pos ) ) {
					localPath.QueueNextWayPoint();
					//debugFlags.Add("Skipped local way point");
					// try to skip one more
					TryToSkipLocalWayPoint();
					return;
				}

				//if (!currentTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad)) {
				//	// There MAY still be obstacles if the way point is outside the current tile
				//	Vector2Int coords = map.WorldPositionToAxialCoords(pos);
				//	if (!(coords == currentAxialCoords)) {
				//		HexTile nextTile = map.GetTile(coords);
				//		if (nextTile != null && !nextTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad)) {
							
				//		}
				//	} else {
				//		localPath.QueueNextWayPoint();
				//		//debugFlags.Add("Skipped local way point");
				//		// try to skip one more
				//		TryToSkipLocalWayPoint();
				//		return;
				//	}
				//}
			}
		}

		/// <summary>
		/// Returns true if the given quad collides with constrained edges.
		/// </summary>
		/// <param name="projectionQuad"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private bool CheckQuadAgainstCurrentTileAndTileWithPosition
						(Vector2[] projectionQuad, Vector3 position) {
			bool collidesWithCurrentTile = currentTile != null && 
				currentTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad);
			if ( collidesWithCurrentTile ) {
				return true;
			}

			// Check against the tile that contains the given position
			Vector2Int coords = map.WorldPositionToAxialCoords(position);
			// If the tile is in the same coords as the current tile then's its the same tile and there's no need to check it
			if (!(coords == currentAxialCoords)) {
				HexTile nextTile = map.GetTile(coords);
				if (nextTile != null && nextTile.CheckQuadCollidesWithConstrainedEdges(projectionQuad)) {
					return true;
				}
			}

			return false;
		}

		private void TryToSkipGlobalWayPoint() {

			// If this is the last waypoint then no way to skip
			if (!globalPath.HasRemainingWayPoints()) {
				return;
			}

			Vector3 pos = globalPath.GetCurrentWayPoint();
			if ((pos - transform.position).magnitude < map.Metrics.tileSize * 0.9f) {
				// try to skip the current global way point
				//Vector3 pos = globalPath.GetCurrentWayPoint();
				// make a quad/box that is our hypothetical projected path from here to the target point
				Vector2[] projectionQuad = ColDet.CreateQuad(pos.JustXZ(), transform.position.JustXZ(), radius * 2.0f);
				// If the quad doesn't collide with anything on this tile then we can skip this waypoint
				if (!CheckQuadAgainstCurrentTileAndTileWithPosition(projectionQuad, pos)) {
					globalPath.QueueNextWayPoint();
					return;
				}
				//globalPath.QueueNextWayPoint();
				////debugFlags.Add("Skipped global way point");
				//// try to skip one more
				//TryToSkipGlobalWayPoint();
				//return;
			}

			

			//Vector2Int nextWaypoint = map.WorldPositionToAxialCoords(pos);

			//if ((pos - transform.position).magnitude < map.Metrics.tileSize && 
			//		nextWaypoint == currentAxialCoords) {
			//	globalPath.QueueNextWayPoint();
			//	debugFlags.Add("Skipped global way point");
			//	// try to skip one more
			//	TryToSkipGlobalWayPoint();
			//	return;
			//}
		}

		bool steering = false;

		private void SteerAwayFromObstacles() {
			//Vector3 center = transform.position + MoveVector * 0.2f;
			//Collider[] overlappingObjects = Physics.OverlapSphere(center, radius);
			RaycastHit hitInfo;

			steering = false;

			float castDistance = radius * 10.0f;

			Vector3 center = Center;
			Vector3 groundNormal = Vector3.up;
			if (Physics.Raycast(center, Vector3.down, out hitInfo, height * 0.6f,
					HexNavMeshManager.SteerLayer) ) {
				groundNormal = hitInfo.normal;
			}

			float midDistance = castDistance;
			float rightDistance = castDistance;
			float leftDistance = castDistance;

			if (Physics.Raycast(center, MoveVector, out hitInfo, castDistance,
					HexNavMeshManager.SteerLayer) ) {
				if (hitInfo.normal != groundNormal) {
					// do one more check about the blackboard's current target
					Blackboard blackboard = gameObject.GetComponent<Blackboard>();
					if ( blackboard != null && blackboard.target != null &&
						hitInfo.collider.gameObject == blackboard.target ) {
						return;
					}


					midDistance = (hitInfo.point - center).magnitude;

					float angle = CommonUtils.VectorToAngle(MoveVector.JustXZ());
					float vary = Mathf.PI * 0.125f;
					float dif = Mathf.PI * 0.25f;
					if (Physics.Raycast(center, CommonUtils.AngleToVector(angle + vary).FromXZ(),
							out hitInfo, castDistance, HexNavMeshManager.SteerLayer)) {
						rightDistance = (hitInfo.point - center).magnitude;
					}

					if (Physics.Raycast(center, CommonUtils.AngleToVector(angle - vary).FromXZ(),
							out hitInfo, castDistance, HexNavMeshManager.SteerLayer)) {
						leftDistance = (hitInfo.point - center).magnitude;
					}

					float newAngle = angle;

					if (leftDistance > rightDistance) {
						newAngle = angle - dif;
					} else {
						newAngle = angle + dif;
					}

					MoveVector = CommonUtils.AngleToVector(newAngle).FromXZ();
					steering = true;
				}



			}
		}

		Coroutine localCoroutine;

		private void ProcessLocalPath() {
			// we only want/need one process coroutine at a time
			//StopAllCoroutines();
			if ( localCoroutine!=null) {
				StopCoroutine(localCoroutine);
			}
			localCoroutine = StartCoroutine(localPath.ProcessJobCoroutine());
			// TODO: run a parallel coroutine that attempts to find the path from end to start
			//		If either of the coroutines succeeds -> success
			//		If either of the coroutines fail -> fail
			// This will sometimes help find paths sooner, but mostly it will allow the path to fail
			// instead of processing endlessly in the case that our destination is unreachable because it
			// is surrounded by walls.
			//	This isn't so much an issue with Local since the nav mesh only exists for as far as the tiles are generated.
		}

		//Coroutine globalCoroutine;
		private void ProcessGlobalPath() {

			currentDestinationAxialCoords = map.WorldPositionToAxialCoords(Destination);

			GlobalPathManager.RequestPath(currentAxialCoords, currentDestinationAxialCoords, 
				RequestPathCallback);
			
			globalPathNeedsRestart = false;
		}

		private void RequestPathCallback(PathJobResult<Vector2Int> pathResult ) {
			if (pathResult.StartNode.Position == currentAxialCoords &&
					pathResult.EndNode.Position==currentDestinationAxialCoords) {
				currentGlobalPathResult = pathResult;
				globalPath.SetPath(pathResult);
			}
			
		}

		//void OnDrawGizmos() {
		//	Gizmos.color = Color.white;
		//	if (steering) {
		//		Gizmos.color = Color.red;
		//	}
		//	Gizmos.DrawWireSphere(transform.position + MoveVector * radius * 10f, radius);
		//}

		#endregion
	}
}