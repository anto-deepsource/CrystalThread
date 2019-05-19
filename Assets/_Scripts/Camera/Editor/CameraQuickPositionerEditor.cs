
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraQuickPositioner))]
public class CameraQuickPositionerEditor : Editor {

	public override void OnInspectorGUI() {
		CameraQuickPositioner tar = (CameraQuickPositioner)target;

		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if (EditorGUI.EndChangeCheck()) {
			tar.RepositionCamera();
		}

		if (GUILayout.Button("Update")) {
			tar.RepositionCamera();
		}


	}
}
