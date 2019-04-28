using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MatchXZPosition : MonoBehaviour {

	public Transform target;

	public float verticalHeight = 0;
	
	//void Start () {
	//	verticalHeight = transform.position.y;
	//}
	
	void Update () {
		transform.position = target.position.DropY() + Vector3.up * verticalHeight;
	}
}
