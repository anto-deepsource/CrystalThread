
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HexMap.Pathfinding {
	[CustomEditor(typeof(HexNavAgent))]
	//[CanEditMultipleObjects]
	public class LocalAgentEditor : Editor {

		HexNavAgent agent;

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			agent = (HexNavAgent)target;

			//if (GUILayout.Button("Clear Path")) {
			//	agent.ForceClearPath();

			//}
		}
	}
}