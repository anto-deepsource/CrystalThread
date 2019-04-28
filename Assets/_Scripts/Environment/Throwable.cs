using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {

	public float spinForce = 100f;

	public float throwForce = 200f;

	public Vector3 lastRotation;

	private Rigidbody myBody;

	private void Start() {
		myBody = GetComponentInChildren<Rigidbody>();
	}

	public void Spin(Vector3 grapPointWorldSpace) {
		myBody = GetComponentInChildren<Rigidbody>();
		//myBody.AddForceAtPosition(transform.forward * spinForce, grapPointWorldSpace + transform.up, ForceMode.Impulse);
		//myBody.AddForceAtPosition(-transform.forward * spinForce, grapPointWorldSpace - transform.up, ForceMode.Impulse);
		//myBody.angularVelocity = transform.up - lastRotation;
		lastRotation = transform.up;
		transform.RotateAround(grapPointWorldSpace, Vector3.right, spinForce);

	}

	public void Throw(Vector3 direction) {
		
		myBody.velocity = direction.normalized * throwForce;
		myBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
	}
}
