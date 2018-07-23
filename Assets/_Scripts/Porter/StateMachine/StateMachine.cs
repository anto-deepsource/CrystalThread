using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class StateMachine : ITask {

		private Dictionary<string, State> states = new Dictionary<string, State>();

		private State currentState;

		private Blackboard blackboard;

		public StateMachine(Blackboard blackboard) {
			this.blackboard = blackboard;
		}

		public void Add(string stateName, State newState ) {
			states.Add(stateName, newState);
		}

		public void ChangeState( string stateName ) {
			State nextState;
			if ( states.TryGetValue(stateName, out nextState ) ) {
				if ( nextState != currentState ) {
					if ( currentState!=null ) {
						currentState.Exit(blackboard);
					}
					
					nextState.Enter(blackboard);

					currentState = nextState;
				}
			} else {
				Debug.Log(string.Format("State not found: {0}", stateName));
			}
		}

		public bool Update(Blackboard blackboard) {
			if (currentState != null) {
				currentState.Update(blackboard);
			}
			return false; // TODO: there may be some conditions where we want our state machine to be "finished"
		}
	}
}
