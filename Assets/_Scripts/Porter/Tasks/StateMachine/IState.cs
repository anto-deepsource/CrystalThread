using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public abstract class IState : PrimitiveTask {

        //public abstract string UniqueKey();

        public StateMachine stateMachine;


		[Tooltip("The state to pass control to if this state finishes or completes. Defaults to the state machine's first state.")]
		public IState proceedingState;

		//public IState(StateMachine machine) {
		//	this.stateMachine = machine;
		//}

		virtual public void OnEnable() {
            if (stateMachine == null) {
                stateMachine = GetComponentInParent<StateMachine>();
            }
        }
		
		virtual public void Exit(Blackboard blackboard) { }

        //abstract public bool UpdateState(Blackboard blackboard);

  //      public void ChangeState( string stateKey ) {
		//	stateMachine.ChangeState(stateKey);
		//}

		public void ChangeToNextState() {
			stateMachine.ChangeState(proceedingState);
		}
	}
}