using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TwoShapeCollisionTest : MonoBehaviour {

	public PolyShape otherShape;

	MeshRenderer meshRenderer;

	PolyShape myShape;

	void Update () {
		meshRenderer = GetComponent<MeshRenderer>();
		myShape = GetComponent<PolyShape>();

		if ( ColDet.PolyShapeAndPolyShape( myShape, otherShape ) ) {
			meshRenderer.material.color = Color.red;
		} else {
			meshRenderer.material.color = Color.blue;
		}
		
	}
}
