using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;
using HexMap.Pathfinding;


namespace Porter {
	
	public class MoveTo : State {

		float stopDistance = 1;
		bool controlUnitAnimator = true;
		public int groundLayer = LayerMask.NameToLayer("Walkable");
		
		private UnitAnimator animator;
		private HexNavAgent hexNavAgent;
		private HexNavLocalAgent hexNavLocalAgent;

		private Vector3 lastPosition;

		public MoveTo(StateMachine machine, float stopDistance = 1, bool controlUnitAnimator = true) :
				base(machine, true, false) {
			this.stopDistance = stopDistance;
			this.controlUnitAnimator = controlUnitAnimator;

			EnterAction = _Enter;

			Add(new Gig(StartNavAgent));
			Add(new Gig(MoveToTarget));

			ExitAction = ExitOrInterrupt;

		}

		void _Enter(Blackboard blackboard) {
			lastPosition = blackboard.transform.position;

			hexNavAgent = Utilities.HexNavAgent(blackboard.gameObject);
			hexNavLocalAgent = Utilities.HexNavLocalAgent(blackboard.gameObject);
			animator = blackboard.GetComponentInChildren<UnitAnimator>();
			
		}

		bool StartNavAgent(Blackboard blackboard) {
			//navAgent.enabled = false;// resets it
			hexNavAgent.enabled = true; // makes sure its on
			hexNavAgent.moveSpeed = blackboard.moveSpeed;
			//navAgent.stoppingDistance = stopDistance;

			hexNavLocalAgent.enabled = true; // makes sure its on
			hexNavLocalAgent.moveSpeed = blackboard.moveSpeed;

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

			if (d < stopDistance * stopDistance) {
				return true;
			}

			Rigidbody myBody = Utilities.Rigidbody(blackboard.gameObject);
			
			if (blackboard.target == null) {
				return true; // failed to do what we wanted, but still done (true)
			}
			
			// ------Global ------------
			hexNavAgent.Destination = blackboard.target.transform.position;

			if ( hexNavAgent.Status == PathStatus.Succeeded ) {

			//	Vector3 move = hexNavAgent.MoveVector;
			//	myBody.velocity = move * hexNavAgent.moveSpeed + new Vector3(0, myBody.velocity.y, 0);
			//	//myBody.MovePosition(myBody.position + move * hexNavAgent.moveSpeed * Time.deltaTime);
			//} else {
			//	myBody.velocity = new Vector3(0, myBody.velocity.y, 0);
			}

			// ------Local ------------
			hexNavLocalAgent.Destination = blackboard.target.transform.position;

			if (hexNavLocalAgent.Status == PathStatus.Succeeded ||
					hexNavLocalAgent.Status == PathStatus.Partial) {

				Vector3 move = hexNavLocalAgent.MoveVector;
				myBody.velocity = move * hexNavLocalAgent.moveSpeed + new Vector3(0, myBody.velocity.y, 0);
				//myBody.MovePosition(myBody.position + move * hexNavAgent.moveSpeed * Time.deltaTime);
				if (myBody.velocity.DropY().magnitude > 0.01f) {
					blackboard.transform.rotation = Quaternion.LookRotation(myBody.velocity.DropY());
				}
			} else {
				myBody.velocity = new Vector3(0, myBody.velocity.y, 0);
			}

			//myBody.velocity = new Vector3(0, myBody.velocity.y, 0);
			myBody.angularVelocity = Vector3.zero;

			// try to do the animation
			if (controlUnitAnimator) {

				Ray ray = new Ray(blackboard.transform.position + Vector3.up * 0.5f, Vector3.down);
				RaycastHit groundHit;
				animator.IsInAir = !Physics.Raycast(ray, out groundHit,  1.6f, ~groundLayer);

				animator.IsRunning = (lastPosition - blackboard.transform.position).magnitude > 0.1f;
				lastPosition = blackboard.transform.position;
			}

			// we've finished (successfully) if the current distance is less than the stop distance
			d = CommonUtils.DistanceSquared(blackboard.transform.position, blackboard.target.transform.position);
			return d < stopDistance * stopDistance;
		}
		
		/// <summary>
		/// Called whenever the statemachine changes from this state.
		/// Make sure that the NavMeshAgent is disabled.
		/// </summary>
		/// <param name="blackboard"></param>
		void ExitOrInterrupt(Blackboard blackboard) {
			hexNavAgent.enabled = false;
			
			if (controlUnitAnimator) {
				animator.IsRunning = false;
			}
		}
	}
}
