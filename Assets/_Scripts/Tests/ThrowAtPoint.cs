using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowAtPoint : MonoBehaviour {

	public GameObject spawnPrefab;

	public Transform target;

	public Transform spawnPoint;

	public bool showTrajectory = false;

	public Vector3 simulatedMovement;

	public float trackTime = 0.1f;
	private float trackDelay = 0.0f;

	public float throwForce = 1;

	public float spawnTime = 1f;
	
	private float spawnDelay = 0;



	public Vector3 lastVelocity;
	public Vector3 lastPosition;

	// Use this for initialization
	void Start () {
		spawnDelay = spawnTime;
	}
	
	// Update is called once per frame
	void Update () {
		spawnDelay -= Time.deltaTime;
		UpdateTargetVelocity();
		if ( spawnDelay < 0 ) {
			spawnDelay = spawnTime;

			Vector3 startPosition = spawnPoint.position;
			Vector3 targetPosition = target.position;
			Vector2 variance = Random.insideUnitCircle * 3f;
			targetPosition += variance.FromXZ();

			GameObject newThrownable = GameObject.Instantiate(spawnPrefab);
			newThrownable.transform.position = startPosition;
			Rigidbody myBody = newThrownable.GetComponent<Rigidbody>();
			myBody.velocity = TrajectoryUtils.GetInitialVelocityToHitMovingTargetWithLateralSpeed(
				startPosition,
				targetPosition,
				GetTargetVelocity(),
				throwForce
			);

		}
	}

	private void UpdateTargetVelocity() {
		trackDelay -= Time.deltaTime;
		if (trackDelay < 0) {
			Vector3 currentPosition = target.position;
			Vector3 currentVelocity = currentPosition - lastPosition;
			lastVelocity = lastVelocity * 0.4f + currentVelocity * 0.6f;

			lastPosition = currentPosition;

			trackDelay += trackTime;
		}
	}

	private Vector3 GetTargetVelocity() {
		if ( !Application.isPlaying ) {
			return simulatedMovement;
		}
		//Rigidbody targetBody = target.gameObject.GetComponentInChildren<Rigidbody>();
		//if ( targetBody!=null ) {
		//	lastVelocity = lastVelocity * 0.9f + targetBody.velocity * 0.1f;
		//	return lastVelocity;
		//}
		//return simulatedMovement;
		
		return lastVelocity / trackTime;
	}

	private void OnDrawGizmos() {
		if (showTrajectory) {
			DrawTrajectoryGizmo();
			DrawTargetVelocityGizmo();
		}
		
	}

	private void DrawTrajectoryGizmo() {
		Vector3 startPoint = spawnPoint ? spawnPoint.position : transform.position;

		Vector3 initialVelocity = TrajectoryUtils.GetInitialVelocityToHitMovingTargetWithLateralSpeed(
			startPoint,
			target.position,
			GetTargetVelocity(),
			throwForce
		);
		
		Vector3 lastPoint = startPoint;
		for (int i = 1; i < 10; i++) {
			Vector3 newPoint = startPoint + TrajectoryUtils.ProjectedPoint(initialVelocity, (float)i * 1.0f);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(lastPoint, newPoint);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(newPoint, .5f);
			lastPoint = newPoint;
		}
	}

	private void DrawTargetVelocityGizmo() {

		Vector3 targetVelocity = GetTargetVelocity();

		Vector3 startPoint = target.transform.position;
		Vector3 lastPoint = startPoint;
		for (int i = 1; i < 10; i++) {
			Vector3 newPoint = startPoint + (targetVelocity * (float)i * 1.0f);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(lastPoint, newPoint);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(newPoint, .5f);
			lastPoint = newPoint;
		}
	}
}
