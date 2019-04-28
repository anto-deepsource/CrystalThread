
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
	public class HumanDialog : PrimitiveTask {

		public Awareness awareness;

		public float responseDelay = 0.8f;

		public StateMachine theStateMachine;

		public FollowState theFollowState;

		private Dictionary<UnitEssence, RelationShip> relationships = new Dictionary<UnitEssence, RelationShip>();

		private UnitEssence myEssence;

		private void Awake() {
			myEssence = gameObject.GetUnitEssence();
			myEssence.Events.Add(this, EssenceEventsCallbacks);
			
		}

		private void Update() {
			var friendlies = awareness.GetAnyNearbyFriendlies();
			foreach( var unit in friendlies) {
				MakeNewRelationshipMaybe(unit);
			}

			if ( myEssence.Posture == PostureState.Engaging && !awareness.EnemiesNearby) {
				myEssence.Posture = PostureState.Idle;
			}
		}

		public void EssenceEventsCallbacks( int eventCode, object data ) {
			if ( !Running ) {
				return;
			}
			switch( (BlackboardEventType)eventCode ) {
				case BlackboardEventType.PositiveUnitInteraction: {
					UnitEssence theirEssence = (UnitEssence)data;
					MakeNewRelationshipMaybe(theirEssence);

					PrepareResponse("Not too bad, thank you.");
					break;
				}
				case BlackboardEventType.NeutralUnitInteraction: {
					UnitEssence theirEssence = (UnitEssence)data;
					MakeNewRelationshipMaybe(theirEssence);

					PrepareResponse("It really is something.");
					break;
				}

				case BlackboardEventType.NegativeUnitInteraction: {
					UnitEssence theirEssence = (UnitEssence)data;
					MakeNewRelationshipMaybe(theirEssence);

					PrepareResponse("Excuse me?");
					break;
				}

				case BlackboardEventType.AlertToDangerInteraction: {
						if( myEssence.Posture==PostureState.Idle) {
							myEssence.Posture = PostureState.Alert;
							UnitEssence theirEssence = (UnitEssence)data;
							MakeNewRelationshipMaybe(theirEssence);

							theFollowState.target = theirEssence.gameObject;
							theStateMachine.ChangeState(theFollowState);
							PrepareResponse("Where are they?!");

							// TODO: time out of alert state and go back to idle state after some time if we don't go into engaging
						}
					break;
				}

				case BlackboardEventType.EnemySpotted: {
						myEssence.Posture = PostureState.Engaging;
						var friendlies = awareness.GetAnyNearbyFriendlies();
						bool alertedSomeone = false;
						foreach (var theirEssence in friendlies) {
							if ( theirEssence.Posture == PostureState.Idle ) {
								theirEssence.Inform(BlackboardEventType.AlertToDangerInteraction, myEssence);
								alertedSomeone = true;
							}
						}

						if (alertedSomeone) {
							PrepareResponse("I've spotted an enemy!");
						}
						
						break;
				}
			}
		}

		private void PrepareResponse(string message) {
			StartCoroutine(PauseThenRespond(responseDelay, message));
		}

		private IEnumerator PauseThenRespond(float pause, string message) {
			yield return new WaitForSeconds(pause);

			myEssence.Play(AnimationKey.Speaks, AnimationData.NewMessage(message));
		}
		

		/// <summary>
		/// If one does not already exist, create a new relationship with the given unit (0, Stranger).
		/// </summary>
		/// <param name="unit"></param>
		private void MakeNewRelationshipMaybe(UnitEssence unit ) {
			if ( !relationships.ContainsKey(unit) ) {
				var newRelationShip = new RelationShip(unit);
				// TODO: start the relationship with small random variables depending on each unit's aspects
				relationships[unit] = newRelationShip;

				myEssence.Play(AnimationKey.Speaks, AnimationData.NewMessage("Oh, hello!") );
			}
		}

		private class RelationShip {
			public UnitEssence unit;
			public RelationShipStatus status = RelationShipStatus.Stranger;
			public float familiarity = 0;

			public RelationShip(UnitEssence unit) {
				this.unit = unit;
			}

		}

		public enum RelationShipStatus {
			Stranger,
			Introduced,
			Acquaintance,
			StrongFamiliarity
		}
	}

	
}