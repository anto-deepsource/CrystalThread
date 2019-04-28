using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class StateMachine : PrimitiveTask {
		
		//private Dictionary<string, IState> states;

		private IState firstState;

		private IState currentState;

		private Blackboard blackboard;

		public StateMachine(Blackboard blackboard) {
			this.blackboard = blackboard;
		}
		
		public IState CurrentState { get { return currentState; } }

		public override void Begin() {
			base.Begin();
			Running = true;
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			
			SetupStatesMaybe();
			ChangeIntoFirstState();
		}

		public override void Stop() {
			base.Stop();
			SetupStatesMaybe();
			if (currentState != null) {
				currentState.Stop();
			}
			StopAll();
			Running = false;
		}
		
		private void StopAll() {
			foreach (var state in GetComponentsInChildren<IState>()) {
				// only care about direct children
				if (state.transform.parent != transform) {
					continue;
				}
				state.Stop();
			}
		}

   //     private void Update() {
   //         if ( !Running ) {
			//	return;
			//}
   //         if (currentState != null && blackboard != null) {
			//	// update and check whether its done at the same time
			//	//if (currentState.UpdateState(blackboard) || currentState.Successful) {
			//	//	// if it is done, have the machine revert to a default state
			//	//	ChangeIntoFirstState();
			//	//}
			//	currentState.UpdateState(blackboard);
			//}
   //     }

        public void SetupStatesMaybe() {
            if (firstState == null) {
                //states = new Dictionary<string, IState>();
				
				foreach (var state in GetComponentsInChildren<IState>()) {
					// only care about direct children
					if (state.transform.parent != transform ) {
						continue;
					}
					//Add(state.UniqueKey(), state);
					if (firstState==null) {
						firstState = state;
						break;
					}
				}
			}
		}

        public void ChangeIntoFirstState() {
            if (firstState!=null) {
                ChangeState(firstState);
            }
        }

  //      public void Add(string stateName, IState newState ) {
		//	states.Add(stateName, newState);
		//}

		public void ChangeState(IState newState) {

			// if its null -> default to first state
			if (newState == null) {
				newState = firstState;
			}

			if (newState != currentState) {
				if (currentState != null) {
					currentState.Exit(blackboard);
					currentState.Stop();
				}

				currentState = newState;
				currentState.Begin();
			}

		}

		//public void ChangeState( string stateName ) {
		//	IState nextState;
		//	if ( states.TryGetValue(stateName, out nextState ) ) {
		//		if ( nextState != currentState ) {
		//			if ( currentState!=null ) {
		//				currentState.Exit(blackboard);
  //                      currentState.Stop();
		//			}

  //                  currentState = nextState;
  //                  currentState.Begin();
  //              }
		//	} else {
		//		Debug.Log(string.Format("State not found: {0}", stateName));
		//	}
		//}

  //      public bool UpdateTask(Blackboard blackboard) {
		//	if (currentState != null) {
		//		// update and check whether its done at the same time
		//		if ( currentState.UpdateState(blackboard) ) {
		//			// if it is done, have the machine revert to a default state
		//			ChangeIntoFirstState();
		//		}
		//	}
		//	return false; // TODO: there may be some conditions where we want our state machine to be "finished"
		//}
	}

}
