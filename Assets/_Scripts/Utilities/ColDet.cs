
using HexMap;
using Poly2Tri;
using System.Collections.Generic;
using UnityEngine;

public static class ColDet {

	/// <summary>
	/// Returns 2 times the signed triangle area. The result is positive if
	/// ABC is ccw, negative if ABC is cw, zero if ABC is degenerate.
	/// </summary>
	public static float Sign(Vector2 A, Vector2 B, Vector2 C) {
		return (A.x - C.x) * (B.y - C.y) - (B.x - C.x) * (A.y - C.y);
	}

	public static bool PointInTriangle(DelaunayTriangle triangle, Vector2 point ) {
		Vector2 p1 = triangle.Points[0].AsVector2();
		Vector2 p2 = triangle.Points[1].AsVector2();
		Vector2 p3 = triangle.Points[2].AsVector2();

		return PointInTriangle(p1, p2, p3, point);
	}

	public static bool PointInTriangle( Vector2 A, Vector2 B, Vector2 C, Vector2 P ) {
		bool b1, b2, b3;

		b1 = Sign(P, A, B) < 0.0f;
		b2 = Sign(P, B, C) < 0.0f;
		b3 = Sign(P, C, A) < 0.0f;

		return ((b1 == b2) && (b2 == b3));
	}

	public static Vector2 JustXZ( this Vector3 point ) {
		return new Vector2(point.x, point.z);
	}

	public static Vector3 JustY(this Vector3 point) {
		return new Vector3(0,point.y,0);
	}

	public static Vector3 DropY( this Vector3 point ) {
		return new Vector3(point.x, 0, point.z);
	}

	public static Vector3 FromXZ(this Vector2 point) {
		return new Vector3(point.x, 0, point.y);
	}
	
	public static float PointLineDistance(Vector2 A, Vector2 B, Vector2 P ) {
		// http://paulbourke.net/geometry/pointlineplane/
		float top = (P.x - A.x) * (B.x - A.x) + (P.y - A.y) * (B.y - A.y);
		float den = (B - A).sqrMagnitude;
		float u = top/den;
		// if u < 0 or u > 1 then the given point is closer to one of the end points
		if ( u < 0 ) {
			return (P - A).magnitude;
		} else
		if ( u > 1 ) {
			return (P - B).magnitude;
		}
		Vector2 v = A + (B - A) * u;
		return (P - v).magnitude;
	}

	public static float PointLineDistance(Vector2 A, Vector2 B, Vector2 P, out Vector2 result) {
		// http://paulbourke.net/geometry/pointlineplane/
		float top = (P.x - A.x) * (B.x - A.x) + (P.y - A.y) * (B.y - A.y);
		float den = (B - A).sqrMagnitude;
		float u = top / den;
		// if u < 0 or u > 1 then the given point is closer to one of the end points
		if (u < 0) {
			result = A;
			return (P - A).magnitude;
		} else
		if (u > 1) {
			result = B;
			return (P - B).magnitude;
		}
		Vector2 v = A + (B - A) * u;
		float distance = (P - v).magnitude;
		Vector2 slope = (B - A).normalized;
		result = P + new Vector2(-slope.y, slope.x) * distance;
		return distance;
	}

	public static float DistanceToClosestPointOnTriangle(DelaunayTriangle triangle, Vector2 point) {
		Vector2 A = triangle.Points[0].AsVector2();
		Vector2 B = triangle.Points[1].AsVector2();
		Vector2 C = triangle.Points[2].AsVector2();

		// right off the bat, if the point is inside the triangle then it is itself the closest point
		if ( PointInTriangle( A, B, C, point) ) {
			// and the distance is 0
			return 0;
		}

		// check the distance between the point and each of the three edges and return the smallest one
		float min = PointLineDistance(A, B, point);
		min = Mathf.Min(min, PointLineDistance(B, C, point));
		min = Mathf.Min(min, PointLineDistance(C, A, point));

		return min;
	}

	/// <summary>
	/// 2D. if the actual contact point is not needed, use the method that does not take an out result,
	/// it is more efficient.
	/// </summary>
	public static float DistanceToClosestPointOnTriangle(DelaunayTriangle triangle, Vector2 point, out Vector2 result) {
		Vector2 A = triangle.Points[0].AsVector2();
		Vector2 B = triangle.Points[1].AsVector2();
		Vector2 C = triangle.Points[2].AsVector2();

		return DistanceToClosestPointOnTriangle(A, B, C, point, out result);
	}

	/// <summary>
	/// 2D. if the actual contact point is not needed, use the method that does not take an out result,
	/// it is more efficient.
	/// </summary>
	public static float DistanceToClosestPointOnTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 point, out Vector2 result) {
		
		// right off the bat, if the point is inside the triangle then it is itself the closest point
		if (PointInTriangle(A, B, C, point)) {
			// and the distance is 0
			result = point;
			return 0;
		}

		// check the distance between the point and each of the three edges and return the smallest one
		float min = PointLineDistance(A, B, point, out result);

		Vector2 temp;

		float distance = PointLineDistance(B, C, point, out temp);
		if (distance < min) {
			min = distance;
			result = temp;
		}
		distance = PointLineDistance(C, A, point, out temp);
		if (distance < min) {
			min = distance;
			result = temp;
		}
		return min;
	}


	/// <summary>
	/// Returns true if the line segment from A to B intersects with the line segment from C to D
	/// </summary>
	public static bool SegmentAndSegment( Vector2 A, Vector2 B, Vector2 C, Vector2 D ) {
		float a1 = Sign(A, B, D); // compute the winding of the triangle ABD
		float a2 = Sign(A, B, C); // compute the other triangle, must have opposite signs to intersect

		if ( a1 * a2 < 0f ) {
			// compute signs for a and b with respect to segment cd
			float a3 = Sign(C, D, A);
			// Since area is constant a1 - a2 = a3 - a4
			float a4 = a3 + a2 - a1;
			// points a and b on different sides of cd
			if ( a3 * a4 < 0f ) {
				return true;
			}
		}

		return false;
	}
	
	public static bool SegmentAndTriangle(DelaunayTriangle triangle, Vector2 P, Vector2 Q) {
		Vector2 A = triangle.Points[0].AsVector2();
		Vector2 B = triangle.Points[1].AsVector2();
		Vector2 C = triangle.Points[2].AsVector2();

		return SegmentAndTriangle(A, B, C, P, Q);
	}

	/// <summary>
	/// Returns true if the 2d line segment P to Q intersects with the triangle ABC
	/// </summary>
	public static bool SegmentAndTriangle( Vector2 A, Vector2 B, Vector2 C, Vector2 P, Vector2 Q ) {
		// We can simple check if either end point is inside the triangle
		// but this doesn't account for if the segment starts and ends outside the triangle
		// but lays across it
		if (PointInTriangle(A, B, C, P) || PointInTriangle(A, B, C, Q)) {
			return true;
		}

		// if any of the edges of the triangle intersect with line segment
		if (SegmentAndSegment(A, B, P, Q))
			return true;
		if (SegmentAndSegment(B, C, P, Q))
			return true;
		if (SegmentAndSegment(C, A, P, Q))
			return true;

		return false;
	}

	public static bool TriangleAndTriangle(DelaunayTriangle triangle, Vector2 U, Vector2 V, Vector2 W) {
		Vector2 A = triangle.Points[0].AsVector2();
		Vector2 B = triangle.Points[1].AsVector2();
		Vector2 C = triangle.Points[2].AsVector2();

		return TriangleAndTriangle(A, B, C, U, V, W);
	}

	/// <summary>
	/// Returns true if the triangle ABC intersects with the triangle UVW
	/// </summary>
	public static bool TriangleAndTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 U, Vector2 V, Vector2 W) {
		// For one triangle to intersect with another, one or more sides on the first triangle must intersect with 
		// one or more sides of the second triangle OR one of the triangles is completely contained within the other
		
		// Check each side of triangle ABC with UV, VW, UW
		// this will also catch if UVW is completely contained within ABC
		if ( SegmentAndTriangle(A,B,C, U, V ) )
			return true;
		if (SegmentAndTriangle(A, B, C, V, W))
			return true;
		if (SegmentAndTriangle(A, B, C, U, W))
			return true;

		// still could be that ABC is completely contained with UVW
		// we can simply check a single point
		if ( PointInTriangle(U,V,W, A) )
			return true;

		return false;
	}

	/// <summary>
	/// Returns true if the 2d line segment P to Q intersects with the quad ABCD
	/// </summary>
	public static bool SegmentAndQuadrilateral(Vector2 A, Vector2 B, Vector2 C, Vector2 D, Vector2 P, Vector2 Q) {
		// simply tests the two triangles ABC and CDA
		return SegmentAndTriangle(A, B, C, P, Q) || SegmentAndTriangle(C, D, A, P, Q);
	}

	/// <summary>
	/// Returns 4 points that make up a quadrilateral with one edge centered at A and has the given width,
	/// and an opposite, parrellel, and similiar edge centered at B.
	/// </summary>
	public static Vector2[] CreateQuad( Vector2 A, Vector2 B, float width ) {
		// the normalized slope from A to B, which ends up being the slope of the other two edges
		Vector2 perpSlope = (B - A).normalized;

		// Go perpendicular from the last vector to get the slope of the first edge/line
		Vector2 slope = new Vector2(-perpSlope.y, perpSlope.x);

		float halfWidth = width * 0.5f;

		return new Vector2[] {
			// first point is A + slope * halfWidth
			A + slope * halfWidth,
			B + slope * halfWidth,
			B - slope * halfWidth,
			A - slope * halfWidth,
		};
	}

	public static float DistanceFromPointToCircle( Vector2 P, Vector2 center, float radius ) {
		// returns 0 if the point is within the circle
		float distance = (P - center).magnitude;
		if ( distance < radius ) {
			return 0;
		}

		// otherwise, the closest point on the circle to the point is the center + radius towards the point
		// so the distance from the point to the circle is distance - radius
		return distance - radius;
	}

	public static bool CircleAndCircle( Vector2 centerA, float radiusA, Vector2 centerB, float radiusB ) {
		float distance = (centerB - centerA).sqrMagnitude;
		// try to avoid doing sqr roots
		float radii = (radiusA + radiusB) * (radiusA + radiusB);
		return distance < radii;
	}

	/// <summary>
	/// Ignores the y axis; treats all points as if they only exist on the x-z plane.
	/// </summary>
	public static bool PolyShapeAndPoint(PolyShape shape, Vector2 point) {
		// depending on how many points the polyshape has, we can be more efficient
		switch (shape.PointCount) {
			case 1:
				// both shapes are only single points:
				return point == shape.PointXZ(0);
			case 2:
				// is a line segment:
				return PointLineDistance(
					shape.PointXZ(0),
					shape.PointXZ(1),
					point) == 0f;
			case 3:
				// is a triangle
				return PointInTriangle(
					shape.PointXZ(0),
					shape.PointXZ(1),
					shape.PointXZ(2),
					point);
			default:
				// is a quad or more complex, we basically have to triangulate the shape and
				// check the point against any of the triangles
				var triangles = shape.GetTriangles();
				foreach( var triangle in triangles ) {
					if ( PointInTriangle(triangle, point ) ) {
						return true;
					}
				}
				break;
		}

		return false;
	}

	public static bool PolyShapeAndSegment(PolyShape shape, Vector2 P, Vector2 Q ) {
		// depending on how many points the polyshape has, we can be more efficient
		switch (shape.PointCount) {
			case 1:
				// one shape is a point, the other is a line segment:
				return PointLineDistance(
					P,
					Q,
					shape.PointXZ(0)) == 0f;
			case 2:
				// both shapes are line segments
				return SegmentAndSegment(
					P,
					Q,
					shape.PointXZ(0),
					shape.PointXZ(1));
			case 3:
				// one shape is a line segment, the other is a triangle
				return SegmentAndTriangle(
					shape.PointXZ(0),
					shape.PointXZ(1),
					shape.PointXZ(2),
					P,
					Q);
			default:
				// is a quad or more complex, we basically have to triangulate the shape and
				// check the segment against any of the triangles
				var triangles = shape.GetTriangles();
				foreach (var triangle in triangles) {
					if (SegmentAndTriangle(triangle, P, Q )) {
						return true;
					}
				}
				break;

		}

		return false;
	}

	public static bool PolyShapeAndTriangle(PolyShape shape, DelaunayTriangle triangle) {
		Vector2 A = triangle.Points[0].AsVector2();
		Vector2 B = triangle.Points[1].AsVector2();
		Vector2 C = triangle.Points[2].AsVector2();

		return PolyShapeAndTriangle(shape, A, B, C );
	}
	
	public static bool PolyShapeAndTriangle(PolyShape shape, Vector2 A, Vector2 B, Vector2 C) {
		// depending on how many points the polyshape has, we can be more efficient
		switch (shape.PointCount) {
			case 1:
				// one shape is a triangle, the other is a single point
				return PointInTriangle(
					A,
					B,
					C,
					shape.PointXZ(0));
			case 2:
				// one shape is a triangle, the other is a line segment
				return SegmentAndTriangle(
					A,
					B,
					C,
					shape.PointXZ(0),
					shape.PointXZ(1));
			case 3:
				// both shapes are triangles
				return ColDet.TriangleAndTriangle(
					A,
					B,
					C,
					shape.PointXZ(0),
					shape.PointXZ(1),
					shape.PointXZ(2));
			default:
				// is a quad or more complex, we basically have to triangulate the shape and
				// check the ABC triangle against any of the triangles
				var triangles = shape.GetTriangles();
				foreach (var triangle in triangles) {
					if (TriangleAndTriangle(triangle, A, B, C)) {
						return true;
					}
				}
				break;

		}

		return false;
	}

	/// <summary>
	/// Ignores the y axis; treats all points as if they only exist on the x-z plane.
	/// </summary>
	public static bool PolyShapeAndPolyShape(PolyShape shapeA, PolyShape shapeB) {
		// if either of the shapes is two points or less we can handle those scenarios differently
		if (shapeA.PointCount == 0 || shapeB.PointCount == 0) {
			return false; // idk
		}

		switch (shapeA.PointCount) {
			case 1:
				return PolyShapeAndPoint(shapeB, shapeA.PointXZ(0));
			case 2:
				return PolyShapeAndSegment(shapeB, shapeA.PointXZ(0), shapeA.PointXZ(1));
			case 3:
				return PolyShapeAndTriangle(shapeB, shapeA.PointXZ(0), shapeA.PointXZ(1), shapeA.PointXZ(2));
			default:
				// both are a quad or more complex, we have to triangulate one of the shapes and
				// check each one against the shape of the othe other
				var triangles = shapeA.GetTriangles();
				foreach (var triangle in triangles) {
					if (PolyShapeAndTriangle(shapeB, triangle)) {
						return true;
					}
				}
				break;
		}

		return false;
	}
}
