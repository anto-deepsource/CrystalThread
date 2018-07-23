using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolyShapeMesh))]
public class PolyShapeMeshEditor : Editor {

	public override void OnInspectorGUI() {
		PolyShapeMesh shape = target as PolyShapeMesh;
		

		if (GUILayout.Button("Setup")) {
			Undo.RecordObject(shape, "Setup");
			shape.Setup();
			EditorUtility.SetDirty(shape);
		}
	}
}
