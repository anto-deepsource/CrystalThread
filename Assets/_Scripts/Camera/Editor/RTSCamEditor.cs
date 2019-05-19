using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RTSCamera))]
public class RTSCamEditor : Editor {

	public override void OnInspectorGUI() {
		RTSCamera rts = (RTSCamera)target;

		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		//if ( EditorGUI.EndChangeCheck() ) {
		//	rts.UpdatePosition(1.0f);
		//}
		
		//if ( GUILayout.Button("Update") ) {
		//	rts.UpdatePosition(1.0f);
		//}


	}
}
