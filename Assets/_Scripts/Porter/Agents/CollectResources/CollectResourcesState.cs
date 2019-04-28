using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	//public class CollectResourcesState : SequenceState {

 //       //public static readonly string Key = "CollectResources";

 //       //public override string UniqueKey() { return Key; }

	//	public CollectResourcesState(StateMachine machine) :
	//	  base(machine, true, false) {

	//		//Add(new Gig(PickResource));
	//		//Add(new MoveToTargetTask(0)); // uses hexnavagent. ensure that hexnavagent is stopped when we're done
	//		//Add(new Gig(CollectResource));

	//	}

	//	//bool PickResource() {
	//	//	GameObject[] pickups = QueryManager.GetNearbyPickups( blackboard.transform.position, 4 );
	//	//	blackboard.target = pickups[0];
	//	//	return true;
	//	//}

	//	//bool CollectResource() {
	//	//	//Debug.Log("Get Resource");
	//	//	return true;
	//	//}

	//	/// <summary>
	//	/// Called whenever the statemachine changes from this state.
	//	/// Make sure that the NavMeshAgent is disabled.
	//	/// </summary>
	//	public override void Exit(Blackboard blackboard) {
	//		base.Exit(blackboard);
	//		Utilities.StopHexNavAgentMaybe(blackboard);
	//	}
	//}
}