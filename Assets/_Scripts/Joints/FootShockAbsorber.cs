using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FootShockAbsorber : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;

	public Rigidbody connectedBody;
	public Collider theirCollider;

	public Transform connectedAnchorPoint;

	public float targetOffset = -1f;

	public float upperLimit = 0;

	public float lowerLimit = -1f;

	//public Vector3 targetOffset = Vector3.down;

	public float strength = 1f;

	public float damping = 0f;

	private Rigidbody myBody;

	private float lastDisplacement = 0;

	public Grounded groundContact;

	//private bool isGrounded = false;

	private Collider myCollider;

	private void Start() {
		myBody = GetComponent<Rigidbody>();
		if (theirCollider == null) {
			theirCollider = connectedBody.GetComponent<Collider>();
		}
		
		myCollider = myBody.GetComponent<Collider>();
		if( theirCollider!=null && myCollider != null ) {
			Physics.IgnoreCollision(theirCollider, myCollider);
		}
		lastDisplacement = CalculateDisplacement();
	}

	private float CalculateDisplacement() {
		var targetPosition = connectedBody.transform.position.y + targetOffset;
		var currentPosition = transform.position.y;
		return targetPosition - currentPosition;
	}

	private float CalculateDisplacementToTarget() {
		var targetPosition = connectedBody.transform.position.y + targetOffset;
		var currentPosition = transform.position.y;
		return targetPosition - currentPosition;
	}

	private void FixedUpdate() {

		bool repositionedVertically = false;

		// perform a raycast from the connected body down
		// if we hit anything we need to at least be on this side of it
		RaycastHit hitResult;
		var startPosition = connectedAnchorPoint.position;
		Vector3 direction = Vector3.down;
		var targetPosition = connectedBody.transform.position.y + targetOffset;
		float maxRaycastDistance = startPosition.y - targetPosition;
		if (Physics.Raycast(startPosition, direction, out hitResult, maxRaycastDistance, groundLayer)
				&& myBody.position.y < hitResult.point.y ) {
			myBody.position = hitResult.point - myCollider.bounds.extents.JustY();
			myBody.velocity = Vector3.zero;
			repositionedVertically = true;
		}

		Vector3 newPosition = myBody.position;
		newPosition = myBody.position.JustY() + connectedBody.transform.position.DropY();
		float yDifference = myBody.position.y - connectedBody.transform.position.y;
		if (yDifference < lowerLimit) {
			newPosition.y = connectedBody.transform.position.y + lowerLimit;
			myBody.velocity = Vector3.zero;
			repositionedVertically = true;
		}
		//else
		//if (yDifference > upperLimit) {
		//	newPosition.y = connectedBody.transform.position.y + upperLimit;
		//	myBody.velocity = Vector3.zero;
		//}
		if (newPosition.y > connectedAnchorPoint.position.y ) {
			newPosition.y = connectedAnchorPoint.position.y + upperLimit;
			myBody.velocity = Vector3.zero;
			repositionedVertically = true;
		}
		myBody.position = newPosition;
		
		// apply a spring force, proportional to the displacement
		float displacement = CalculateDisplacementToTarget();

		if (groundContact.IsGrounded) {
			connectedBody.AddForce(-Vector3.up * displacement * strength);
		} else {
			myBody.AddForce(Vector3.up * displacement * strength);
		}
		


		//apply a damping force, proportional to the velocity
		float changeInDisplacement = displacement - lastDisplacement;
		if (groundContact.IsGrounded) {
			connectedBody.AddForce(-changeInDisplacement * Vector3.up * damping);
		}
		lastDisplacement = displacement;

		//connectedBody.AddForce(Physics.gravity);
	}

	//public void OnCollisionStay(Collision collision) { 
	//	if (CommonUtils.IsOnLayer(collision.gameObject, groundLayer)) {
	//		if (!isGrounded) {
	//			isGrounded = true;
	//		}
	//	}
	//}

	//public void OnCollisionExit(Collision collision) {
	//	if (CommonUtils.IsOnLayer(collision.gameObject, groundLayer)) {
	//		isGrounded = false;
	//	}
	//}
}
