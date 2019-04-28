
using UnityEditor;
using UnityEngine;
using ClipperTests;
using HexMap;
using System;

[CustomEditor(typeof(ResourcesComponent))]
public class ResourcesComponentEditor : Editor {

	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		var resources = (ResourcesComponent)target;

		if ( Application.isPlaying) {
			foreach(ResourceType resourceType in Enum.GetValues(typeof( ResourceType))) {
				int value = 0;
				if ( !resources.Quantities.TryGetValue(resourceType, out value) ) {
					value = 0;
				}
				string label = string.Format("{0}: {1}", resourceType.ToString(), value);
				EditorGUILayout.LabelField(label);
			}
		}
	}
}
