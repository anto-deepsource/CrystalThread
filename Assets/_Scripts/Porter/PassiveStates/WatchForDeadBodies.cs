
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;
using System.Linq;

namespace Porter {
	public class WatchForDeadBodies : PrimitiveTask {

		public Awareness awareness;

		public IState onSpotted;

		//private HashSet<GameObject> seenBodies = new HashSet<GameObject>();

		private UnitEssence myEssence;

		public override void Begin() {
			base.Begin();
			if (myEssence == null) {
				myEssence = gameObject.GetUnitEssence();
			}
		}


		private void Update() {
			if (onSpotted == null) {
				Debug.LogWarning("State was null");
				return;
			}

			var nearbyBodies = from pair in awareness.units
						where pair.Value.unit.IsDead
						select pair.Key;

			Successful = false;

			foreach( var body in nearbyBodies) {
				Successful = true;
				if ( !onSpotted.Running ) {
					var essence = gameObject.GetUnitEssence();
					essence.Play(AnimationKey.Alerted, AnimationData.none);
					//essence.Inform(BlackboardEventType.EnemySpotted, awareness.MainEnemyPosition);

					onSpotted.stateMachine.ChangeState(onSpotted);
					//seenBodies.Add(body);

					var blackboard = gameObject.GetComponentInParent<Blackboard>();
					blackboard.target = body;

					myEssence.Posture = PostureState.Alert;
					break;
				}
			}

			Running = false;
			
		}
	}
}