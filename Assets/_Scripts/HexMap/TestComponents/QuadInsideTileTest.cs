using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuadInsideTileTest : MonoBehaviour {

	public HexTile hexTile;

	MeshRenderer meshRenderer;

	PolyShape myShape;

	void Update() {
		meshRenderer = GetComponent<MeshRenderer>();
		myShape = GetComponent<PolyShape>();

		//if ( HexNavMeshManager.CheckPointIsPathable(transform.position)) {
		//	meshRenderer.material.color = Color.blue;
		//}
		//else {
		//	meshRenderer.material.color = Color.red;
		//}

		if (HexNavMeshManager.CheckIsBuildablePosition(myShape)) {
			meshRenderer.material.color = Color.blue;
		}
		else {
			meshRenderer.material.color = Color.red;
		}

	}
	}
