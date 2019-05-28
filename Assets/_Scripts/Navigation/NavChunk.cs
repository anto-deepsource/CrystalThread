using ClipperLib;
using Poly2Tri;
using System.Collections.Generic;
using UnityEngine;
using HexMap;

namespace Porter.Navigation {

	using ClipperPolygon = List<IntPoint>;
	using Polygons = List<List<IntPoint>>;

	public class NavChunk {
		//Here we are scaling all coordinates up by 100 when they're passed to Clipper 
		//via Polygon (or Polygons) objects because Clipper no longer accepts floating  
		//point values. Likewise when Clipper returns a solution in a Polygons object, 
		//we need to scale down these returned values by the same amount before displaying.
		private static float scale = 100; //or 1 or 10 or 10000 etc for lesser or greater precision.

		public HexMetrics metrics;

		public NavWeb parentWeb;

		//public Mesh sharedMesh;

		[SerializeField]
		public List<DelaunayTriangle> triangles = new List<DelaunayTriangle>();

		/// <summary>
		/// A list of any triangles that were found to have neighboring triangles on adajacent tiles.
		/// We have to keep track of this in case that other tile changes/updates 
		/// </summary>
		private List<OffTileEdgeData> offTileTriangles = new List<OffTileEdgeData>();

		/// <summary>
		/// A list of points where each pair represents an edge on the x-z plane that should be considered a wall.
		/// </summary>
		public List<Vector2> constrainedEdges = new List<Vector2>();

		/// <summary>
		/// Axial coordinates of the tile
		/// </summary>
		public int column;
		public int row;

		public List<PolyShape> staticObstacles = new List<PolyShape>();

		public HexMap.HexMap map;

		//public List<Mesh> reusableMeshes = new List<Mesh>();

		private bool _navMeshBuilt = false;
		public bool NavMeshBuilt {
			get { return _navMeshBuilt; }
			set {
				if (_navMeshBuilt && !value) {
					AlertAdjacentTilesOfNavMeshChange();
				}
				_navMeshBuilt = value;
			}
		}

		public NavChunk(HexMap.HexMap map, HexMetrics metrics, int column, int row) {
			this.map = map;
			this.metrics = metrics;
			this.column = column;
			this.row = row;
		}
		
		public void BuildNavMesh() {
			// Start with the full size and shape of the hexagon minus any walls
			Polygons startShape = GetHexagon();

			// Clip each obstacle

			Polygons obstaclePolys = new Polygons();
			foreach (var obstacle in staticObstacles) {
				obstacle.AppendPolygons(ref obstaclePolys, scale);

			}

			//PolyTree obstacles = new PolyTree();

			//Clipper c = new Clipper();
			//c.AddPaths(obstaclePolys, PolyType.ptClip, true);
			//c.Execute(ClipType.ctUnion, obstacles, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

			PolyTree solution = new PolyTree();

			Clipper c = new Clipper();
			c.AddPaths(startShape, PolyType.ptSubject, true);
			c.AddPaths(obstaclePolys, PolyType.ptClip, true);
			c.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
			//c.Execute(ClipType.ctDifference, solution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

			// Triangulate the resulting polygons

			List<List<PolygonPoint>> outsidePoints = new List<List<PolygonPoint>>();
			List<Polygon> holes = new List<Polygon>();

			triangles.Clear();

			PolyNode node = solution.GetFirst();

			while (node != null) {
				if (node.IsHole) {
					holes.Add(new Poly2Tri.Polygon(ConvertPoints(node.Contour)));
				}
				else {
					outsidePoints.Add(ConvertPoints(node.Contour));
				}
				node = node.GetNext();
			}

			// TODO: if there is more than one set of points in outsidePoints we can make some assumptions
			// about which edges are traversable
			foreach (var points in outsidePoints) {
				Poly2Tri.Polygon poly = new Poly2Tri.Polygon(points);

				// Convert each of the holes
				foreach (Polygon hole in holes) {
					poly.AddHole(hole);
				}

				// Triangulate it!  Note that this may throw an exception if the data is bogus.
				try {
					DTSweepContext tcx = new DTSweepContext();
					tcx.PrepareTriangulation(poly);
					DTSweep.Triangulate(tcx);
					tcx = null;
				}
				catch (System.Exception e) {
					//UnityEngine.Profiling.Profiler.Exit(profileID);
					throw e;
				}

				triangles.AddRange(poly.Triangles);
			}

			// try to link all these triangles with any nearby tiles
			ValidateTriangleNeighbors();
			CacheConstrainedEdges();

			NavMeshBuilt = true;
		}

		public void ValidateTriangleNeighbors() {
			//offTileTriangles.Clear();
			//foreach( var neighorChunk in parentWeb.Search()) { 
			////foreach (var direction in HexDirectionUtils.All()) {
			////	if (!map.IsWallAt(column, row, direction)) {
			//		//HexTile neighborChunk = map.GetTile(HexUtils.MoveFrom(column, row, direction));
					
			//		if (neighborChunk == null || !neighborChunk.NavMeshBuilt) {
			//			continue;
			//		}
			//		bool messedWithTheirEdges = false;

			//		foreach (var myTriangle in triangles) {
			//			for (int i = 0; i < 3; i++) {
			//				// we only need to check edges that are currently constrained
			//				if (myTriangle.EdgeIsConstrained[i]) {
			//					DTSweepConstraint edge;
			//					if (myTriangle.GetEdge(i, out edge)) {

			//						foreach (var theirTriangle in neighborChunk.triangles) {
			//							int ip = theirTriangle.IndexOf(edge.P, 0.1f);
			//							if (ip != -1) {
			//								int iq = theirTriangle.IndexOf(edge.Q, 0.1f);
			//								if (iq != -1) {
			//									// then these two triangles share an edge
			//									int index = theirTriangle.EdgeIndex(ip, iq);

			//									if (index != -1) {
			//										myTriangle.SetNeighborOnEdge(i, theirTriangle, index);
			//										map.ResetLocalNodesEdgesMaybe(theirTriangle);

			//										// Store this fact so that if this tile's nav mesh ever changes we
			//										// can unlink them from us
			//										offTileTriangles.Add(new OffTileEdgeData() {
			//											myTriangle = myTriangle,
			//											edgeIndex = i,
			//											theirTriangle = theirTriangle,
			//											theirEdgeIndex = index,
			//											neighborChunk = neighborChunk
			//										});

			//										// let that tile know that we messed with its edges
			//										messedWithTheirEdges = true;

			//										break;
			//									}


			//								}
			//							}
			//						}
			//					}
			//				}
			//			}
			//		}

			//		// let that tile know that we messed with its edges
			//		if (messedWithTheirEdges) {
			//			neighborChunk.CacheConstrainedEdges();
			//		}
			////	}
			////}


		}

		public void CacheConstrainedEdges() {
			constrainedEdges.Clear();
			// do one more pass through the triangles and remember any constrained edges
			foreach (var myTriangle in triangles) {
				for (int i = 0; i < 3; i++) {
					// we only need to check edges that are currently constrained
					if (myTriangle.EdgeIsConstrained[i]) {
						DTSweepConstraint edge;
						if (myTriangle.GetEdge(i, out edge)) {
							constrainedEdges.Add(edge.P.AsVector2());
							constrainedEdges.Add(edge.Q.AsVector2());
						}
					}
				}
			}
		}

		/// <summary>
		/// This may not be necessary since we don't bother checking, during our rebuild/linking to them,
		/// checking whether their triangle already has a neighbor on that edge.
		/// </summary>
		private void AlertAdjacentTilesOfNavMeshChange() {
			foreach (var data in offTileTriangles) {
				data.myTriangle.RemoveNeighborOnEdge(data.edgeIndex, data.theirTriangle, data.theirEdgeIndex);
				data.neighborChunk.CacheConstrainedEdges();
			}
		}

		public Polygons GetHexagon() {
			Polygons subjs = new Polygons();
			ClipperPolygon rect1 = new ClipperPolygon();
			foreach (var corner in HexCornerUtils.AllCorners()) {
				bool isWallLeft = map.IsWallAt(column, row, corner.GetDirection());
				bool isWallRight = map.IsWallAt(column, row, corner.GetDirection().Last());
				bool isWallAway = map.IsWallAt(HexUtils.MoveFrom(column, row, corner.GetDirection()), corner.GetDirection().Last2());
				// if there are walls all around
				if (isWallLeft && isWallRight) {
					Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
					Vector3 point = pos + HexUtils.CornerPosition(corner.GetInt()) *
						metrics.tileSize * metrics.tileInnerRadiusPercent;
					//point = transform.TransformPoint(point);
					rect1.Add(new IntPoint(point.x * scale, point.z * scale));
				}
				else if (!isWallLeft && !isWallRight) {
					if (isWallAway) {
						// add a point for the outside at the bridge point
						Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						Vector3 point = pos + BridgePoint(metrics, corner, corner.Last(),
							1, metrics.outerBridgePercent);
						//Vector3 point = HexUtils.CornerPosition(corner.GetInt()) * metrics.tileSize;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
						// add a point for the inside
						pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						point = pos + HexUtils.CornerPosition(corner.GetInt()) *
								metrics.tileSize * metrics.tileInnerRadiusPercent;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
						// add another point for the outside
						pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						point = pos + BridgePoint(metrics, corner, corner.Next(),
							1, metrics.outerBridgePercent);
						//point = HexUtils.CornerPosition(corner.GetInt()) * metrics.tileSize;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
					}
					else {
						Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						Vector3 point = pos + HexUtils.CornerPosition(corner.GetInt()) * metrics.tileSize;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
					}

				}
				else {
					// one of the sides, but not the other is a wall
					if (isWallLeft) {
						// add a point for the outside at the bridge point
						Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						Vector3 point = pos + BridgePoint(metrics, corner, corner.Last(),
							1, metrics.outerBridgePercent);
						//Vector3 point = HexUtils.CornerPosition(corner.GetInt()) * metrics.tileSize;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
						// add a point for the inside
						pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						point = pos + HexUtils.CornerPosition(corner.GetInt()) *
								metrics.tileSize * metrics.tileInnerRadiusPercent;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));

					}
					else {
						// add a point for the inside
						Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						Vector3 point = pos + HexUtils.CornerPosition(corner.GetInt()) *
								metrics.tileSize * metrics.tileInnerRadiusPercent;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
						// add another point for the outside
						pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);
						point = pos + BridgePoint(metrics, corner, corner.Next(),
							1, metrics.outerBridgePercent);
						//point = HexUtils.CornerPosition(corner.GetInt()) * metrics.tileSize;
						//point = transform.TransformPoint(point);
						rect1.Add(new IntPoint(point.x * scale, point.z * scale));
					}
				}


			}
			subjs.Add(rect1);

			return subjs;
		}

		private static List<PolygonPoint> ConvertPoints(List<IntPoint> points) {
			int count = points.Count;
			List<PolygonPoint> result = new List<PolygonPoint>(count);
			for (int i = 0; i < count; i++) {
				IntPoint p = points[i];
				PolygonPoint pp = new PolygonPoint(p.X / scale, p.Y / scale);
				result.Add(pp);
			}
			return result;
		}

		public static Vector3 BridgePoint(HexMetrics metrics, HexCorner fromCorner, HexCorner towardCorner,
				float ringPercentage, float bridgePercentage) {
			Vector3 cornerVertex = HexUtils.CornerPosition((int)fromCorner) * metrics.tileSize;

			Vector3 nextCornerPoint = HexUtils.CornerPosition(towardCorner.GetInt()) * metrics.tileSize;

			Vector3 vector = nextCornerPoint - cornerVertex;
			float bridgeMod = 0.5f * (1f - bridgePercentage);
			Vector3 bridgePoint = cornerVertex + vector * bridgeMod;

			return bridgePoint * ringPercentage;
		}

		public bool CheckQuadCollidesWithConstrainedEdges(Vector2[] quad) {
			if (!NavMeshBuilt) {
				BuildNavMesh();
			}
			// we can collision detect this against any of the constrained triangle edges in this tile
			for (int i = 0; i < constrainedEdges.Count; i += 2) {
				// take the points two at a time to make an edge
				Vector2 P = constrainedEdges[i];
				Vector2 Q = constrainedEdges[i + 1];

				// if our path collides with any of these edges, we can't just walk straight there
				if (ColDet.SegmentAndQuadrilateral(
						quad[0],
						quad[1],
						quad[2],
						quad[3],
						 P, Q)
					) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Stores the references to a triangle that connects to a triangle on an adjacent tile.
		/// </summary>
		private struct OffTileEdgeData {
			public DelaunayTriangle myTriangle;
			public int edgeIndex;
			public DelaunayTriangle theirTriangle;
			public int theirEdgeIndex;
			public NavChunk neighborChunk;
		}
	}
}