
using Poly2Tri;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexMap {
	[CustomEditor(typeof(HexTile))]
	public class HexTileEditor : Editor {

		private const float handleSize = 0.04f;
		private const float pickSize = 0.06f;

		private static bool drawDebugTriangles = true;
		private static bool drawDebugTriangleConnections = false;
		private static bool drawTriangleEdges = true;
		private static bool draweConstrainedEdges = true;

		private static Color debugDrawColor = new Color(0, 0, 1, 0.5f);

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			HexTile tile = target as HexTile;
			
			if (GUILayout.Button("Build NavMesh")) {
				Undo.RecordObject(tile, "Build NavMesh");
				tile.BuildNavMesh();
				EditorUtility.SetDirty(tile);
			}

			EditorGUI.BeginChangeCheck();
			drawDebugTriangles = GUILayout.Toggle(drawDebugTriangles, "Draw Debug Triangles");
			if ( drawDebugTriangles) {
				debugDrawColor = EditorGUILayout.ColorField("Triangle Color", debugDrawColor );
				drawDebugTriangleConnections = GUILayout.Toggle(drawDebugTriangleConnections, "Draw Triangle Connections");
				drawTriangleEdges = GUILayout.Toggle(drawTriangleEdges, "Draw Triangle Edges");
				draweConstrainedEdges = GUILayout.Toggle(draweConstrainedEdges, "Draw Triangle Edges");
			}

			if (EditorGUI.EndChangeCheck()) {
				SceneView.RepaintAll();
			}
		}
		public void OnSceneGUI() {
			HexTile tile = target as HexTile;
			
			if ( drawDebugTriangles) {
				foreach (var triangle in tile.triangles) {
					var points = new Vector3[3];

					var centroid = triangle.Centroid();
					Vector3 center = new Vector3(centroid.Xf, tile.transform.position.y, centroid.Yf);

					for (int i = 0; i < 3; i++) {
						var p = triangle.Points[i];
						points[i] = new Vector3(p.Xf, tile.transform.position.y, p.Yf);
						if (drawTriangleEdges) {
							var direction = (center - points[i]).normalized;
							points[i] = points[i] + direction * 0.3f;
						}
						points[i] = HexNavMeshManager.WorldPosToWorldPosWithGround(points[i]);
						
						//points[i] = tile.transform.TransformPoint(points[i]);
					}

					Handles.color = debugDrawColor;
					Handles.DrawAAConvexPolygon(points);

					if (drawDebugTriangleConnections) {
						// draw a line from this triangle to each of its neighbors
						Handles.color = new Color(1, 1, 1, 0.8f);

						for (int i = 0; i < 3; i++) {
							if (!triangle.EdgeIsConstrained[i]) {
								var neighbor = triangle.Neighbors[i];
								Handles.DrawLine(center, neighbor.Center());
							}
						}
					}
					
					if ( draweConstrainedEdges) {
						// draw a line on each of the constrained edges
						Handles.color = new Color(1, 0, 0, 0.8f);

						for (int i = 0; i < 3; i++) {
							if (triangle.EdgeIsConstrained[i]) {
								var neighbor = triangle.Neighbors[i];
								DTSweepConstraint edge;
								if (triangle.GetEdge(i, out edge)) {
									var start = points[i] = HexNavMeshManager.WorldPosToWorldPosWithGround(edge.P.AsVector3());
									var end = points[i] = HexNavMeshManager.WorldPosToWorldPosWithGround(edge.Q.AsVector3());
									Handles.DrawLine(start, end);
								}

							}
						}
					}
					
				}
			}

			
			
		}

	}
}