using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HexMap;

[CustomEditor(typeof(PolyShape))]
public class PolyShapeEditor : Editor {

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private Transform handleTransform;
	private Quaternion handleRotation;
	private int selectedIndex = -1;

	PolyShape shape;

	public override void OnInspectorGUI() {
		shape = target as PolyShape;

		if (selectedIndex >= 0 && selectedIndex < shape.PointCount) {
			DrawSelectedPointInspector();
		}

		if (GUILayout.Button("Add Point")) {
			Undo.RecordObject(shape, "Add Point");
			shape.AddPoint();
			EditorUtility.SetDirty(shape);

			selectedIndex = shape.PointCount - 1;
		}
	}

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", shape.GetPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(shape, "Move Point");
			EditorUtility.SetDirty(shape);
			shape.SetPoint(selectedIndex, point);
		}
		if (GUILayout.Button("Remove Point")) {
			Undo.RecordObject(shape, "Remove Point");
			shape.RemovePoint(selectedIndex);
			EditorUtility.SetDirty(shape);

			if (selectedIndex >= shape.PointCount) {
				selectedIndex = shape.PointCount - 1;
			}
		}
	}

	private void OnSceneGUI() {
		shape = target as PolyShape;
		handleTransform = shape.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		
		for( int i =0; i < shape.PointCount; i ++ ) {
			Vector3 point = ShowPoint(i);
			Handles.color = Color.gray;
			int j = i + 1 >= shape.PointCount? 0: i + 1;
			Vector3 nextPoint = handleTransform.TransformPoint(shape.GetPoint(j));
			Handles.DrawLine(point, nextPoint);
		}
	}

	private Vector3 ShowPoint(int index) {
		Vector3 point = handleTransform.TransformPoint(shape.GetPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}
		Handles.color = Color.white;
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) {
			selectedIndex = index;
			Repaint();
		}
		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(shape, "Move Point");
				EditorUtility.SetDirty(shape);
				shape.SetPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
}
