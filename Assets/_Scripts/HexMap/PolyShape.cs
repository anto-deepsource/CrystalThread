using ClipperLib;
using Poly2Tri;
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

		public IEnumerable<Vector3> GetWorldPoints() {
			foreach ( var point in points ) {
				yield return transform.TransformPoint(point);
			}
		}

		/// <summary>
		/// Returns the indexed point in world position, but only on the x-z plane
		/// </summary>
		public Vector2 PointXZ(int index) {
			return transform.TransformPoint(points[index]).JustXZ();
		}

		public void SetPoint(int index, Vector3 value) {
			points[index] = value;
			triangles = null;
		}

		public Vector2[] GetPoints2D() {
			Vector2[] vertices = new Vector2[PointCount];
			for (int i = 0; i < PointCount; i++) {
				Vector3 v = points[i];
				vertices[i] = new Vector2(v.x, v.z);
			}
			return vertices;
		}

		public Vector2[] GetWorldPoints2D() {
			Vector2[] vertices = new Vector2[PointCount];
			for (int i = 0; i < PointCount; i++) {
				Vector3 v = transform.TransformPoint(points[i]);
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

		/// <summary>
		/// Caches the results of triangulating the polyshape
		/// </summary>
		List<DelaunayTriangle> triangles;
		Vector3 triangulatedPosition;

		public List<DelaunayTriangle> GetTriangles() {

			if ( triangles == null || transform.position!= triangulatedPosition) {
				List<PolygonPoint> outsidePoints = new List<PolygonPoint>();
				for (int i = 0; i < PointCount; i++) {
					Vector3 point = GetPointWorldPosition(i);
					PolygonPoint pp = new PolygonPoint(point.x, point.z);
					outsidePoints.Add(pp);
				}

				Poly2Tri.Polygon poly = new Poly2Tri.Polygon(outsidePoints);
				
				// Triangulate it!  Note that this may throw an exception if the data is bogus.
				try {
					DTSweepContext tcx = new DTSweepContext();
					tcx.PrepareTriangulation(poly);
					DTSweep.Triangulate(tcx);
					tcx = null;
				} catch (System.Exception e) {
					//UnityEngine.Profiling.Profiler.Exit(profileID);
					throw e;
				}

				triangles = new List<DelaunayTriangle>(poly.Triangles);
				triangulatedPosition = transform.position;
			}
			return triangles;
		}

		public int PointCount { get { return points.Count; } }

		public void AddPoint() {
			Vector3 newPoint = Vector3.left;
			if (PointCount > 0) {
				newPoint += points[points.Count - 1];
			}
			points.Add(newPoint);
			triangles = null;
		}

		public void AddPoint(Vector3 point ) {
			points.Add(point);
			triangles = null;
		}

		public void Clear() {
			points.Clear();
			triangles = null;
		}

		public void RemovePoint(int index) {
			Vector3 item = points[index];
			points.Remove(item);
			triangles = null;
		}

		/// <summary>
		/// Returns a list of any Colliders intersecting this polyshape.
		/// Uses PolyShape collision detection, which mostly ignores y.
		/// Creates poly shapes on collider objects if it needs to
		/// </summary>
		public GameObject[] Cast(LayerMask layer) {
			// start by doing an AABB box cast for any gameobjects in an overlapping box
			// this will be as much y-axis checking that we'll do
			//var center = transform.position;
			var center = CenterOfGravity();
			var extents = AABBHalfExtents();
			Collider[] colliders = Physics.OverlapBox(center, extents, transform.rotation);
			if ( colliders.Length == 0 ) {
				return new GameObject[0];
			}

			List<GameObject> results = new List<GameObject>();

			foreach ( var collider in colliders ) {
				PolyShape otherShape = Utilities.PolyShape(collider.gameObject);
				if ( ColDet.PolyShapeAndPolyShape( otherShape, this ) ) {
					results.Add(collider.gameObject);
				}
			}
			
			return results.ToArray();
		}

		public bool CheckAgainstObject(GameObject target) {
			PolyShape otherShape = Utilities.PolyShape(target);
			return ColDet.PolyShapeAndPolyShape(otherShape, this);
		}

		public Vector3 CenterOfGravity() {
			Vector3 result = Vector3.zero;
			foreach( var point in points ) {
				result = result + point;
			}
			return transform.TransformPoint( result / points.Count );
		}

		public Vector3 AABBHalfExtents() {
			Vector3 min = points[0];
			Vector3 max = points[0];
			foreach (var point in points) {
				min = Vector3.Min(min, point);
				max = Vector3.Max(max, point);
			}
			return (max - min) + Vector3.up;
		}

		public void CreateCircle( Vector3 center, float radius, int slices ) {
			for (int i = 0; i < slices; i++) {
				float theta = Mathf.PI * 2f / (float)slices * i;
				float x = Mathf.Cos(theta) * radius;
				float z = Mathf.Sin(theta) * radius;
				Vector3 point = center + x * Vector3.right + z * Vector3.forward;
				AddPoint(point);
			}
		}

		public void CreateAABB( Vector3 center, float halfExtentX, float halfExtentZ ) {
			Vector3 point = center + halfExtentX * Vector3.right + halfExtentZ * Vector3.forward;
			AddPoint(point);

			point = center + halfExtentX * Vector3.right + halfExtentZ * Vector3.back;
			AddPoint(point);

			point = center + halfExtentX * Vector3.left + halfExtentZ * Vector3.back;
			AddPoint(point);

			point = center + halfExtentX * Vector3.left + halfExtentZ * Vector3.forward;
			AddPoint(point);
		}

		/// <summary>
		/// Creates a polyshape around a RadiusIndicator, resulting in a semi-circle
		/// </summary>
		public void CreateSemiCircle(Vector3 center, float radius, float value, float startAngle, int slices) {
			AddPoint(center);

			// negative values for the segments or radius don't make sense and cause problems
			slices = Mathf.Max(3, slices);
			radius = Mathf.Max(0, radius);

			float twoPI = Mathf.PI * 2.0f;
			float delta = twoPI / (float)slices;
			
			Vector3 leg;
			Vector2 uvCenter = new Vector2(0.5f, 0.5f);
			for (int i = 0; (float)(i - 1) / (float)slices <= value; i++) {
				float theta = Mathf.Deg2Rad * startAngle - Mathf.Min(i * delta, value * twoPI);

				leg = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));

				Vector3 point = center + leg * radius;
				AddPoint(point);
			}
		}
	}

}
