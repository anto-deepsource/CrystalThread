
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor {

	private void OnSceneGUI() {
		var colony = (Spawner)target;
		//if ( Selection.activeObject == colony) {
		Handles.color = new Color(0, 0, 1, .4f);
		Handles.DrawSolidDisc(colony.transform.position, Vector3.up, colony.radius);
		//}
	}
}
