
using Poly2Tri;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HexMap {
	[CustomEditor(typeof(HexTile))]
	public class HexTileEditor : Editor {

		private const float handleSize = 0.04f;
		private const float pickSize = 0.06f;
		
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			HexTile tile = target as HexTile;
			
			if (GUILayout.Button("Build NavMesh")) {
				Undo.RecordObject(tile, "Build NavMesh");
				tile.BuildNavMesh();
				EditorUtility.SetDirty(tile);
			}
		}
		public void OnSceneGUI() {
			HexTile tile = target as HexTile;

			foreach( var triangle in tile.triangles ) {
				var points = new Vector3[3];

				var centroid = triangle.Centroid();
				Vector3 center = new Vector3(centroid.Xf, tile.transform.position.y, centroid.Yf);

				for( int i = 0; i < 3; i ++ ) {
					var p = triangle.Points[i];
					points[i] = new Vector3(p.Xf, tile.transform.position.y, p.Yf);
					points[i] = points[i] + (center - points[i]) * 0.1f;
					//points[i] = tile.transform.TransformPoint(points[i]);
				}
				
				Handles.color = new Color(0, 0, 1, 0.5f);
				Handles.DrawAAConvexPolygon(points);

				// draw a line from this triangle to each of its neighbors
				Handles.color = new Color(1, 1, 1, 0.8f);

				for( int i = 0; i < 3; i ++ ) {
					if ( !triangle.EdgeIsConstrained[i] ) {
						var neighbor = triangle.Neighbors[i];
						Handles.DrawLine(center, neighbor.Center());
					}
				}

				// draw a line on each of the constrained edges
				Handles.color = new Color(1, 0, 0, 0.8f);

				for (int i = 0; i < 3; i++) {
					if (triangle.EdgeIsConstrained[i]) {
						var neighbor = triangle.Neighbors[i];
						DTSweepConstraint edge;
						if ( triangle.GetEdge(i, out edge) ) {
							Handles.DrawLine(edge.P.AsVector3(), edge.Q.AsVector3());
						}
						
					}
				}
			}
			
		}

	}
}