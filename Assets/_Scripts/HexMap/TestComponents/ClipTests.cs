using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;
using HexMap;

namespace ClipperTests {
	using ClipperPolygon = List<IntPoint>;
	using Polygons = List<List<IntPoint>>;
	
	public class ClipTests : MonoBehaviour {

		public PolyShape sourceShape;
		public PolyShape diffShape;

		//Here we are scaling all coordinates up by 100 when they're passed to Clipper 
		//via Polygon (or Polygons) objects because Clipper no longer accepts floating  
		//point values. Likewise when Clipper returns a solution in a Polygons object, 
		//we need to scale down these returned values by the same amount before displaying.
		private float scale = 100; //or 1 or 10 or 10000 etc for lesser or greater precision.
		
		public void RunClip() {
			Polygons subjs = new Polygons();
			ClipperPolygon rect1 = new ClipperPolygon();
			for( int i = 0; i < sourceShape.PointCount; i ++ ) {
				Vector3 point = sourceShape.GetPointWorldPosition(i);
				rect1.Add(new IntPoint(point.x, point.z));
			}
			subjs.Add(rect1);

			Polygons clips = new Polygons();
			ClipperPolygon rect2 = new ClipperPolygon();
			for (int i = 0; i < diffShape.PointCount; i++) {
				Vector3 point = diffShape.GetPointWorldPosition(i);
				rect2.Add(new IntPoint(point.x, point.z));
			}
			clips.Add(rect2);

			PolyTree solution = new PolyTree();

			Clipper c = new Clipper();
			c.AddPaths(subjs, PolyType.ptSubject, true);
			c.AddPaths(clips, PolyType.ptClip, true);
			c.Execute(ClipType.ctDifference, solution);

			Debug.Log(solution.ChildCount);

			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = new List<Vector3>();

			PolyNode node = solution.GetFirst();

			while( node !=null ) {
				List<Vector3> path = new List<Vector3>();
				foreach (var point in node.Contour) {
					Vector3 v = new Vector3(point.X, transform.position.y, point.Y);
					v = transform.InverseTransformPoint(v);
					path.Add(v);
				}
				if ( node.IsHole ) {
					poly.holes.Add(path);
				} else {
					poly.outside = path;
				}
				node = node.GetNext();
			}
			
			Mesh mesh = Poly2Mesh.CreateMesh(poly);

			MeshFilter filter = GetComponent<MeshFilter>();
			filter.mesh = mesh;

			//PolyShape myShape = GetComponent<PolyShape>();
			//myShape.points.Clear();
			//foreach( var point in solution[0] ) {
			//	Vector3 v = new Vector3(point.X, transform.position.y, point.Y);
			//	v = transform.InverseTransformPoint(v);
			//	myShape.points.Add(v);
			//}
		}
	}

}
