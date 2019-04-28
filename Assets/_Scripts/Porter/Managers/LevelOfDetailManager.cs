using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOfDetailManager : MonoBehaviour {

	#region Public Members

	[Tooltip("The game object in the scene that we consider as 'the center of the world' and" +
		"choose levels of detail based on their distance to them.")]
	public GameObject centerOfTheWorld;

	[Tooltip("Things within this distance to the C.O.W. are considered Level One.")]
	public float levelOneDistance = 1000f;

	[Tooltip("Things outside the previous level but within this distance to the C.O.W." +
		" are considered Level Two.")]
	public float levelTwoDistance = 3000f;

	//[Tooltip("The minimum time, in seconds, that an object stays in a level after changing." +
	//	"This is to reduce jittering between two levels.")]
	//public float minimumTimeInLevel = 1f;

	#endregion

	#region Static Functions

	private static LevelOfDetailManager instance;

	/// <summary>
	/// Convience method for grabbing the in-scene gameobject with the given component.
	/// </summary>
	public static LevelOfDetailManager GetObject() {
		if (instance == null) {
			instance = FindObjectOfType<LevelOfDetailManager>() as LevelOfDetailManager;
		}
		return instance;
	}
	#endregion


	public Vector3 CowPosition() {
		return centerOfTheWorld.transform.position;
	}
}
