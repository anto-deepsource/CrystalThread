
using UnityEditor;
using UnityEngine;
using ClipperTests;
using HexMap;

[CustomEditor(typeof(CoordsIndicator))]
public class CoordIndicatorEditor : Editor {

	public void OnSceneGUI() {
		CoordsIndicator shape = target as CoordsIndicator;
		if ( shape.maker!=null ) {
			Vector2Int coords = shape.maker.WorldPositionToAxialCoords(shape.transform.position);

			Handles.Label(shape.transform.position + Vector3.back, coords.ToString());
		}
		
		
	}
}
