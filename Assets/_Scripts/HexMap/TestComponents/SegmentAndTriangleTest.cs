using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class SegmentAndTriangleTest : MonoBehaviour {

	public PolyShape triangle;

	public PolyShape segment;

	LineRenderer lineRenderer;
	PolyShape lineSegment;

	// Use this for initialization
	void OnEnable () {
		lineRenderer = GetComponent<LineRenderer>();
		lineSegment = GetComponent<PolyShape>();
	}
	
	// Update is called once per frame
	void Update () {
		lineRenderer.positionCount = lineSegment.PointCount;
		for( int i = 0; i < lineSegment.PointCount; i ++ ) {
			lineRenderer.SetPosition(i, lineSegment.GetPointWorldPosition(i));
		}
		
		if ( triangle != null ) {
			if ( triangle.PointCount == 3 ) {
				if (ColDet.SegmentAndTriangle(
						triangle.GetPointWorldPosition(0).JustXZ(),
						triangle.GetPointWorldPosition(1).JustXZ(),
						triangle.GetPointWorldPosition(2).JustXZ(),
						lineSegment.GetPointWorldPosition(0).JustXZ(),
						lineSegment.GetPointWorldPosition(1).JustXZ()
						)) {
					lineRenderer.material.color = Color.blue;
				} else {
					lineRenderer.material.color = Color.white;
				}
			} else
			if (triangle.PointCount == 4) {
				if (ColDet.SegmentAndQuadrilateral(
						triangle.GetPointWorldPosition(0).JustXZ(),
						triangle.GetPointWorldPosition(1).JustXZ(),
						triangle.GetPointWorldPosition(2).JustXZ(),
						triangle.GetPointWorldPosition(3).JustXZ(),
						lineSegment.GetPointWorldPosition(0).JustXZ(),
						lineSegment.GetPointWorldPosition(1).JustXZ()
						)) {
					lineRenderer.material.color = Color.blue;
				} else {
					lineRenderer.material.color = Color.white;
				}
			}

		} else

		if (segment != null) {
			if (ColDet.SegmentAndSegment(
			segment.GetPointWorldPosition(0).JustXZ(),
			segment.GetPointWorldPosition(1).JustXZ(),
			lineSegment.GetPointWorldPosition(0).JustXZ(),
			lineSegment.GetPointWorldPosition(1).JustXZ()
			)) {
				lineRenderer.material.color = Color.blue;
			} else {
				lineRenderer.material.color = Color.white;
			}
		}
	}
}
