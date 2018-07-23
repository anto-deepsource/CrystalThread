using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HexMap {
	[CustomEditor(typeof(HexagonMaker))]
	//[CanEditMultipleObjects]
	public class HexMakerEditor : Editor {

		HexagonMaker hexMaker;

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			hexMaker = (HexagonMaker)target;

			if (GUILayout.Button("Generate Tile")) {
				hexMaker.Setup();
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			}

			if (GUILayout.Button("Clear")) {
				hexMaker.Clear();
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			}
		}
	}
}