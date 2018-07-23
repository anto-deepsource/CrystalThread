using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Container for a particular building/structure that can be built, leaving a new GameObject on the battlefield.
/// </summary>
[CreateAssetMenuAttribute(menuName = "Build Menu/Structure Data")]
public class StructureData : ScriptableObject {

	public string title = "";

	public List<ResourceTypeValuePair> costs = new List<ResourceTypeValuePair>();

	public Sprite icon;

	public GameObject objectPrefab;
}
