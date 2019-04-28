
using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {

	/// <summary>
	/// Given a target object, moves into position and attempts to 'carry' it.
	/// The target object may be moving.
	/// If an attack is defined, the unit uses that attack on the object while picking it up.
	/// </summary>
	public class PickUpObjectState : IState {

		public Carryable targetObject;
		
		[Tooltip("The attack or ability to use in order to pick up the object.")]
		public AbstractAttack attack;
		
		MoveToTargetTask moveToTargetTask;
		Sequence sequence;
		
		private float coolDownTimer = -1f;

		private Blackboard blackboard;

		private UnitEssence myEssence;

		private void Start() {
			myEssence = gameObject.GetUnitEssence();
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			if (sequence == null) {
				sequence = new Sequence(false);
				moveToTargetTask = new MoveToTargetTask(blackboard, GetStopDistance());
				sequence.Add(new Gig(MoveIntoPosition));
				sequence.Add(new Gig(RotateIntoPosition));
				sequence.Add(new Gig(SwingMelee));
				sequence.Add(new Gig(WaitCooldown));
				sequence.Add(new Gig(Done));
			}
		}


		public override void Begin() {
			base.Begin();
			
			sequence.RestartSequence();
		}

		public void Update() {
			sequence.UpdateTask();
		}

		/// <summary>
		/// Called whenever the statemachine changes from this state.
		/// Make sure that the NavMeshAgent is disabled.
		/// </summary>
		public override void Exit(Blackboard blackboard) {
			base.Exit(blackboard);
			// since this state use navigation/move to task, we need to try and stop the nav agent
			Utilities.StopHexNavAgentMaybe(blackboard);
		}
		
		private float GetStopDistance() {
			if (attack != null) {
				return attack.IdealRange();
			}
			return 3f;
		}
		
		private bool MoveIntoPosition() {
			moveToTargetTask.TargetTransform = targetObject.transform;
			return moveToTargetTask.UpdateTask();
		}

		bool RotateIntoPosition() {

			moveToTargetTask.TargetTransform = targetObject.transform;
			moveToTargetTask.UpdateTask();

			var targetTurnVector = (targetObject.transform.position.JustXZ() -
				transform.position.JustXZ()).normalized;

			blackboard.TurnVector = targetTurnVector;

			if (attack != null) {
				return attack.CanUseOn(targetObject.gameObject);
			}
			
			float turnForce = CommonUtils.AngleBetweenVectors(targetTurnVector, transform.forward.JustXZ());
			if (turnForce * turnForce < .1f) {
				blackboard.TurnVector = Vector2.zero;
				return true;
			}

			return false;
		}

		bool SwingMelee() {
			if ( attack!=null ) {
				attack.Activate();
				coolDownTimer = attack.immobilizingCooldown;
			} else {
				coolDownTimer = 0;
			}
			
			myEssence.StartCarrying(targetObject);

			return true;
		}

		bool WaitCooldown() {
			// reduce by the delta time
			coolDownTimer -= Time.deltaTime;

			// if the timer is still above zero then we're still waiting
			return coolDownTimer <= 0;
		}

		bool Done() {
			ChangeToNextState();
			return true;
		}
	}
}