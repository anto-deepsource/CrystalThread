using ClipperLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	using ClipperPolygon = List<IntPoint>;
	using Polygons = List<List<IntPoint>>;

	public class PolyShape : MonoBehaviour {

		public List<Vector3> points = new List<Vector3>();

		public Vector3 GetPoint(int index) {
			return points[index];
		}

		public Vector3 GetPointWorldPosition(int index) {
			return transform.TransformPoint(points[index]);
		}

		public void SetPoint(int index, Vector3 value) {
			points[index] = value;
		}

		public Vector2[] GetPoints2D() {
			Vector2[] vertices = new Vector2[PointCount];
			for (int i = 0; i < PointCount; i++) {
				Vector3 v = points[i];
				vertices[i] = new Vector2(v.x, v.z);
			}
			return vertices;
		}

		public Polygons Polygons() {
			Polygons polys = new Polygons();
			ClipperPolygon cp = new ClipperPolygon();
			for (int i = 0; i < PointCount; i++) {
				Vector3 point = GetPointWorldPosition(i);
				cp.Add(new IntPoint(point.x, point.z));
			}
			polys.Add(cp);

			return polys;
		}

		public void AppendPolygons( ref Polygons polys , float scale ) {
			ClipperPolygon cp = new ClipperPolygon();
			// corrent winding
			if ( ColDet.Sign( points[0], points[1], points[2] ) >0 ) {
				for (int i = 0; i < PointCount; i++) {
					Vector3 point = GetPointWorldPosition(i);
					cp.Add(new IntPoint(point.x * scale, point.z * scale));
				}
			} else {
				for (int i = PointCount-1; i >=0 ; i--) {
					Vector3 point = GetPointWorldPosition(i);
					cp.Add(new IntPoint(point.x * scale, point.z * scale));
				}
			}
			
			
			polys.Add(cp);
		}

		public int PointCount { get { return points.Count; } }

		public void AddPoint() {
			Vector3 newPoint = Vector3.left;
			if (PointCount > 0) {
				newPoint += points[points.Count - 1];
			}
			points.Add(newPoint);
		}

		public void RemovePoint(int index) {
			Vector3 item = points[index];
			points.Remove(item);
		}

	}

}
