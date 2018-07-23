
using UnityEditor;
using UnityEngine;
using ClipperTests;

[CustomEditor(typeof(ClipTests))]
public class ClipTestEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		ClipTests shape = target as ClipTests;


		if (GUILayout.Button("Clip")) {
			Undo.RecordObject(shape, "Clip");
			shape.RunClip();
			EditorUtility.SetDirty(shape);
		}
	}
}
