
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StaticObjectSpawner))]
public class StaticObjectSpawnerEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var spawner = (StaticObjectSpawner)target;
		if ( GUILayout.Button("Setup")) {
			spawner.SpawnObjects();
		}
	}

	private void OnSceneGUI() {
		var spawner = (StaticObjectSpawner)target;
		//if ( Selection.activeObject == colony) {
		Handles.color = new Color(0, 0, 1, .2f);
		Handles.DrawSolidDisc(spawner.transform.position, Vector3.up, spawner.radius);
		//}
	}
}
