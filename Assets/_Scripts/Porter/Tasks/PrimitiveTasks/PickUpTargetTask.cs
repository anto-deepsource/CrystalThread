
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class PickUpTargetTask : PrimitiveTask {

		[Tooltip("The attack or ability to use in order to pick up the object.")]
		public AbstractAttack attack;

		[Tooltip("Only used if no attack is given")]
		public float cooldownTime;
		
		private UnitEssence myEssence;
		private Blackboard blackboard;

		private float lastUsedTime = -100;

		private void Start() {
			myEssence = gameObject.GetUnitEssence();
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
		}

		private void Update() {
			Successful = false;
			if ( GetTimeSinceLastUsed() >= CooldownTime() ) {
				var target = blackboard.target;
				if ( attack != null ) {
					if ( attack.CanUseOn(target) ) {
						ActivateMove();
					}
				} else {
					ActivateMove();
				}
			}
			Running = !Successful; // if we're not successful -> we're stilling running
		}

		private float CooldownTime() {
			if (attack != null) {
				return attack.recastCooldown;
			}
			else {
				return cooldownTime;
			}
		}

		private float GetTimeSinceLastUsed() {
			return Time.time - lastUsedTime;
		}

		private void ActivateMove() {
			if (attack != null) {
				attack.Activate();
			}
			{
				var target = blackboard.target;
				var targetCarryable = target.GetComponent<Carryable>();
				if (targetCarryable != null) {
					myEssence.StartCarrying(targetCarryable);
				} else {
					Debug.Log("Target not carryable.");
				}
				
			}
			lastUsedTime = Time.time;
			Successful = true;
		}


	}
}