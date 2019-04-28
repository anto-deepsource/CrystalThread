
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeleeWeaponAnimator))]
public class MeleeWeaponAnimatorEditor : Editor {

	MeleeWeaponAnimator animator;

	private Transform handleTransform;
	private Quaternion handleRotation;

	private bool hideHandle;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		animator = (MeleeWeaponAnimator)target;

		hideHandle = EditorGUILayout.Toggle("Hide handle", hideHandle);
	}

	private void OnSceneGUI() {
		animator = target as MeleeWeaponAnimator;

		Tools.hidden = hideHandle;

		handleTransform = animator.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		Vector3 point = handleTransform.TransformPoint( animator.handlePoint );
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(animator, "Move Point");
			EditorUtility.SetDirty(animator);
			animator.handlePoint = handleTransform.InverseTransformPoint(point);
		}

		point = handleTransform.TransformPoint(animator.tipPoint);
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(animator, "Move Point");
			EditorUtility.SetDirty(animator);
			animator.tipPoint = handleTransform.InverseTransformPoint(point);
		}
	}
}
