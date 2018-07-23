using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(PolyShape))]
public class CreateQuadTest : MonoBehaviour {

	public Transform A;
	public Transform B;

	public float width = 5;

	public PolyShape segment;

	private PolyShape myShape;

	private PolyShapeMesh shapeMesh;
	private MeshRenderer meshRenderer;

	// Use this for initialization
	void OnEnable () {
		myShape = GetComponent<PolyShape>();
		shapeMesh = GetComponent<PolyShapeMesh>();
		meshRenderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if ( A ==null || B == null ) {
			return;
		}

		Vector2[] quad = ColDet.CreateQuad(A.position.JustXZ(), B.position.JustXZ(), width);

		myShape.points = new List<Vector3>( );
		foreach( var point in quad ) {
			myShape.points.Add( transform.InverseTransformPoint( point.FromXZ() ));
		}

		if (shapeMesh!=null ) {
			shapeMesh.Setup();
		}

		if (segment != null) {
			if (ColDet.SegmentAndQuadrilateral(
						quad[0],
						quad[1],
						quad[2],
						quad[3],
						segment.GetPointWorldPosition(0).JustXZ(),
						segment.GetPointWorldPosition(1).JustXZ()
						)) {
				meshRenderer.material.color = Color.blue;
			} else {
				meshRenderer.material.color = Color.white;
			}

		}
	}
}
