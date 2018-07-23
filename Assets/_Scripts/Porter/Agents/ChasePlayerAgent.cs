using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class ChasePlayerAgent : MonoBehaviour {


		public int frameWait = 2;

		Blackboard blackboard;

		private int fw = 0;

		private ITask task;
		private bool running = false;

		private StateMachine stateMachine;

		private QuickListener listener;

		// State Keys
		public static readonly string ChasePlayer = "ChasePlayer";
		public static readonly string Staggered = "Staggered";
		public static readonly string Unstaggered = "Unstaggered";

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
				task.Update(blackboard);
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
			root.Add(stateMachine);

			task = root;
		}

		private void CreateStateMachine() {
			stateMachine = new StateMachine(blackboard);

			blackboard.target = QueryManager.GetPlayer();

			State chasePlayer = new ChaseAttackTarget(stateMachine,9);
			stateMachine.Add(ChasePlayer, chasePlayer);
			
			State staggered = StaggeredState(stateMachine);
			stateMachine.Add(Staggered, staggered);

			State unstaggered = UnstaggeredState(stateMachine);
			stateMachine.Add(Unstaggered, unstaggered);

			stateMachine.ChangeState(ChasePlayer);
		}

		//private bool AlwaysTask(Blackboard blackboard) {
		//	if (blackboard.staggerTime > 0) {

		//	}
		//	return false;
		//}

		private State StaggeredState(StateMachine stateMachine) {
			State state = new State(stateMachine, false, true);
			state.Add(new Gig(StaggeredGig));

			return state;
		}

		private bool StaggeredGig(Blackboard blackboard) {
			blackboard.Play(AnimationKeys.Key.Staggered );
			
			return true; // finishes successfully, then the state pauses after all tasks
		}

		private State UnstaggeredState(StateMachine stateMachine) {
			State state = new State(stateMachine, false, true);
			state.Add(new Gig(UnstaggeredGig));

			return state;
		}

		private bool UnstaggeredGig(Blackboard blackboard) {
			blackboard.Play(AnimationKeys.Key.StaggeredEnd);
			stateMachine.ChangeState(ChasePlayer);
			return true; // finishes successfully, then the state pauses after all tasks, but we should be changing state anyways
		}

		private void BlackboardEventCallback(int eventCode, object data) {
			
			switch ((BlackboardEventType)eventCode) {
				case BlackboardEventType.Staggered:
					stateMachine.ChangeState(Staggered);
					break;
				case BlackboardEventType.StaggeredEnd:
					stateMachine.ChangeState(Unstaggered);
					break;
			}
		}
	}
}