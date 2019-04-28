
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a Task that causes the agent to 'Move to a Point'.
/// It grabs the point off of the blackboard's targetPoint.
/// </summary>
public class MoveToPointTask : ITask {

	public Vector3 target;

    public float stopDistance = 1;
    public bool controlUnitAnimator = true;
	public LayerMask groundLayer;

	private UnitAnimator animator;
	private HexNavAgent hexNavAgent;

	private Vector3 lastPosition;

	public MoveToPointTask(float stopDistance = 1, bool controlUnitAnimator = true) {
		this.stopDistance = stopDistance;
		this.controlUnitAnimator = controlUnitAnimator;
	}

	private void Initialize(Blackboard blackboard) {
		if (hexNavAgent == null) {
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
		// DISABLED on February 9, 2019
		return false;
		//Initialize(blackboard);
		
		//// If we're already within the distance, skip anything else and report finished (success)
		//float d = CommonUtils.DistanceSquared(blackboard.transform.position.DropY(),
		//	target.DropY());

		//if (d < stopDistance * stopDistance) {
		//	blackboard.MoveVector = Vector3.zero;

		//	if (controlUnitAnimator) {
		//		animator.IsRunning = false;
		//	}

		//	return FinalizeMaybe(true);
		//}

		//// evaluate the pathing and get a move vector
		//hexNavAgent.Destination = target;
		//blackboard.MoveVector = hexNavAgent.MoveVector;

		//if (blackboard.MoveVector.sqrMagnitude > 0.01f) {
		//	blackboard.TurnVector = blackboard.MoveVector.JustXZ();
		//}

		//// try to do the animation
		//if (controlUnitAnimator) {

		//	Ray ray = new Ray(blackboard.transform.position + Vector3.up * 0.5f, Vector3.down);
		//	RaycastHit groundHit;
		//	animator.IsInAir = !Physics.Raycast(ray, out groundHit, 1.6f, ~groundLayer);

		//	bool isRunning = !(d < stopDistance * stopDistance) &&
		//		(lastPosition - blackboard.transform.position).magnitude > 0.1f;

		//	animator.IsRunning = isRunning;
		//	lastPosition = blackboard.transform.position;
		//}

		//// we've finished (successfully) if the current distance is less than the stop distance
		//return FinalizeMaybe(d < stopDistance * stopDistance);
	}

	/// <summary>
	/// If update returns true we want to turn the nav agent off here, otherwise we won't get another chance.
	/// </summary>
	private bool FinalizeMaybe(bool updateFinished) {
		if (updateFinished) {
			hexNavAgent.Stop();
		}
		return updateFinished;
	}
}
