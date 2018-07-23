using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitAnimator))]
public class UnitAnimEditor : Editor {

	UnitAnimator unitAnimator;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		unitAnimator = (UnitAnimator)target;

		if (GUILayout.Button("Setup")) {
			unitAnimator.Setup();
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

		}
	}
}
