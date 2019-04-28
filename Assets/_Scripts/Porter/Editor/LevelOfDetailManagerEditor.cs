
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Porter {
	[CustomEditor(typeof(LevelOfDetailManager))]
	public class LevelOfDetailManagerEditor : Editor {

		private void OnSceneGUI() {
			var awareness = (LevelOfDetailManager)target;
			Handles.color = new Color(0, 0, 1, .4f);

			var position = awareness.CowPosition();

			Handles.DrawWireDisc(position, Vector3.up, awareness.levelOneDistance);
			Handles.DrawWireDisc(position, Vector3.up, awareness.levelTwoDistance);

		}
	}
}