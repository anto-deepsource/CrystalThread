using System;
using System.Collections.Generic;
using UnityEngine;
namespace HexMap {
	public class MeshBuilder {

		[NonSerialized] public List<Vector3> vertices;
		[NonSerialized] public List<int> triangles;
		[NonSerialized] public List<Color> colors;
		[NonSerialized] public List<Vector2> uvs;
		[NonSerialized] public List<Vector2> uvs2;

		public Mesh mesh;

		private bool readyToBuild = false;

		public MeshBuilder(string name = "New Mesh") {
			ClearAndSetup(name);
		}

		public void ClearAndSetup(string name = "New Mesh", Mesh recycleMesh = null) {
			if ( recycleMesh == null ) {
				if ( !Application.isPlaying) {
					mesh = new Mesh();
				} else {
					mesh = MeshPool.Get();
				}
			} else {
				mesh = recycleMesh;
			}
			
			//mesh = new Mesh();
			mesh.name = name;

			vertices = ListPool<Vector3>.Get();
			triangles = ListPool<int>.Get();
			colors = ListPool<Color>.Get();
			uvs = ListPool<Vector2>.Get();
			uvs2 = ListPool<Vector2>.Get();

			readyToBuild = true;
		}


		public Mesh Build() {
			if (!readyToBuild) {
				throw new Exception("Mesh Builder not ready to build.");
			}
			// Return the lists to the list pool
			mesh.SetVertices(vertices);
			ListPool<Vector3>.Add(vertices);

			mesh.SetTriangles(triangles, 0);
			ListPool<int>.Add(triangles);
			mesh.RecalculateNormals();

			mesh.SetColors(colors);
			ListPool<Color>.Add(colors);

			mesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);

			mesh.SetUVs(1, uvs2);
			ListPool<Vector2>.Add(uvs2);

			mesh.RecalculateNormals();

			readyToBuild = false;

			return mesh;
		}

		#region Hexagon

		private void AddHexPointData(
			HexMetrics metrics,
			Vector3 localPos,
			Vector3 worldCenterPos,
			Color color,
			float gradient
		) {
			vertices.Add(localPos);

			//UVS
			Vector2 uv = metrics.XZPositionToUV(worldCenterPos + localPos);
			uvs.Add(uv);

			// Colors
			colors.Add(color);

			// Gradients
			uvs2.Add(new Vector2(gradient, 0));
		}

		private void AddHexWallPointData(
			Vector3 localPos,
			Vector2 uv,
			Color color
		) {
			vertices.Add(localPos);

			//UVS
			uvs.Add(uv);

			// Colors
			colors.Add(color);
		}

		public static Vector3 BridgePoint(HexMetrics metrics, HexCorner fromCorner, HexCorner towardCorner, 
				float ringPercentage, float bridgePercentage, Vector3 centerVertex, float[] cornerHeights ) {
			Vector3 cornerVertex = HexUtils.CornerPosition((int)fromCorner) * metrics.tileSize;
			cornerVertex = cornerVertex + Vector3.up * cornerHeights[fromCorner.GetInt()] * metrics.mapHeight;

			Vector3 nextCornerPoint = HexUtils.CornerPosition(towardCorner.GetInt()) * metrics.tileSize;
			nextCornerPoint = nextCornerPoint + Vector3.up * cornerHeights[towardCorner.GetInt()] * metrics.mapHeight;

			Vector3 vector = nextCornerPoint - cornerVertex;
			float bridgeMod = 0.5f * (1f - bridgePercentage);
			Vector3 bridgePoint = cornerVertex + vector * bridgeMod;

			vector = centerVertex - bridgePoint;
			return bridgePoint + vector * (1f - ringPercentage);
		}

		private Vector3 SpokePoint( HexMetrics metrics, HexCorner corner, float ringPercentage, float centerHeight, float[] cornerHeights) {
			// Base the height of the 'spokes' on the straight line from the center out to the corner of the whole tile
			Vector3 cornerVertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize * ringPercentage;
			float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * ringPercentage;
			return cornerVertex + Vector3.up * (vertexHeight * metrics.mapHeight);
		}

		/// <summary>
		/// Setups, creates, and builds the walls hexagon mesh.
		/// </summary>
		/// <param name="metrics"></param>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public Mesh WallsHexagon(HexMetrics metrics, HexMap hexMap, int column, int row, Mesh recycleMesh = null) {

			ClearAndSetup("Walls Hexagon Tile", recycleMesh);

			Vector3 center = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);

			float centerHeight = metrics.XZPositionToHeight(center);

			float[] cornerHeights = new float[6];
			foreach (var corner in HexCornerUtils.AllCorners()) {
				Vector3 vertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize;
				cornerHeights[corner.GetInt()] = metrics.XZPositionToHeight(center + vertex);
			}

			#region Base Hexagon

			
			// Add vertex data for each corner first in order to reduce redundancy (18 vertices -> 7)
			// Not making triangles yet
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// Vertice w/ height
				Vector3 cornerPoint = SpokePoint(metrics, corner, metrics.tileInnerRadiusPercent, centerHeight, cornerHeights);
				float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * metrics.tileInnerRadiusPercent;
				AddHexPointData(metrics, cornerPoint, center, Color.white, vertexHeight);
			}

			// Add the center point last just so that we can use the HexCorner enums as indexes and the center is @ 6
			int Center = 6; // just for readability
							// UV
			uvs.Add(metrics.XZPositionToUV(center));

			// Vertice
			Vector3 cenVertex = new Vector3(0, centerHeight * metrics.mapHeight, 0);
			vertices.Add(cenVertex);

			// Color
			colors.Add(Color.white);

			// Gradient
			uvs2.Add(new Vector2(centerHeight, 0));

			// Make triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				triangles.Add((int)corner.Next()); // Next first because of normals
				triangles.Add((int)corner);
				triangles.Add(Center);
			}

			#endregion

			// The starting index of the points for the outer ring
			int ringStartIndex = vertices.Count; // should be 7 here
			
			// ----------Add another ring to the hexagon
			#region First Ring

			// Add vertex data for each outside corner
			// Not making triangles yet
			// Add three points for each corner: the actual outside corner and then the two bridge points
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				HexDirection direction = corner.GetDirection();

				float theta = HexUtils.CornerAngle((int)corner);

				// We need to account for the three walls connecting to this corner:
				// the wall on this hex going left,
				// the wall on this hex going right,
				// and the wall adjacent to this hex running out and away
				bool leftWall = hexMap.IsWallAt(column, row, corner.GetDirection());
				bool rightWall = hexMap.IsWallAt(column, row, corner.GetDirection().Last());
				// move to the hex left of the given corner, turn two directions CW and check that wall
				bool awayWall = hexMap.IsWallAt(HexUtils.MoveFrom(column, row, corner.GetDirection()), corner.GetDirection().Last2());

				

				// -------Actual Corner---------- -
				{
					Vector3 cornerPoint = SpokePoint(metrics, corner, metrics.tileOuterRadiusPercent, centerHeight, cornerHeights);
					// raise the inner ring corner if either of the sides are alls
					float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * metrics.tileOuterRadiusPercent;
					if (leftWall || rightWall) {
						cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall || rightWall ? Color.black : Color.white;
					AddHexPointData(metrics, cornerPoint, center, color, vertexHeight);
				}

				//// ------- CCW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint( metrics, corner, corner.Next(), metrics.tileOuterRadiusPercent,
						metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);

					if (leftWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
				}

				//// ------- CW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), metrics.tileOuterRadiusPercent,
						metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);

					if (rightWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = rightWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
					
				}
			}

			// Make triangles, accounting for one corner and one side at a time

			// we actually need quads, so pairs of triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// inside point is [corner], outside point is [corner * 3 + ORStart]
				int outerCorner = (int)corner * 3 + ringStartIndex + 0;
				// left bridge point is [corner * 3 + ORStart + 1], right bridge point is [corner * 3 + ORStart + 2]
				int rightBridgePoint = (int)corner * 3 + ringStartIndex + 2;
				int leftBridgePoint = (int)corner * 3 + ringStartIndex + 1;
				// corner.Next() is the point CCW
				int nextRightBridgePoint = (int)corner.Next() * 3 + ringStartIndex + 2;

				// first half of the quad
				triangles.Add((int)corner);
				triangles.Add((int)corner.Next());
				triangles.Add(nextRightBridgePoint);

				// second half of the quad
				triangles.Add(nextRightBridgePoint);
				triangles.Add(leftBridgePoint);
				triangles.Add((int)corner);

				// span from the quad to the corner with a quad
				triangles.Add(leftBridgePoint);
				triangles.Add(outerCorner);
				triangles.Add((int)corner);

				// second part of corner span
				triangles.Add(rightBridgePoint);
				triangles.Add((int)corner);
				triangles.Add(outerCorner);
			}

			#endregion

			#region Second Ring
			// ---------- Add another ring to the hexagon

			// The starting index of the points for the outer ring
			int ORStart = vertices.Count;
			
			// Add vertex data for each outside corner
			// Not making triangles yet
			// Add three points for each corner: the actual outside corner and then the two bridge points
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				HexDirection direction = corner.GetDirection();

				float theta = HexUtils.CornerAngle((int)corner);

				// We need to account for the three walls connecting to this corner:
				// the wall on this hex going left,
				// the wall on this hex going right,
				// and the wall adjacent to this hex running out and away
				bool leftWall = hexMap.IsWallAt(column, row, corner.GetDirection());
				bool rightWall = hexMap.IsWallAt(column, row, corner.GetDirection().Last());
				// move to the hex left of the given corner, turn two directions CW and check that wall
				bool awayWall = hexMap.IsWallAt(HexUtils.MoveFrom(column, row, corner.GetDirection()), corner.GetDirection().Last2());
				
				// -------Actual Corner---------- -
				{
					Vector3 cornerPoint = SpokePoint(metrics, corner, 1, centerHeight, cornerHeights);
					// raise the inner ring corner if either of the sides are alls
					float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * metrics.tileOuterRadiusPercent;
					if (leftWall || rightWall || awayWall) {
						cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall || rightWall || awayWall ? Color.black : Color.white;
					AddHexPointData(metrics, cornerPoint, center, color, vertexHeight);
				}

				//// INNER Bridge points
				//// ------- CCW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1, metrics.outerBridgePercent, cenVertex, cornerHeights);

					if (leftWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
				}

				//// ------- CW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1, metrics.outerBridgePercent, cenVertex, cornerHeights);

					if (rightWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = rightWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
				}

				////OUTER Bridge Points
				//// ------- CCW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1, metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);

					if (leftWall || rightWall || awayWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall || rightWall || awayWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
				}

				//// ------- CW Bridge Point -----------
				{
					Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1, metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);

					if (leftWall || rightWall || awayWall) {
						bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
					}
					Color color = leftWall || rightWall || awayWall ? Color.black : Color.white;
					AddHexPointData(metrics, bridgePoint, center, color, cornerHeights[corner.GetInt()]);
				}
			}

			// Make triangles, accounting for one corner and one side at a time

			// we actually need quads, so pairs of triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// inside point is [corner*3 + iRStart], outside point is [corner * 5 + ORStart]
				int innerCorner = (int)corner * 3 + ringStartIndex;
				int nextInnerCorner = (int)corner.Next() * 3 + ringStartIndex;
				int outerCorner = (int)corner * 5 + ORStart + 0;
				// left inner bridge point is [corner * 3 + iRStart + 1], right  inner bridge point is [corner * 3 + iRStart + 2]
				int rightInnerBridgePoint = (int)corner * 3 + ringStartIndex + 2;
				int leftInnerBridgePoint = (int)corner * 3 + ringStartIndex + 1;
				int nextRightInnerBridgePoint = (int)corner.Next() * 3 + ringStartIndex + 2;
				// left outer bridge point is [corner * 5 + ORStart + 1], right  outer bridge point is [corner * 5 + ORStart + 2]
				int rightOuterBridgePoint = (int)corner * 5 + ORStart + 2;
				int leftOuterBridgePoint = (int)corner * 5 + ORStart + 1;
				int nextRightOuterBridgePoint = (int)corner.Next() * 5 + ORStart + 2;

				int leftOuterOuterBridgePoint = (int)corner * 5 + ORStart + 3;
				int rightOuterOuterBridgePoint = (int)corner * 5 + ORStart + 4;

				// first half of the quad
				triangles.Add(leftInnerBridgePoint);
				triangles.Add(nextRightInnerBridgePoint);
				triangles.Add(nextRightOuterBridgePoint);

				//second half of the quad
				triangles.Add(nextRightOuterBridgePoint);
				triangles.Add(leftOuterBridgePoint);
				triangles.Add(leftInnerBridgePoint);

				triangles.Add(innerCorner);
				triangles.Add(leftInnerBridgePoint);
				triangles.Add(leftOuterBridgePoint);

				triangles.Add(rightInnerBridgePoint);
				triangles.Add(innerCorner);
				triangles.Add(rightOuterBridgePoint);

				triangles.Add(rightOuterBridgePoint);
				triangles.Add(innerCorner);
				triangles.Add(rightOuterOuterBridgePoint);

				triangles.Add(leftOuterBridgePoint);
				triangles.Add(leftOuterOuterBridgePoint);
				triangles.Add(innerCorner);

				triangles.Add(innerCorner);
				triangles.Add(outerCorner);
				triangles.Add(rightOuterOuterBridgePoint);

				triangles.Add(outerCorner);
				triangles.Add(innerCorner);
				triangles.Add(leftOuterOuterBridgePoint);
			}
			#endregion


			return Build();
		}

		public Mesh ElevatedTileHexagon(HexMetrics metrics, HexMap hexMap, int column, int row, Mesh recycleMesh = null) {

			ClearAndSetup("Hexagon Tile With Elevation", recycleMesh);

			Vector3 center = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);

			int elevation = hexMap.GetElevationAt(column, row);

			float elevationHeight = metrics.elevationStepHeight * elevation;

			Color baseColor = hexMap.tileset.GetColorForElevation(elevation, metrics.maxElevation);

			float centerHeight = metrics.XZPositionToHeight(center);

			float[] cornerHeights = new float[6];
			foreach (var corner in HexCornerUtils.AllCorners()) {
				Vector3 vertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize;
				cornerHeights[corner.GetInt()] = metrics.XZPositionToHeight(center + vertex);
			}

			#region Base Hexagon

			float baseHexagonPercentage = metrics.tileInnerRadiusPercent;
			
			// Add vertex data for each corner first in order to reduce redundancy (18 vertices -> 7)
			// Not making triangles yet
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// Vertice w/ height
				//Vector3 cornerPoint = SpokePoint(metrics, corner, baseHexagonPercentage, centerHeight, cornerHeights);
				// Base the height of the 'spokes' on the straight line from the center out to the corner of the whole tile
				Vector3 cornerVertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize * baseHexagonPercentage;

				float projectedSpokeHeight = (cornerHeights[corner.GetInt()] - centerHeight)
												* baseHexagonPercentage;
				float vertexHeight = centerHeight + projectedSpokeHeight;
				Vector3 cornerPoint = cornerVertex + Vector3.up * (vertexHeight * metrics.mapHeight + elevationHeight);
				float textureGradient = vertexHeight;
				AddHexPointData(metrics, cornerPoint, center, baseColor, textureGradient);
			}

			// Add the center point last just so that we can use the HexCorner enums as indexes and the center is @ 6
			int Center = 6; // just for readability
							// UV
			uvs.Add(metrics.XZPositionToUV(center));

			// Vertice
			Vector3 centerVertex = new Vector3(0, centerHeight * metrics.mapHeight + elevationHeight, 0);
			vertices.Add(centerVertex);

			// Color
			colors.Add(baseColor);

			// Gradient
			uvs2.Add(new Vector2(centerHeight, 0));

			// Make triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				triangles.Add((int)corner.Next()); // Next first because of normals
				triangles.Add((int)corner);
				triangles.Add(Center);
			}

			#endregion

			// The starting index of the points for the outer ring
			int ringStartIndex = vertices.Count; // should be 7 here

			// ----------Add another ring to the hexagon
			#region First Ring

			float firstRingPercentage = metrics.tileOuterRadiusPercent;

			// Add vertex data for each outside corner
			// Not making triangles yet
			// Add three points for each corner: the actual outside corner and then the two bridge points
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				HexDirection direction = corner.GetDirection();

				float theta = HexUtils.CornerAngle((int)corner);

				//// We need to account for the three walls connecting to this corner:
				//// the wall on this hex going left,
				//// the wall on this hex going right,
				//// and the wall adjacent to this hex running out and away
				//bool leftWall = hexMap.IsWallAt(column, row, corner.GetDirection());
				//bool rightWall = hexMap.IsWallAt(column, row, corner.GetDirection().Last());
				//// move to the hex left of the given corner, turn two directions CW and check that wall
				//bool awayWall = hexMap.IsWallAt(HexUtils.MoveFrom(column, row, corner.GetDirection()), corner.GetDirection().Last2());

				
				
				// -------Actual Corner---------- -
				{
					Vector3 cornerVertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize * firstRingPercentage;

					float nextNeighborElevationHeight = metrics.elevationStepHeight *
						hexMap.GetElevationAt(HexUtils.MoveFrom(column, row, direction));
					float lastNeighborElevationHeight = metrics.elevationStepHeight *
						hexMap.GetElevationAt(HexUtils.MoveFrom(column, row, direction.Last()));
					float heightModFromElevation = 
						(elevationHeight + nextNeighborElevationHeight + lastNeighborElevationHeight) * 0.33f;

					float projectedSpokeHeight = (cornerHeights[corner.GetInt()] - centerHeight)
													* firstRingPercentage;
					float vertexHeight = centerHeight + projectedSpokeHeight;
					Vector3 cornerPoint = cornerVertex + 
						Vector3.up * (vertexHeight * metrics.mapHeight + heightModFromElevation);
					float textureGradient = vertexHeight;

					bool isWall = hexMap.IsWallAt(column, row, direction.Last());
					Color slopeColor = baseColor;
					if (isWall) {
						slopeColor = hexMap.tileset.steepSlopeColor;
					}

					AddHexPointData(metrics, cornerPoint, center, slopeColor, textureGradient);

				}

				//// ------- CCW Bridge Point -----------
				{
					var fromCorner = corner;
					var towardCorner = corner.Next();
					var bridgePercentage = metrics.tileInnerRadiusPercent;

					//Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), firstRingPercentage,
					//	metrics.tileInnerRadiusPercent, centerVertex, cornerHeights);

					float nextNeighborElevationHeight = metrics.elevationStepHeight *
						hexMap.GetElevationAt(HexUtils.MoveFrom(column, row, direction));
					float heightModFromElevation =
						(elevationHeight + nextNeighborElevationHeight ) * 0.5f;


					Vector3 cornerVertex = HexUtils.CornerPosition((int)fromCorner) * metrics.tileSize;
					cornerVertex = cornerVertex + 
						Vector3.up * (cornerHeights[fromCorner.GetInt()] * metrics.mapHeight + heightModFromElevation);

					Vector3 nextCornerPoint = HexUtils.CornerPosition(towardCorner.GetInt()) * metrics.tileSize;
					nextCornerPoint = nextCornerPoint + 
						Vector3.up * (cornerHeights[towardCorner.GetInt()] * metrics.mapHeight + heightModFromElevation);

					Vector3 vector = nextCornerPoint - cornerVertex;
					float bridgeMod = 0.5f * (1f - bridgePercentage);
					Vector3 bridgePoint = cornerVertex + vector * bridgeMod;

					vector = centerVertex - bridgePoint;
					bridgePoint = bridgePoint + vector * (1f - firstRingPercentage);

					bool isWall = hexMap.IsWallAt(column, row, direction);
					Color slopeColor = baseColor;
					if (isWall) {
						slopeColor = hexMap.tileset.steepSlopeColor;
					}
					
					AddHexPointData(metrics, bridgePoint, center, slopeColor, cornerHeights[corner.GetInt()]);
				}

				//// ------- CW Bridge Point -----------
				{
					//Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), firstRingPercentage,
					//	metrics.tileInnerRadiusPercent, centerVertex, cornerHeights);

					var fromCorner = corner;
					var towardCorner = corner.Last();
					var bridgePercentage = metrics.tileInnerRadiusPercent;

					//Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), firstRingPercentage,
					//	metrics.tileInnerRadiusPercent, centerVertex, cornerHeights);
					
					float neighborElevationHeight = metrics.elevationStepHeight *
						hexMap.GetElevationAt(HexUtils.MoveFrom(column, row, direction.Last()));
					float heightModFromElevation =
						(elevationHeight + neighborElevationHeight) * 0.5f;

					Vector3 cornerVertex = HexUtils.CornerPosition((int)fromCorner) * metrics.tileSize;
					cornerVertex = cornerVertex + 
						Vector3.up * (cornerHeights[fromCorner.GetInt()] * metrics.mapHeight + heightModFromElevation);

					Vector3 nextCornerPoint = HexUtils.CornerPosition(towardCorner.GetInt()) * metrics.tileSize;
					nextCornerPoint = nextCornerPoint + Vector3.up * (cornerHeights[towardCorner.GetInt()] * metrics.mapHeight + heightModFromElevation);

					Vector3 vector = nextCornerPoint - cornerVertex;
					float bridgeMod = 0.5f * (1f - bridgePercentage);
					Vector3 bridgePoint = cornerVertex + vector * bridgeMod;

					vector = centerVertex - bridgePoint;
					bridgePoint = bridgePoint + vector * (1f - firstRingPercentage);

					bool isWall = hexMap.IsWallAt(column, row, direction.Last());
					Color slopeColor = baseColor;
					if (isWall) {
						slopeColor = hexMap.tileset.steepSlopeColor;
					}
					AddHexPointData(metrics, bridgePoint, center, slopeColor, cornerHeights[corner.GetInt()]);

				}
			}

			// Make triangles, accounting for one corner and one side at a time

			// we actually need quads, so pairs of triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// inside point is [corner], outside point is [corner * 3 + ORStart]
				int outerCorner = (int)corner * 3 + ringStartIndex + 0;
				// left bridge point is [corner * 3 + ORStart + 1], right bridge point is [corner * 3 + ORStart + 2]
				int rightBridgePoint = (int)corner * 3 + ringStartIndex + 2;
				int leftBridgePoint = (int)corner * 3 + ringStartIndex + 1;
				// corner.Next() is the point CCW
				int nextRightBridgePoint = (int)corner.Next() * 3 + ringStartIndex + 2;

				// first half of the quad
				triangles.Add((int)corner);
				triangles.Add((int)corner.Next());
				triangles.Add(nextRightBridgePoint);

				// second half of the quad
				triangles.Add(nextRightBridgePoint);
				triangles.Add(leftBridgePoint);
				triangles.Add((int)corner);

				// span from the quad to the corner with a quad
				triangles.Add(leftBridgePoint);
				triangles.Add(outerCorner);
				triangles.Add((int)corner);

				// second part of corner span
				triangles.Add(rightBridgePoint);
				triangles.Add((int)corner);
				triangles.Add(outerCorner);
			}

			#endregion



			return Build();
		}

		/// <summary>
		/// Setups, creates, and builds the base hexagon mesh that always fills an entire tile.
		/// </summary>
		public Mesh BaseHexagon(HexMetrics metrics, HexMap hexMap, int column, int row, Mesh recycleMesh = null) {

			ClearAndSetup("Hexagon Tile", recycleMesh);

			Vector3 center = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);

			float centerHeight = metrics.XZPositionToHeight(center);

			float[] cornerHeights = new float[6];
			foreach (var corner in HexCornerUtils.AllCorners()) {
				Vector3 vertex = HexUtils.CornerPosition((int)corner) * metrics.tileSize;
				cornerHeights[corner.GetInt()] = metrics.XZPositionToHeight(center + vertex);
			}

			#region Base Hexagon


			// Add vertex data for each corner first in order to reduce redundancy (18 vertices -> 7)
			// Not making triangles yet
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				// Vertice w/ height
				Vector3 cornerPoint = SpokePoint(metrics, corner, 1, centerHeight, cornerHeights);
				float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * 1;
				AddHexPointData(metrics, cornerPoint, center, Color.white, vertexHeight);
			}

			// Add the center point last just so that we can use the HexCorner enums as indexes and the center is @ 6
			int Center = 6; // just for readability
							// UV
			uvs.Add(metrics.XZPositionToUV(center));

			// Vertice
			Vector3 cenVertex = new Vector3(0, centerHeight * metrics.mapHeight, 0);
			vertices.Add(cenVertex);

			// Color
			colors.Add(Color.white);

			// Gradient
			uvs2.Add(new Vector2(centerHeight, 0));

			// Make triangles
			foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				triangles.Add((int)corner.Next()); // Next first because of normals
				triangles.Add((int)corner);
				triangles.Add(Center);
			}

			#endregion

			#region Collider Fill-Out Bottom Hexagon

			//float thickness = 30f;

			//// The starting index of the points for the outer ring
			//int ringStartIndex = vertices.Count; // should be 7 here

			//// Add vertex data for each corner first in order to reduce redundancy (18 vertices -> 7)
			//// Not making triangles yet
			//foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
			//	// Vertice w/ height
			//	Vector3 cornerPoint = SpokePoint(metrics, corner, 1, centerHeight, cornerHeights);
			//	cornerPoint = cornerPoint + Vector3.down * thickness;
			//	float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight) * 1;
			//	AddHexPointData(metrics, cornerPoint, center, Color.white, vertexHeight);
			//}
			
			//// UV
			//uvs.Add(metrics.XZPositionToUV(center));

			//// Vertice
			//Vector3 cenBottomVertex = new Vector3(0, centerHeight * metrics.mapHeight - thickness, 0);
			//vertices.Add(cenBottomVertex);

			//// Color
			//colors.Add(Color.white);

			//// Gradient
			//uvs2.Add(new Vector2(centerHeight, 0));

			//// Make triangles
			//foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
			//	triangles.Add((int)corner.Next() + ringStartIndex); // Next first because of normals
			//	triangles.Add(Center + ringStartIndex);
			//	triangles.Add((int)corner + ringStartIndex);
				
			//}

			//#endregion

			//#region Create Faces around the hexagon between the two rings

			//// we actually need quads, so pairs of triangles
			//foreach (HexCorner corner in HexCornerUtils.AllCorners()) {
				
			//	// first half of the quad
			//	triangles.Add((int)corner.Next() + ringStartIndex);
			//	triangles.Add((int)corner);
			//	triangles.Add((int)corner.Next());

			//	//second half of the quad
			//	triangles.Add((int)corner + ringStartIndex);
			//	triangles.Add((int)corner);
			//	triangles.Add((int)corner.Next() + ringStartIndex);

			//}

			#endregion


			return Build();
		}

		/// <summary>
		/// Setups, creates, and builds the walls hexagon mesh.
		/// </summary>
		/// <param name="metrics"></param>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <returns></returns>
		public Mesh WallMesh(HexMetrics metrics, HexMap hexMap, int column, int row,
				HexCorner corner, Mesh recycleMesh = null) {

			ClearAndSetup("Wall Mesh", recycleMesh);

			var direction = corner.GetDirection();

			// We need to account for the three walls connecting to this corner:
			// the wall on this hex going left,
			// the wall on this hex going right,
			// and the wall adjacent to this hex running out and away
			bool leftWall = hexMap.IsWallAt(column, row, direction);
			bool rightWall = hexMap.IsWallAt(column, row, direction.Last());
			// move to the hex left of the given corner, turn two directions CW and check that wall
			bool awayWall = hexMap.IsWallAt(HexUtils.MoveFrom(column, row, direction), direction.Last2());

			// depending on where there are walls determines what type of mesh-corner we make
			// if none of these are walls then don't make anything
			if ( !leftWall && !rightWall && !awayWall ) {
				return Build();
			}
			
			Vector3 center = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);

			float centerHeight = metrics.XZPositionToHeight(center);
			Vector3 cenVertex = new Vector3(0, centerHeight * metrics.mapHeight, 0);

			float[] cornerHeights = new float[6];
			foreach (var c in HexCornerUtils.AllCorners()) {
				Vector3 vertex = HexUtils.CornerPosition((int)c) * metrics.tileSize;
				cornerHeights[c.GetInt()] = metrics.XZPositionToHeight(center + vertex);
			}
			
			if (!leftWall && !rightWall && awayWall) {
				CreateWallEndCap(metrics, corner, center, cenVertex, centerHeight, cornerHeights);
			} else 
			if ( leftWall && rightWall ) {
				CreateFullWall(metrics, corner, center, cenVertex, centerHeight, cornerHeights);
			} else
			if (!leftWall && rightWall) {
				CreateJustRightWall(metrics, corner, center, cenVertex, centerHeight, cornerHeights);
			}
			else
			if ( leftWall && !rightWall) {
				CreateJustLeftWall(metrics, corner, center, cenVertex, centerHeight, cornerHeights);
			}

			return Build();
		}

		/// <summary>
		/// For when the given corner has no walls on the left and right, but a all on the 'away' axis
		/// </summary>
		private void CreateWallEndCap(HexMetrics metrics, HexCorner corner,
				Vector3 center, Vector3 cenVertex, float centerHeight, float[] cornerHeights) {

			float uPercentage = 0.1f;

			// Vertex 0
			// Closest Ground Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileInnerRadiusPercent, centerHeight, cornerHeights);
				Color color = Color.white;
				Vector2 uv = new Vector2(0.5f * uPercentage, 0f);
				AddHexWallPointData(cornerPoint, uv, color);
			}
			
			// Vertex 1
			// Closest Top-of-wall Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileOuterRadiusPercent, centerHeight, cornerHeights);
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0.5f * uPercentage, 0.5f);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			#region Right Side

			// Vertex 2
			// Closest Gound CW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileOuterRadiusPercent,
					metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(0.75f * uPercentage, 0f);
				AddHexWallPointData(bridgePoint, uv, color);

			}
			
			// Vertex 3
			// Closest Top-of-wall CW Bridge Point
			{
				// This point defines a lot about how the corner looks
				float radiusOffset = (1f - metrics.tileOuterRadiusPercent) * 0.5f;
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileOuterRadiusPercent + radiusOffset,
					metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0.75f * uPercentage, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 4
			// Farthest Ground CW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1,
					metrics.outerBridgePercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(1f * uPercentage, 0f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 5
			// Farthest Top-of-wall CW Bridge Point
			{
				// This point needs to be matched up with the adjacent tile
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1,
					metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1f * uPercentage, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 6
			// Furthest Top-of-wall corner Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner, 1, centerHeight, cornerHeights);
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1f * uPercentage, 1f);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			triangles.Add(0);
			triangles.Add(1);
			triangles.Add(2);
			
			triangles.Add(2);
			triangles.Add(1);
			triangles.Add(3);

			triangles.Add(2);
			triangles.Add(3);
			triangles.Add(4);

			triangles.Add(4);
			triangles.Add(3);
			triangles.Add(5);

			triangles.Add(3);
			triangles.Add(6);
			triangles.Add(5);

			triangles.Add(1);
			triangles.Add(6);
			triangles.Add(3);

			#endregion

			int leftStartIndex = vertices.Count;

			#region Left Side

			// Vertex 7
			// Closest Gound CW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileOuterRadiusPercent,
					metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(.25f * uPercentage, 0f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 8
			// Closest Top-of-wall CW Bridge Point
			{
				// This point defines a lot about how the corner looks
				float radiusOffset = (1f - metrics.tileOuterRadiusPercent) * 0.5f;
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileOuterRadiusPercent + radiusOffset,
					metrics.tileInnerRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(.25f * uPercentage, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 9
			// Farthest Ground CW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1,
					metrics.outerBridgePercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(0f * uPercentage, 0f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 10
			// Farthest Top-of-wall CCW Bridge Point
			{
				// This point needs to be matched up with the adjacent tile
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1,
					metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0f * uPercentage, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);
			}
			
			triangles.Add(0);
			triangles.Add(7);
			triangles.Add(1);

			triangles.Add(7);
			triangles.Add(8);
			triangles.Add(1);

			triangles.Add(7);
			triangles.Add(9);
			triangles.Add(8);

			triangles.Add(9);
			triangles.Add(10);
			triangles.Add(8);

			triangles.Add(8);
			triangles.Add(10);
			triangles.Add(6);

			triangles.Add(1);
			triangles.Add(8);
			triangles.Add(6);

			#endregion
		}


		/// <summary>
		/// For when the given corner has walls on both the left and right
		/// </summary>
		private void CreateFullWall(HexMetrics metrics, HexCorner corner,
				Vector3 center, Vector3 cenVertex, float centerHeight, float[] cornerHeights) {

			// Vertex 0
			// Closest Ground Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileInnerRadiusPercent, centerHeight, cornerHeights);
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 0f);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			// Vertex 1
			// Closest Top-of-wall Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileOuterRadiusPercent, centerHeight, cornerHeights);
				// raise the inner ring corner if either of the sides are alls
				float vertexHeight = centerHeight + (cornerHeights[corner.GetInt()] - centerHeight)
					* metrics.tileOuterRadiusPercent;
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 0.5f);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			#region Right Side

			// Vertex 2
			// Closest Gound CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileInnerRadiusPercent, 0, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 3
			// Closest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileOuterRadiusPercent, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 4
			// Furthest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					1, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 1f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 5
			// Furthest Top-of-wall Corner Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					1, centerHeight, cornerHeights);
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 1f);
				AddHexWallPointData(cornerPoint, uv, color);

			}

			triangles.Add(0);
			triangles.Add(1);
			triangles.Add(2);

			triangles.Add(2);
			triangles.Add(1);
			triangles.Add(3);
			
			triangles.Add(3);
			triangles.Add(1);
			triangles.Add(4);

			triangles.Add(4);
			triangles.Add(1);
			triangles.Add(5);

			#endregion

			#region Left Side

			// Vertex 6
			// Closest Gound CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileInnerRadiusPercent, 0, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(-1f, 0f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 7
			// Closest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileOuterRadiusPercent, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(-1f, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 8
			// Furthest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					1, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(-1f, 1f);
				AddHexWallPointData(bridgePoint, uv, color);

			}
			
			triangles.Add(0);
			triangles.Add(6);
			triangles.Add(1);
			
			triangles.Add(6);
			triangles.Add(7);
			triangles.Add(1);

			triangles.Add(7);
			triangles.Add(8);
			triangles.Add(1);

			triangles.Add(8);
			triangles.Add(5);
			triangles.Add(1);

			#endregion

		}


		/// <summary>
		/// For when the given corner has walls on just the right side
		/// </summary>
		private void CreateJustRightWall(HexMetrics metrics, HexCorner corner,
				Vector3 center, Vector3 cenVertex, float centerHeight, float[] cornerHeights) {
			// Vertex 0
			// Farthest Ground CCW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1,
					metrics.outerBridgePercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(0, 0f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 1
			// Farthest Top-of-wall CCW Bridge Point
			{
				// This point needs to be matched up with the adjacent tile
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(), 1,
					metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, .5f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 2
			// Closest Ground Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileInnerRadiusPercent, centerHeight, cornerHeights);
				Color color = Color.white;
				Vector2 uv = new Vector2(1f- metrics.tileInnerRadiusPercent, 0f);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			// Vertex 3
			// Closest Gound CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileInnerRadiusPercent, 0, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 4
			// Closest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					metrics.tileOuterRadiusPercent, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 5
			// Furthest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(),
					1, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 1f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 6
			// Furthest Top-of-wall Corner Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					1, centerHeight, cornerHeights);
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 1f);
				AddHexWallPointData(cornerPoint, uv, color);

			}

			triangles.Add(0);
			triangles.Add(1);
			triangles.Add(2);

			triangles.Add(2);
			triangles.Add(4);
			triangles.Add(3);

			triangles.Add(2);
			triangles.Add(1);
			triangles.Add(4);

			triangles.Add(4);
			triangles.Add(1);
			triangles.Add(5);

			triangles.Add(5);
			triangles.Add(1);
			triangles.Add(6);
		}

		/// <summary>
		/// For when the given corner has walls on just the left side
		/// </summary>
		private void CreateJustLeftWall(HexMetrics metrics, HexCorner corner,
				Vector3 center, Vector3 cenVertex, float centerHeight, float[] cornerHeights) {
			// Vertex 0
			// Farthest Ground CW Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1,
					metrics.outerBridgePercent, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0f);
				AddHexWallPointData( bridgePoint, uv, color);
			}

			// Vertex 1
			// Farthest Top-of-wall CW Bridge Point
			{
				// This point needs to be matched up with the adjacent tile
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Last(), 1,
					metrics.tileOuterRadiusPercent, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);
			}

			// Vertex 2
			// Closest Ground Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					metrics.tileInnerRadiusPercent, centerHeight, cornerHeights);
				Color color = Color.white;
				Vector2 uv = new Vector2(metrics.tileInnerRadiusPercent, 0);
				AddHexWallPointData(cornerPoint, uv, color);
			}

			// Vertex 3
			// Closest Gound CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileInnerRadiusPercent, 0, cenVertex, cornerHeights);

				Color color = Color.white;
				Vector2 uv = new Vector2(0, 0);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 4
			// Closest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					metrics.tileOuterRadiusPercent, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 0.5f);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 5
			// Furthest Top-of-wall CW Mid Bridge Point
			{
				Vector3 bridgePoint = BridgePoint(metrics, corner, corner.Next(),
					1, 0, cenVertex, cornerHeights);
				bridgePoint = bridgePoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(0, 1);
				AddHexWallPointData(bridgePoint, uv, color);

			}

			// Vertex 6
			// Furthest Top-of-wall Corner Point
			{
				Vector3 cornerPoint = SpokePoint(metrics, corner,
					1, centerHeight, cornerHeights);
				cornerPoint = cornerPoint + Vector3.up * metrics.wallHeight;
				Color color = Color.white;
				Vector2 uv = new Vector2(1, 1);
				AddHexWallPointData(cornerPoint, uv, color);

			}

			triangles.Add(0);
			triangles.Add(2);
			triangles.Add(1);

			triangles.Add(2);
			triangles.Add(3);
			triangles.Add(4);

			triangles.Add(2);
			triangles.Add(4);
			triangles.Add(1);

			triangles.Add(4);
			triangles.Add(5);
			triangles.Add(1);

			triangles.Add(5);
			triangles.Add(6);
			triangles.Add(1);
		}

		#endregion
	}
}