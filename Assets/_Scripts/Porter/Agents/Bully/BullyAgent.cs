
using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class BullyAgent : MonoBehaviour {

		public int frameWait = 2;

		public float radius = 300f;

		public Awareness awareness;

		#region Private Variables

		Blackboard blackboard;

		private int fw = 0;

		private ITask task;
		private bool running = false;

		private StateMachine stateMachine;

		private QuickListener listener;

		#endregion

		// State Keys
		public static readonly string PatrolKey = "Patrol";
		public static readonly string EngageKey = "Engage";

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

		void Update() {
			if (running && task != null) {
				task.UpdateTask();
			} else
			// delay for a few frames at first, then start
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

			//IState patrol = new PatrolState(stateMachine, transform.position, radius, awareness);
			//stateMachine.Add(PatrolKey, patrol);

			////IState engage = new EngageState(stateMachine, awareness);
			////stateMachine.Add(EngageKey, engage);

			//stateMachine.ChangeState(PatrolKey);
		}

		private void BlackboardEventCallback(int eventCode, object data) {

			switch ((BlackboardEventType)eventCode) {

				case BlackboardEventType.Damaged:

					// TODO:
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