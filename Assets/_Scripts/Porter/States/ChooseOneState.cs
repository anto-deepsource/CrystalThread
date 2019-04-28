
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {

	/// <summary>
	/// Has a list of substates that, upon this state triggering, we select one
	/// and run it as a single shot, then change to a preset/constant 'next' state
	/// </summary>
	public class ChooseOneState : IState {

		//public IState nextState;

		//public override string UniqueKey() { return "ChooseOne"; }

		private List< IState> substates;

		private IState currentState;

		private void Start() {
			if (this.stateMachine != null) {
				this.stateMachine = GetComponentInParent<StateMachine>();
			}
		}
		
		public override void Begin() {
			base.Begin();
			SetupStatesMaybe();
			StopAll();
			ChooseState();
			currentState.Begin();
		}

		//public void Update() {
		//	return currentState.UpdateState(blackboard);
		//}

		public override void Exit(Blackboard blackboard) {
			base.Exit(blackboard);
			currentState.Exit(blackboard);
		}

		private void SetupStatesMaybe() {
			if (substates == null) {
				substates = new List<IState >();

				foreach (var state in GetComponentsInChildren<IState>()) {
					// only care about direct children
					if (state.transform.parent != transform) {
						continue;
					}

					substates.Add( state);

				}
			}
		}

		private void StopAll() {
			foreach( var state in substates) {
				state.Stop();
			}
		}

		private void ChooseState() {
			currentState = CommonUtils.RandomChoice<IState>(substates);
		}
	}
}
