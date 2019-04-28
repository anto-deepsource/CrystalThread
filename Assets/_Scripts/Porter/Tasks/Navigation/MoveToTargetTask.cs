using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveToTargetTask : ITask {

	public Transform TargetTransform {
		get {
			return _targetTransform;
		}
		set {
			usingTransform = true;
			_targetTransform = value;
		}
	}
	private Transform _targetTransform;

	public Vector3 TargetPoint {
		get {
			return _targetPoint;
		}
		set {
			usingTransform = false;
			_targetPoint = value;
		}
	}
	private Vector3 _targetPoint;

	public float stopDistance = 1;

	public float speedModifier = 1;

	bool controlUnitAnimator = true;
	public int groundLayer = LayerMask.NameToLayer("Walkable");

	private UnitAnimator animator;
	private HexNavAgent hexNavAgent;
	private Blackboard blackboard;

	private Vector3 lastPosition;

	private bool usingTransform = false; // versus using the vector3 target point

	public MoveToTargetTask(Blackboard blackboard, float stopDistance = 1, bool controlUnitAnimator = true) {
		this.blackboard = blackboard;
		this.stopDistance = stopDistance;
		this.controlUnitAnimator = controlUnitAnimator;
	}

	private void Initialize() {
		if (hexNavAgent == null ) {
			lastPosition = blackboard.transform.position;

			hexNavAgent = Utilities.HexNavAgent(blackboard.gameObject);
			hexNavAgent.enabled = true; // makes sure its on
			hexNavAgent.radius = blackboard.radius;

			animator = blackboard.GetComponentInChildren<UnitAnimator>();
			if (controlUnitAnimator && animator == null)
				controlUnitAnimator = false;
		}
	}

    public bool UpdateTask() {
		Initialize();

		if (usingTransform && TargetTransform == null) {

			return FinalizeMaybe(true); // still done (true)
		}

		var targetPos = TargetPoint;
		if (usingTransform) {
			targetPos = TargetTransform.position;
		}

		// If we're already within the distance, skip anything else and report finished (success)
		float d = CommonUtils.DistanceSquared(blackboard.transform.position.DropY(), targetPos.DropY());

		if (d < stopDistance * stopDistance) {
			blackboard.MoveVector = Vector3.zero;

			if (controlUnitAnimator) {
				animator.IsRunning = false;
			}

			return FinalizeMaybe(true);
		}

		//// ------Global ------------
		//hexNavAgent.Destination = blackboard.target.transform.position;

		//if (hexNavAgent.Status == PathStatus.Succeeded) {

		//	// ------Local ------------
		hexNavAgent.Destination = targetPos;

		//if (hexNavLocalAgent.Status == PathStatus.Succeeded) {
			blackboard.MoveVector = hexNavAgent.MoveVector * speedModifier;

		if (blackboard.MoveVector.sqrMagnitude > 0.01f) {
			blackboard.TurnVector = blackboard.MoveVector.JustXZ();
			//transform.rotation = Quaternion.LookRotation(movement);
		}
		else
		if (blackboard.IsGrounded) {
			//if (target != null) {
			//	transform.LookAt(target.transform.position.DropY() + transform.position.y * Vector3.up);
			//}
		}
		//	//} else {
		//	//	blackboard.moveVector = hexNavAgent.MoveVector;
		//}

		//} else {
		//	blackboard.moveVector = Vector3.zero;
		//}

		//d = CommonUtils.DistanceSquared(blackboard.transform.position, blackboard.target.transform.position);

		// try to do the animation
		if (controlUnitAnimator) {

			Ray ray = new Ray(blackboard.transform.position + Vector3.up * 0.5f, Vector3.down);
			RaycastHit groundHit;
			animator.IsInAir = !Physics.Raycast(ray, out groundHit, 1.6f, ~groundLayer);

			bool isRunning = !(d < stopDistance * stopDistance) && 
				(lastPosition - blackboard.transform.position).magnitude > 0.1f;

			animator.IsRunning = isRunning;
			lastPosition = blackboard.transform.position;
		}

		// we've finished (successfully) if the current distance is less than the stop distance
		return FinalizeMaybe(d < stopDistance * stopDistance);
	}

	/// <summary>
	/// If update returns true we want to turn the nav agent off here, otherwise we won't get another chance.
	/// </summary>
	private bool FinalizeMaybe( bool updateFinished ) {
		if ( updateFinished ) {
			hexNavAgent.Stop();
		}
		return updateFinished;
	}
}
