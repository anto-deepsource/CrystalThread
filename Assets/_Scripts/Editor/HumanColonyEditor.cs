using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HumanColony) )]
public class HumanColonyEditor : Editor {

	private void OnSceneGUI() {
		var colony = (HumanColony)target;
		//if ( Selection.activeObject == colony) {
		Handles.color = new Color(0,0,1,.4f);
			Handles.DrawSolidDisc(colony.transform.position, Vector3.up, colony.radius);
		//}
	}
}
