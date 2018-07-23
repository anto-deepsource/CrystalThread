using Poly2Tri;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DelaunayTriangleExtensions {
	
	public static Vector3 Center( this DelaunayTriangle triangle ) {
		return triangle.Centroid().AsVector3();
	}

	public static Vector3 AsVector3( this  TriangulationPoint point ) {
		return new Vector3(point.Xf, 0, point.Yf);
	}

	public static Vector2 AsVector2(this TriangulationPoint point) {
		return new Vector2(point.Xf, point.Yf);
	}
}
