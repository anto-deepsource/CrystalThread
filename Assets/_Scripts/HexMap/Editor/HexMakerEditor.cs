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
			
			if (GUILayout.Button("Clear")) {
				hexMaker.ClearAllTiles();
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			}
		}
	}
}