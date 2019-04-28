
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Bones))]
public class BoneEditor : Editor {

	

	public override void OnInspectorGUI() {

		DrawDefaultInspector();

		var bones = (Bones)target;
		if ( !bones.shouldLockDistance) {
			bones.lockDistance = bones.BoneVector().magnitude;
		}

		bones.shouldLockDistance = EditorGUILayout.Toggle("Lock Distance", bones.shouldLockDistance );
	}

	float handleSize = .2f;

	float ringSize = .2f;

	private bool selectingEndPosition = false;


	public void OnSceneGUI() {
		var bones = (Bones)target;

		Handles.DrawWireDisc(bones.transform.position, bones.BoneVector(), ringSize);

		if ( selectingEndPosition ) {
			Tools.hidden = true;

			if (Handles.Button(bones.transform.position, Quaternion.identity, handleSize, 2,
				Handles.DotHandleCap)) {
				selectingEndPosition = false;
			}
			bones.endPosition.position = Handles.DoPositionHandle(bones.endPosition.position,
				bones.transform.rotation);

			if (bones.shouldLockDistance ) {
				var vector = bones.BoneVector();
				bones.endPosition.position = bones.transform.position
					+ vector.normalized * bones.lockDistance;
			}

		} else {
			Tools.hidden = false;
			if (Handles.Button(bones.endPosition.position, Quaternion.identity, handleSize, 2,
				Handles.DotHandleCap)) {
				selectingEndPosition = true;
			}
		}

		Tools.hidden = false;
	}
}
