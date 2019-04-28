
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitEssence))]
public class UnitEditor : Editor {

	private void OnSceneGUI() {
		var unit = (UnitEssence)target;
		//if ( Selection.activeObject == colony) {
		Handles.color = new Color(0, 0, 1, .4f);
		//Handles.DrawSolidDisc(unit.transform.position, Vector3.up, unit.radius);
		Handles.DrawLine(unit.transform.position, unit.transform.position + unit.TurnVector.FromXZ() * 6f);
		//}
	}
}
