using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverAboveTarget : MonoBehaviour {

	public Transform targetObject;

	public float turnSpeed = 1;
	public float moveSpeed;
	public float brakeForce;

	public float idealHorizontalDistance = 20f;

	public float idealVerticalDistance = 100f;

	Vector3 targetDirection;
	float targetDistance;
	Rigidbody myBody;
	Vector3 movement;

	void Start() {
		myBody = GetComponent<Rigidbody>();
	}

	private void Update() {
		targetDirection = targetObject.position - transform.position;
		targetDistance = targetDirection.JustXZ().magnitude;
		if (targetDistance > idealHorizontalDistance) {
			movement = targetDirection;
		} else
		if (targetDistance < idealHorizontalDistance) {
			movement = -targetDirection;
		}
		else {
			movement = Vector3.zero;
		}
		float targetVerticalDistance = targetDirection.y;
		movement.y = targetVerticalDistance - idealVerticalDistance;


	}

	private void FixedUpdate() {
		
		var moveVector = movement.normalized;

		if (moveVector.sqrMagnitude > 0.01f) {
			var idealVelocity = moveVector * moveSpeed;
			var currentVelocity = myBody.velocity;
			var idealDifference = idealVelocity - currentVelocity;
			myBody.AddForce(idealDifference);
		}
		else {
			var currentVelocity = myBody.velocity;
			var idealDifference = -currentVelocity;
			myBody.AddForce(idealDifference * brakeForce);
		}


		//if (makeUnitLookForward) {
		TurnToDirection(targetDirection.JustXZ() );
		transform.forward = targetDirection;
		//Vector3 forward = (targetObject.position - transform.position).DropY();
		//transform.rotation = Quaternion.LookRotation(forward);
		//}
		//else
		//if (moveVector.sqrMagnitude > 0.01f) {
		//	TurnToDirection(moveVector.JustXZ());

		//	//transform.rotation = Quaternion.LookRotation(myBody.velocity.DropY());
		//}

		var currentTorque = myBody.angularVelocity;
		//Debug.Log(currentTorque);
		myBody.AddTorque(0, -currentTorque.y * 0.6f, 0);
	}

	private void TurnToDirection(Vector2 xzVector) {
		float targetTheta = Mathf.Atan2(xzVector.y, xzVector.x);
		var currentVector = transform.forward.JustXZ().normalized;
		float currentTheta = Mathf.Atan2(currentVector.y, currentVector.x);
		float difference = CommonUtils.AngleBetweenThetas(targetTheta, currentTheta);

		myBody.AddTorque(0, difference * turnSpeed, 0);
	}
}
