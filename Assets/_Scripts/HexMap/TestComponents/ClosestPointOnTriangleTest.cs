using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPointOnTriangleTest : MonoBehaviour {

	public Transform point;

	public PolyShape triangle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 result;
		ColDet.DistanceToClosestPointOnTriangle( 
			triangle.transform.TransformPoint( triangle.points[0] ).JustXZ(),
			triangle.transform.TransformPoint(triangle.points[1]).JustXZ(),
			triangle.transform.TransformPoint(triangle.points[2]).JustXZ(),
			point.position.JustXZ(), out result );

		transform.position = new Vector3(result.x, 0, result.y);
	}
}
