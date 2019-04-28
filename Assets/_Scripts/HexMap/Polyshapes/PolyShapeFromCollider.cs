using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyShapeFromCollider : MonoBehaviour {

	public int slices = 8;

	public void CreatePolyShape() {
		PolyShape shape = Utilities.PolyShape(gameObject, slices, true);
	}
}
