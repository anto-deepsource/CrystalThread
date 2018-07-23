using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class State : Sequence {

		StateMachine machine;

		public bool PauseOnFinish { get; private set; }
		public bool RestartOnEnter { get; private set; }

		public State(StateMachine machine, bool loopOnFinish = true, bool pauseOnFinish = true, bool restartOnEnter = true) : base(loopOnFinish) {
			this.machine = machine;
			PauseOnFinish = pauseOnFinish;
			RestartOnEnter = restartOnEnter;
		}

		public ChangeStateDelegate EnterAction;

		/// <summary>
		/// Called when the state is exited normally or interrupted
		/// </summary>
		public ChangeStateDelegate ExitAction;

		public void Enter(Blackboard blackboard) {
			if (RestartOnEnter) {
				RestartSequence();
			}
			if ( EnterAction!=null ) {
				EnterAction(blackboard);
			}
		}

		public void Exit(Blackboard blackboard) {
			if (ExitAction != null) {
				ExitAction(blackboard);
			}
		}

		public override bool Update(Blackboard blackboard) {
			bool result = base.Update(blackboard);
			if ( PauseOnFinish ) {
				return false;
			}
			return result;
		}
	}

	public delegate void ChangeStateDelegate(Blackboard blackboard);
}
