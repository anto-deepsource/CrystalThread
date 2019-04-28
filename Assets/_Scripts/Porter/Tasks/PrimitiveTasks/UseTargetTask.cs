
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class UseTargetTask : PrimitiveTask {
		
		private UnitEssence myEssence;
		private Blackboard blackboard;
		
		//private void Start() {
		//	myEssence = gameObject.GetUnitEssence();
		//	if (blackboard == null) {
		//		blackboard = GetComponentInParent<Blackboard>();
		//	}
		//}

		public override void Begin() {
			base.Begin();
			myEssence = gameObject.GetUnitEssence();
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			Successful = false;

			ActivateUse();
		}

		private void ActivateUse() {
			// 'Use' can mean one of a couple things:
			// - If we're carrying something then its 'Use this object on the target'

			if (myEssence.currentlyCarriedObject != null) {

				// One thing is: If we're carrying a resourceable and the target is a resource receptical
				Resourceable resourceable = myEssence.currentlyCarriedObject.GetComponent<Resourceable>();
				ResourceReceptical receptical = blackboard.target.GetComponent<ResourceReceptical>();
				if (resourceable != null && receptical != null) {
					receptical.ProcessResourceable(resourceable);
					Successful = true;
				}
			}
		}

		private void Update() {
			Running = false;
		}
		

	}
}