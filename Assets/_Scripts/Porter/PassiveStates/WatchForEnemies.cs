using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
	public class WatchForEnemies : PrimitiveTask {

		public Awareness awareness;
		
		public IState onSpottedEnemy;

		private bool lastEnemiesNearby = false;

		private UnitEssence myEssence;

		public override void Begin() {
			base.Begin();
			if (myEssence == null) {
				myEssence = gameObject.GetUnitEssence();
			}
		}

		private void Update() {
			if (onSpottedEnemy == null) {
				Debug.LogWarning("State was null");
				return;
			}

			if (awareness.EnemiesNearby && !onSpottedEnemy.Running ) {
				// alert the state machine to engage
				var essence = gameObject.GetUnitEssence();
				essence.Play(AnimationKey.Alerted, AnimationData.none);
				essence.Inform(BlackboardEventType.EnemySpotted, awareness.MainEnemyPosition);

				onSpottedEnemy.stateMachine.ChangeState(onSpottedEnemy);

				myEssence.Posture = PostureState.Engaging;
			}

			//bool justSpottedAnEnemy = !lastEnemiesNearby && awareness.EnemiesNearby;
			////bool notEngaging = onSpottedEnemy != onSpottedEnemy.stateMachine.CurrentState;
			//if (justSpottedAnEnemy) {
				
			//}

			//lastEnemiesNearby = awareness.EnemiesNearby;

			Successful = awareness.EnemiesNearby;
			Running = false;
		}
	}
}