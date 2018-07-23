using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaggerOnCollision : MonoBehaviour {

	public float minMomentum = 1.0f;
	public float duration = 0.5f;

	private Blackboard blackboard;
	//private Rigidbody myBody;

	// Use this for initialization
	void OnEnable () {
		blackboard = GetComponent<Blackboard>();
		//myBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnCollisionEnter(Collision collision) {
		Rigidbody theirBody = collision.collider.GetComponent<Rigidbody>();
		if (theirBody!=null && theirBody.velocity.magnitude > minMomentum) {
			//Debug.Log("Collision");
			//blackboard.triggers[StateTransition.Staggered] = true;
			blackboard.SetStagger(duration);
		}
	}
}
