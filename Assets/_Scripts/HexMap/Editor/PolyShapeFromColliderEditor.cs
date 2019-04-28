using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolyShapeFromCollider))]
public class PolyShapeFromColliderEditor : Editor {

	public override void OnInspectorGUI() {
		PolyShapeFromCollider shape = target as PolyShapeFromCollider;

		DrawDefaultInspector();

		if (GUILayout.Button("Create")) {
			Undo.RecordObject(shape, "Create");
			shape.CreatePolyShape();
			EditorUtility.SetDirty(shape);
		}
	}
}
