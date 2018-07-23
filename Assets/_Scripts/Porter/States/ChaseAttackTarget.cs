using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Porter {

	/// <summary>
	/// State: ChasePlayer(loopOnFinish = true ) {
	///		Sequence(stopOnAnyFails = true) : {
	///			target = GetPlayer()
	///			MoveToTarget(target, stopDistance, moveSpeed, groundLayer)
	///			PlayAnimation(Attack)
	///			WaitForAnimationEvent(Damage)
	///			ApplyDamage() (not implemented yet)
	///		}
	///}
	/// </summary>
	public class ChaseAttackTarget : State {

		float stopDistance = 1;
		bool controlUnitAnimator = true;
		public int groundLayer = LayerMask.NameToLayer("Walkable");

		private AnimationMaster aniMaster;
		private UnitAnimator animator;
		private NavMeshAgent navAgent;

		private bool seenDamageEvent = false;

		public ChaseAttackTarget(StateMachine machine, float stopDistance = 1, bool controlUnitAnimator = true) : 
				base(machine, true, false ) {
			this.stopDistance = stopDistance;
			this.controlUnitAnimator = controlUnitAnimator;

			EnterAction = _Enter;

			Add( new Gig(StartNavAgent) );
			Add( new Gig(MoveToTarget) );
			Add( new Gig(MaybeAttack) );
			Add( new Gig(Attack) );
			Add( new Gig(WaitUntilAttackEnd) );
			// TODO: add apply damage

			ExitAction = ExitOrInterrupt;
			
		}

		void _Enter(Blackboard blackboard ) {
			aniMaster = Utilities.AnimationMaster(blackboard.gameObject);
			aniMaster.EventTriggered += AnimationEventHandler;

			navAgent = Utilities.NavAgent(blackboard.gameObject);
			animator = blackboard.GetComponentInChildren<UnitAnimator>();

			seenDamageEvent = false;
		}

		bool StartNavAgent(Blackboard blackboard) {
			navAgent.enabled = false;// resets it
			navAgent.enabled = true; // makes sure its on
			navAgent.speed = blackboard.moveSpeed;
			navAgent.stoppingDistance = stopDistance;
			
			if (controlUnitAnimator && animator == null)
				controlUnitAnimator = false;

			return true; // always finished right away
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blackboard"></param>
		/// <returns></returns>
		bool MoveToTarget(Blackboard blackboard) {
			// If we're already within the distance, skip anything else and report finished (success)
			float d = CommonUtils.DistanceSquared(blackboard.transform.position, blackboard.target.transform.position);

			if ( d < stopDistance * stopDistance ) {
				return true;
			}

			Rigidbody myBody = Utilities.Rigidbody(blackboard.gameObject);

			if (!navAgent.enabled) {
				return true; // failed to do what we wanted, but still done (true)
			}
			
			if (blackboard.target == null) {
				return true; // failed to do what we wanted, but still done (true)
			}

			navAgent.destination = blackboard.target.transform.position;

			// perform the body movement
			// if there are tranlation issues it may be because this maybe should happen on FixedUpdate
			myBody.MovePosition(
				new Vector3(
					navAgent.nextPosition.x,
					Mathf.Max(myBody.position.y, navAgent.nextPosition.y),
					navAgent.nextPosition.z
				)
			);
			myBody.velocity = new Vector3(0, myBody.velocity.y, 0);
			myBody.angularVelocity = Vector3.zero;

			// try to do the animation
			if (controlUnitAnimator) {

				Ray ray = new Ray(blackboard.transform.position, Vector3.down);
				RaycastHit groundHit;
				animator.IsInAir = !Physics.Raycast(ray, out groundHit, navAgent.height * 1.1f, ~groundLayer);

				animator.IsRunning = (navAgent.nextPosition - blackboard.transform.position).magnitude > 0.01f;
			}

			// we've finished (successfully) if the current distance is less than the stop distance
			d = CommonUtils.DistanceSquared(blackboard.transform.position, blackboard.target.transform.position);
			return d < stopDistance* stopDistance;
		}

		/// <summary>
		/// If we are within range -> Proceed, otherwise restart the sequence
		/// </summary>
		/// <param name="blackboard"></param>
		/// <returns></returns>
		bool MaybeAttack(Blackboard blackboard) {
			float d = CommonUtils.Distance(blackboard.transform.position, blackboard.target.transform.position);
			if ( d < stopDistance ) {
				return true;
			} else {
				RestartSequence();
				return false;
			}
		}

		bool Attack(Blackboard blackboard ) {
			seenDamageEvent = false;
			aniMaster.Play(AnimationKeys.Key.Attack, 0.8f);

			if (controlUnitAnimator) {
				animator.IsRunning = false;
			}

			return true;
		}

		void AnimationEventHandler(object sender, AnimationKeys.Event args) {
			if (args == AnimationKeys.Event.Damage) {
				seenDamageEvent = true;
			}
		}

		bool WaitUntilAttackEnd(Blackboard blackboard ) {
			return seenDamageEvent; // until we've seen the point in the animation where the damage happens we're not finished
		}

		/// <summary>
		/// Called whenever the statemachine changes from this state.
		/// Make sure that the NavMeshAgent is disabled.
		/// </summary>
		/// <param name="blackboard"></param>
		void ExitOrInterrupt(Blackboard blackboard) {
			navAgent.enabled = false;

			aniMaster.EventTriggered -= AnimationEventHandler;

			if (controlUnitAnimator) {
				animator.IsRunning = false;
			}
		}
	}
}
