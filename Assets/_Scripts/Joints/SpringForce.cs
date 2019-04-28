using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringForce : MonoBehaviour {

	public Rigidbody connectedBody;

	public Vector3 anchorPoint;

	public float force = 10;
	public float maxForce = 100;
	public float dampening = 1;

	private Vector3 lastVector;

	void FixedUpdate () {
		//var grabPoint = connectedBody.transform.TransformPoint(anchorPoint);

		Vector3 vector;
		if (connectedBody != null) {
			vector = connectedBody.transform.TransformPoint(anchorPoint) - transform.position;
		}
		else {
			vector = anchorPoint - transform.position;
		}
		var displacementForce = vector * force;
		if (displacementForce.sqrMagnitude > maxForce * maxForce) {
			displacementForce = vector.normalized * maxForce;
		}
		connectedBody.AddForceAtPosition(-displacementForce, anchorPoint);

		var changeInVector = vector - lastVector;
		connectedBody.AddForce(-changeInVector * dampening);
		lastVector = vector;
	}

	private void OnDrawGizmos() {
		Gizmos.DrawLine(connectedBody.transform.TransformPoint(anchorPoint), transform.position);
	}
}
