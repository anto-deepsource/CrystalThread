using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBetweenTwoPoints : MonoBehaviour {

	public Transform pointA;
	public Transform pointB;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = new Vector3(transform.localScale.x, (pointB.position - pointA.position).magnitude * 0.5f, transform.localScale.z);
	}
}
