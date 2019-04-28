using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class MoveTask : PrimitiveTask {

		public float stopDistance = 5f;

		MoveToTargetTask moveToTargetTask;

		private Blackboard blackboard;

		private void Start() {
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			moveToTargetTask = new MoveToTargetTask(blackboard, stopDistance);
		}

		public override void Stop() {
			base.Stop();
			// since this state use navigation/move to task, we need to try and stop the nav agent
			if (blackboard != null) {
				Utilities.StopHexNavAgentMaybe(blackboard);
			}
		}

		public void Update() {
			if ( blackboard.target==null ) {
				Debug.Log("No target");
				Running = false;
				return;
			}

			var targetTransform = blackboard.target.transform;

			moveToTargetTask.stopDistance = stopDistance;
			moveToTargetTask.TargetTransform = targetTransform;
			Successful = moveToTargetTask.UpdateTask();
			
			if (Successful) {
				// we're in range of the target -> turn to face them

				var targetTurnVector = (targetTransform.position.JustXZ() -
				transform.position.JustXZ()).normalized;

				blackboard.TurnVector = targetTurnVector;

				float turnForce = CommonUtils.AngleBetweenVectors(targetTurnVector, transform.forward.JustXZ());
				if (turnForce * turnForce < .01f) {
					blackboard.TurnVector = Vector2.zero;
					Successful = true;
				} else {
					Successful = false;
				}
			}
			Running = !Successful; // if we're not successful -> we're stilling running
		}
		
	}
}