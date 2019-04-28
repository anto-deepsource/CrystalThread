using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {

	/// <summary>
	/// Picks an enemy, moves into position, and attacks the target.
	/// Evaluates the situation and picks the target upon entering the state and
	/// again after attacking that target.
	/// Continuously evaluates other targets and reevaluates upon:
	/// -Taking damage
	/// -Becoming aware of a new enemy
	/// -Losing track of the current target
	/// -Target dying
	/// </summary>
	public class EngageState : IState {

        //public float attackDistance = 3;

        //[Tooltip("A reference to a point relative to the agent, used to position the agent relative to the target.")]
        //public Transform targetPosition;

        public Awareness awareness;

		public AbstractAttack attack;

		public float strafeDistance = 10f;

		public float pathPointThreshold = .4f;




		//public static readonly string Key = "Engage";

  //      public override string UniqueKey() { return Key; }

        MoveToTargetTask moveToTargetTask;
		Sequence sequence;

		//MoveToPointTask moveToPointTask;

		private float coolDownTimer = -1f;

		private Blackboard blackboard;

        private void Start() {
            SetupSequence();
        }

        private void SetupSequence() {
            if ( this.awareness == null ) {
                awareness = GetComponentInChildren<Awareness>();
            }
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			if (sequence==null) {
				sequence = new Sequence(true);
				sequence.Add(new Gig(PickTarget));
				moveToTargetTask = new MoveToTargetTask(blackboard, GetStopDistance());
				sequence.Add(new Gig(MoveTowardsMainEnemy));
				//sequence.Add(moveToTargetTask);
				//moveToPointTask = new MoveToPointTask(pathPointThreshold);
				sequence.Add(new Gig(RotateToAttackPosition));
				sequence.Add(new Gig(SwingMelee));
				sequence.Add(new Gig(WaitCooldown));
				sequence.Add(new Gig(PickStrafePoint));
				sequence.Add(new Gig(StrafeUntilNextAttack));
			}
        }

        private float GetStopDistance() {
            return attack.IdealRange();
        }

        private bool PickTarget() {

			if ( awareness.EnemiesNearby ) {
				moveToTargetTask.TargetTransform = awareness.MainEnemyTarget.transform;
				//blackboard.target = awareness.MainEnemyTarget;
				moveToTargetTask.stopDistance = GetStopDistance();

				return true;
			} else {
				ChangeToNextState();
				//Successful = true;
				return false;
			}
		}
		
		private bool MoveTowardsMainEnemy( ) {
			if (!awareness.EnemiesNearby) {
				ChangeToNextState();
				//Successful = true;
				return false;
			}
			moveToTargetTask.TargetPoint = awareness.MainEnemyPosition;
			return moveToTargetTask.UpdateTask();
		}

		private void MoveToAndTurnToMainEnemyTarget( ) {
			if (!awareness.EnemiesNearby) {
				ChangeToNextState();
				//Successful = true;
				return;
			}
			moveToTargetTask.TargetPoint = awareness.MainEnemyPosition;
			moveToTargetTask.UpdateTask();

			//moveToPointTask.target = attack.GetCastPosition(awareness.MainEnemyTarget.transform.position);
			//moveToPointTask.UpdateTask(blackboard);

			var targetTurnVector = (awareness.MainEnemyTarget.transform.position.JustXZ() -
				transform.position.JustXZ()).normalized;

			blackboard.TurnVector = targetTurnVector;
		}

		bool RotateToAttackPosition( ) {
			
			if (!awareness.EnemiesNearby) {
				ChangeToNextState();
				//Successful = true;
				return false;
			}
			MoveToAndTurnToMainEnemyTarget();
			
			return attack.CanUseOn(awareness.MainEnemyTarget);

			//float turnForce = CommonUtils.AngleBetweenVectors(targetTurnVector, transform.forward.JustXZ());
			//if ( turnForce*turnForce < .1f ) {
			//	blackboard.turnVector = Vector2.zero;
			//	return true;
			//}

			//return false;
		}

		bool SwingMelee( ) {
			MoveToAndTurnToMainEnemyTarget();
			attack.Activate();
			coolDownTimer = attack.immobilizingCooldown;
			return true;
		}

        bool WaitCooldown( ) {
			MoveToAndTurnToMainEnemyTarget();
			// reduce by the delta time
			coolDownTimer -= Time.deltaTime;

            // if the timer is still above zero then we're still waiting
            return coolDownTimer <= 0;
        }

		Vector2 strafeDirection;

		bool PickStrafePoint() {
			if (!awareness.EnemiesNearby) {
				ChangeToNextState();
				//Successful = true;
				return false;
			}
			Vector2 ourCurrentPosition = transform.position.JustXZ();
			Vector2 theirPosition = awareness.MainEnemyTarget.transform.position.JustXZ();
			Vector2 vector = ourCurrentPosition - theirPosition;
			Vector2 pVector = new Vector2(-vector.y, vector.x);
			float r = Random.value*4f - 2f;
			strafeDirection = (vector+ pVector*r).normalized;

			return true;
		}

		bool StrafeUntilNextAttack() {
			if (!awareness.EnemiesNearby) {
				ChangeToNextState();
				//Successful = true;
				return false;
			}
			var theirPosition = awareness.MainEnemyTarget.transform.position;
			moveToTargetTask.TargetPoint = theirPosition + (strafeDirection * strafeDistance).FromXZ();
			moveToTargetTask.UpdateTask();

			//moveToPointTask.target = theirPosition + (strafeDirection * strafeDistance).FromXZ();

			//moveToPointTask.UpdateTask(blackboard);
			blackboard.TurnVector = (awareness.MainEnemyTarget.transform.position.JustXZ() -
				transform.position.JustXZ()).normalized;
			return !attack.IsOnCooldown;
		}

		public override void Begin() {
			base.Begin();
			
			SetupSequence();
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
	}
}