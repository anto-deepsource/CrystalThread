
using HexMap;
using HexMap.Pathfinding;
using Porter.ProblemSolving;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {

	public class ListenToSubunits : PrimitiveTask {

		private HiveAgent hiveAgent;
		
		public override void Begin() {
			base.Begin();
			if (hiveAgent == null) {
				hiveAgent = GetComponentInParent<HiveAgent>();
			}
			StartListening();
		}

		public override void Stop() {
			base.Stop();
			StopListening();
		}

		private void StartListening() {
			foreach( var subunit in hiveAgent.subunits) {
				UnitEssence theirEssence = subunit.GetUnitEssence();
				theirEssence.Events.Add(this, EssenceCallback);
			}
		}

		private void StopListening() {
			foreach (var subunit in hiveAgent.subunits) {
				UnitEssence theirEssence = subunit.GetUnitEssence();
				theirEssence.Events.RemoveKey(this);
			}
		}

		private void EssenceCallback( int eventCode, object data) {
			Debug.Log((BlackboardEventType)eventCode);
			switch ((BlackboardEventType)eventCode) {
				
				case BlackboardEventType.Staggered:
					break;
				case BlackboardEventType.StaggeredEnd:
					break;
				case BlackboardEventType.Damaged:
					break;
				case BlackboardEventType.ResourcesAdded:
					break;
				case BlackboardEventType.ResourcesRemoved:
					break;
				case BlackboardEventType.Death:
					break;
				case BlackboardEventType.Incapacitated:
					break;
				case BlackboardEventType.PositiveUnitInteraction:
					break;
				case BlackboardEventType.NeutralUnitInteraction:
					break;
				case BlackboardEventType.NegativeUnitInteraction:
					break;
				case BlackboardEventType.AlertToDangerInteraction:
					break;
				case BlackboardEventType.EnemySpotted:
					break;
			}
		}
	}
}