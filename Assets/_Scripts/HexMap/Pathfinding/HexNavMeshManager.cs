using HexMap.Pathfinding;
using Poly2Tri;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	public class HexNavMeshManager : Singleton<HexNavMeshManager> {
		private HexNavMeshManager() { }

		public float maxProcessTime = 0.01f;

		public LayerMask steerLayer;

		#region Private members

		private static HexagonMaker maker;

		private static HexMap map;

		#endregion

		#region Public Static Functions

		public static HexagonMaker GetHexMaker() {
			if ( maker == null ) {
				// TOOD: come up with something better than this
				maker = GameObject.Find("HexMap").GetComponent<HexagonMaker>();
			}
			return maker;
		}

		public static HexMap GetHexMap() {
			if (map == null) {
				// TOOD: come up with something better than this
				map = GameObject.Find("HexMap").GetComponent<HexMap>();
			}
			return map;
		}

		public static GameObject TargetObject {
			get { return GetHexMap().TargetObject;  }
		}

		public static Vector2Int WorldPositionToAxialCoords( Vector3 worldPos ) {
			return GetHexMap().WorldPositionToAxialCoords(worldPos);
		}

		public static float XZPositionToHeight(Vector3 position, bool scaleByMapHeight = false) {
			return GetHexMap().XZPositionToHeight(position, scaleByMapHeight);
		}

		public static Vector3 WorldPositionPlusMapHeight( Vector3 position ) {
			return new Vector3(0, XZPositionToHeight(position, true), 0) + position;
		}

		public static float MaxProcessTime { get { return Instance.maxProcessTime; } }

		public static LayerMask SteerLayer { get { return Instance.steerLayer; } }

		public static void EnsureAboveMap( Transform target ) {
			HexagonMaker maker = GetHexMaker();
			float leastY = maker.metrics.XZPositionToHeight(target.position, true);
			if (target.position.y < leastY - 100f) {
				//target.position = new Vector3(target.position.x, leastY + 1, target.position.z);
				target.position = WorldPosToWorldPosWithGround(target.position);
				Rigidbody myBody = target.gameObject.GetComponent<Rigidbody>();
				if ( myBody !=null ) {
					myBody.velocity = new Vector3(myBody.velocity.x, 0, myBody.velocity.z);
				}
				
			}
		}

		public static PathJob<DelaunayTriangle> NewLocalJob() {
			var job = new PathJob<DelaunayTriangle>();
			job.GetNodeFromPosition = GetHexMap().GetLocalNodeByPosition;
			job.GetNodeFromArea = map.GetLocalNodeByTriangle;
			job.Heuristic = LocalHeuristic;
			job.CostFunction = LocalCostFunction;
			return job;
		}

		public static PathJob<Vector2Int> NewGlobalJob() {
			var job = new PathJob<Vector2Int>();
			job.GetNodeFromPosition = GetHexMap().GetGlobalNodeFromWorldPosition;
			job.GetNodeFromArea = map.GetGlobalNode;
			job.Heuristic = GlobalHeuristic;
			job.CostFunction = GlobalCostFunction;
			return job;
		}

		public static bool CheckPointIsPathable(Vector3 point) {
			LocalNode localNode = GetHexMap().GetLocalNodeByPosition(point);
			if (localNode == null) {
				// This will happen if the tile doesn't currently exist
				return false;
			}
			// Check that the triangle actually contains the given point
			return ColDet.PointInTriangle(localNode.Position, point.JustXZ());
		}

		/// <summary>
		/// Returns a random pathable point within the given circle.
		/// </summary>
		public static Vector3 GetAnyPointInArea( Vector3 center, float radius, int triesLeft = 30 ) {

			// don't know what to do with bad radii
			if ( radius <= 0 || triesLeft == 0) {
				return center;
			}

			// pick a random point
			Vector3 point = center +  Random.insideUnitCircle.FromXZ() * radius;

			// we can check whether the point is on a path by trying to get
			// the local node off the hexmap:
			//		-if there is not local node -> nav data for that position doesn't exist
			//		-if there is a local node and that node's triangle CONTAINS our point
			//			-> this is a point that we can navigate to
			//		-if there is a local node and that node's triangle DOES NOT contain our point
			//			-> this is NOT a point that we can navigate to

			LocalNode localNode = GetHexMap().GetLocalNodeByPosition(point);
			if (localNode == null) {
				//then we're not so worried about pathing because the whole global tile doesn't exist
				return point;
			}
			else
			if (ColDet.PointInTriangle(localNode.Position, point.JustXZ())) {
				return point;
			}
			else {
				// try to pick another point
				// TODO: this has a risk or running infinitely if NONE of the points within the circle are pathable
				return GetAnyPointInArea(center, radius, triesLeft - 1);

				//return localNode.Position.Center();
			}

			return point;

		}

		public static Vector3 WorldPosToWorldPosWithGround( Vector3 worldPos ) {
			float distance = 10000;
			RaycastHit hitRayHit;
			HexMap hexMap = GetHexMap();
			if ( Physics.Raycast(worldPos.DropY() + Vector3.up * distance, Vector3.down,
					out hitRayHit, distance * distance, ~hexMap.gameObject.layer ) ) {
				return new Vector3(worldPos.x, hitRayHit.point.y, worldPos.z);
			}
			return HexNavMeshManager.WorldPositionPlusMapHeight(worldPos);
		}

		/// <summary>
		/// Checks the nav mesh and returns true if the polyshape doesn't collide with constained edges.
		/// </summary>
		public static bool CheckIsBuildablePosition(PolyShape polyShape ) {

			var hexMap = GetHexMap();
			var hexCoords = WorldPositionToAxialCoords(polyShape.CenterOfGravity());
			var tile = hexMap.GetTile(hexCoords);

			if (tile == null) {
				return false;
			}

			foreach (var point in polyShape.GetWorldPoints()) {
				if (!CheckPointIsPathable(point)) {
					return false;
				}
			}

			// TODO: enforce that it has exactly 4 vertices
			Vector2[] quad = polyShape.GetWorldPoints2D();

			bool collides = tile.CheckQuadCollidesWithConstrainedEdges(quad);

			return !collides;
		}

		#endregion

		public static float LocalHeuristic(DelaunayTriangle start, Vector3 end) {

			//// X-Z manhatten distance from the start position to the end.
			//return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);

			//// Euclidean distance (way the crow flys)
			//return (end - start).magnitude;

			// Move along the legs of a 90/45/45 triangle between start and end.
			// Sort of if manhattan distance rotated to be the worst case from any point to any other
			// hyp = euclidean distance, a = b, h^2 = b^2 + a^2
			// 2a^2 = h^2
			// a^2 = h^2/2
			// a = sqrt( h^2/2 )
			float h2 = (end.JustXZ() - start.Center().JustXZ()).sqrMagnitude;
			float a = Mathf.Sqrt(h2 * 0.5f);
			return a * 2;
		}

		public static float GlobalHeuristic(Vector2Int start, Vector3 end) {
			Vector2Int coords = GetHexMap().WorldPositionToAxialCoords(end);
			return HexUtils.Distance(start, coords);
		}

		public static float LocalCostFunction(DelaunayTriangle position, Vector3 start) {
			return (position.Center().JustXZ() - start.JustXZ()).sqrMagnitude;
		}

		public static float GlobalCostFunction(Vector2Int position, Vector3 start) {
			Vector3 worldPosition = GetHexMap().AxialCoordsToWorldPosition(position);
			return (worldPosition.JustXZ() - start.JustXZ()).magnitude;
		}
	}
}