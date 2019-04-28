using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class CollectResourcesAgent : MonoBehaviour {


		public int frameWait = 2;

		Blackboard blackboard;

		private int fw = 0;

		private ITask task;
		private bool running = false;

		private StateMachine stateMachine;

		private QuickListener listener;

		// State Keys
		public static readonly string MoveTo = "MoveTo";
		//public static readonly string Staggered = "Staggered";
		//public static readonly string Unstaggered = "Unstaggered";

		void Awake() {
			fw = frameWait;
		}

		void Start() {
			blackboard = GetComponent<Blackboard>();
			listener = new QuickListener(BlackboardEventCallback);
			blackboard.events.Add(listener);
		}

		void OnDestroy() {
			blackboard?.events.Remove(listener);
		}

		// Update is called once per frame
		void Update() {
			if (running && task != null) {
				task.UpdateTask();
			} else
			if (fw > 0) {
				fw--;
				if (fw == 0)
					StartTask();
			}
		}

		private void StartTask() {
			running = true;

			CreateStateMachine();

			Parallel root = new Parallel();
			//root.Add(new Gig(AlwaysTask));
			//root.Add(stateMachine);

			task = root;
		}

		private void CreateStateMachine() {
			stateMachine = new StateMachine(blackboard);
			//stateMachine.SetupStatesMaybe();
			////blackboard.target = QueryManager.GetPlayer();

			//SequenceState chasePlayer = new CollectResourcesState(stateMachine );
			//stateMachine.Add(MoveTo, chasePlayer);

			////State staggered = StaggeredState(stateMachine);
			////stateMachine.Add(Staggered, staggered);

			////State unstaggered = UnstaggeredState(stateMachine);
			////stateMachine.Add(Unstaggered, unstaggered);

			//stateMachine.ChangeState(MoveTo);
		}
		
		//private State StaggeredState(StateMachine stateMachine) {
		//	State state = new State(stateMachine, false, true);
		//	state.Add(new Gig(StaggeredGig));

		//	return state;
		//}

		//private bool StaggeredGig(Blackboard blackboard) {
		//	blackboard.Play(AnimationKeys.Key.Staggered);

		//	return true; // finishes successfully, then the state pauses after all tasks
		//}

		//private State UnstaggeredState(StateMachine stateMachine) {
		//	State state = new State(stateMachine, false, true);
		//	state.Add(new Gig(UnstaggeredGig));

		//	return state;
		//}

		//private bool UnstaggeredGig(Blackboard blackboard) {
		//	blackboard.Play(AnimationKeys.Key.StaggeredEnd);
		//	stateMachine.ChangeState(MoveTo);
		//	return true; // finishes successfully, then the state pauses after all tasks, but we should be changing state anyways
		//}

		private void BlackboardEventCallback(int eventCode, object data) {

			switch ((BlackboardEventType)eventCode) {
				case BlackboardEventType.Staggered:
					//stateMachine.ChangeState(Staggered);
					break;
				case BlackboardEventType.StaggeredEnd:
					//stateMachine.ChangeState(Unstaggered);
					break;
				case BlackboardEventType.Damaged:
					break;
				case BlackboardEventType.ResourcesAdded:

					break;
				case BlackboardEventType.ResourcesRemoved:
					break;
				case BlackboardEventType.Death:
					StopAgent();
					break;
			}


		}

		private void StopAgent() {
			running = false;
			StopAllCoroutines();
		}
	}
}