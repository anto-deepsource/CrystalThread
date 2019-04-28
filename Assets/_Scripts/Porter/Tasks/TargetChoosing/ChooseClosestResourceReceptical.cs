
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class ChooseClosestResourceReceptical : PrimitiveTask {

		public float searchRange = 20f;

		private UnitEssence myEssence;
		private Blackboard blackboard;

		private GameObject closestResourceReceptical = null;

		private void Start() {
			myEssence = gameObject.GetUnitEssence();
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
		}

		public override void Begin() {
			base.Begin();

			var center = transform.position;
			var nearbyProcessingPoints =
				QueryManager.GetNearbyResourceRecepticals(center, searchRange);
			// TODO: choose the one that is nearest considering pathing, not just how the crow flys
			if ( nearbyProcessingPoints.Length == 0 ) {
				closestResourceReceptical = null;
			} else {
				closestResourceReceptical = nearbyProcessingPoints[0];
			}
		}

		private void Update() {
			Successful = false;

			if (closestResourceReceptical!=null ) {
				blackboard.target = closestResourceReceptical;
				Successful = true;
				Debug.Log("Chose " + closestResourceReceptical.name);
			}

			Running = false; // TODO: use pathfinding jobs
		}
		

	}
}