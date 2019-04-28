using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
	public class MoveToTarget : MonoBehaviour {


		//public int frameWait = 2;

		//Blackboard blackboard;

		//private int fw = 0;

		//private ITask task;
		//private bool running = false;

		//private StateMachine stateMachine;

		//private QuickListener listener;

		//// State Keys
		//public static readonly string MoveTo = "MoveTo";
		//public static readonly string Staggered = "Staggered";
		//public static readonly string Unstaggered = "Unstaggered";

		//void Awake() {
		//	fw = frameWait;
		//}

		//void Start() {
		//	blackboard = GetComponent<Blackboard>();
		//	listener = new QuickListener(BlackboardEventCallback);
		//	blackboard.events.Add(listener);
		//}

		//void OnDestroy() {
		//	blackboard?.events.Remove(listener);
		//}

		//// Update is called once per frame
		//void Update() {
		//	if (running && task != null) {
		//		task.UpdateTask();
		//	} else
		//	if (fw > 0) {
		//		fw--;
		//		if (fw == 0)
		//			StartTask();
		//	}
		//}

		//private void StartTask() {
		//	running = true;

		//	CreateStateMachine();

		//	Parallel root = new Parallel();
		//	//root.Add(new Gig(AlwaysTask));
		//	//root.Add(stateMachine);

		//	task = root;
		//}

		//private void CreateStateMachine() {
		//	stateMachine = new StateMachine(blackboard);

		//	//blackboard.target = QueryManager.GetPlayer();

		//	SequenceState chasePlayer = MoveToTargetState(stateMachine);
		//	stateMachine.Add(MoveTo, chasePlayer);

		//	SequenceState staggered = StaggeredState(stateMachine);
		//	stateMachine.Add(Staggered, staggered);

		//	SequenceState unstaggered = UnstaggeredState(stateMachine);
		//	stateMachine.Add(Unstaggered, unstaggered);

		//	stateMachine.ChangeState(MoveTo);
		//}

		////private bool AlwaysTask(Blackboard blackboard) {
		////	if (blackboard.staggerTime > 0) {

		////	}
		////	return false;
		////}

		////Add(new MoveToTargetTask(0));

		//private SequenceState MoveToTargetState(StateMachine stateMachine) {
		//	SequenceState state = new SequenceState(stateMachine, true, false);
		//	state.Add(new MoveToTargetTask(1));

		//	return state;
		//}
		
		//private SequenceState StaggeredState(StateMachine stateMachine) {
		//	SequenceState state = new SequenceState(stateMachine, false, true);
		//	state.Add(new Gig(StaggeredGig));

		//	return state;
		//}

		//private bool StaggeredGig(Blackboard blackboard) {
		//	blackboard.Play(AnimationKey.Staggered, AnimationData.none);

		//	return true; // finishes successfully, then the state pauses after all tasks
		//}

		//private SequenceState UnstaggeredState(StateMachine stateMachine) {
		//	SequenceState state = new SequenceState(stateMachine, false, true);
		//	state.Add(new Gig(UnstaggeredGig));

		//	return state;
		//}

		//private bool UnstaggeredGig(Blackboard blackboard) {
		//	blackboard.Play(AnimationKey.StaggeredEnd, AnimationData.none);
		//	stateMachine.ChangeState(MoveTo);
		//	return true; // finishes successfully, then the state pauses after all tasks, but we should be changing state anyways
		//}

		//private void BlackboardEventCallback(int eventCode, object data) {

		//	switch ((BlackboardEventType)eventCode) {
		//		case BlackboardEventType.Staggered:
		//			stateMachine.ChangeState(Staggered);
		//			break;
		//		case BlackboardEventType.StaggeredEnd:
		//			stateMachine.ChangeState(Unstaggered);
		//			break;
		//		case BlackboardEventType.Damaged:
		//			//Debug.Log("Agent Damaged");
		//			break;
		//		case BlackboardEventType.Death:
		//			StopAgent();
		//			break;
		//	}
		//}

		//private void StopAgent() {
		//	running = false;
		//	StopAllCoroutines();
		//	//HexNavAgent hexNavAgent = Utilities.HexNavAgent(blackboard.gameObject);
		//	//hexNavAgent.enabled = false;
		//	HexNavAgent hexNavAgent = Utilities.HexNavAgent(blackboard.gameObject);
		//	hexNavAgent.Stop();
		//	hexNavAgent.enabled = false;
		//}
	}
}