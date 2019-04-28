
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {

	/// <summary>
	/// A base class for States that extends the Parallel task and allows tasks to be run concurrently
	/// </summary>
	public class ParallelState : IState {

        public bool loopEachOnFinish = true;

        Parallel rootParallel;
        
        //public override string UniqueKey() { return "Parallel"; }

        public ParallelState(StateMachine machine, bool loopEachOnFinish = true) {
            Initialize(loopEachOnFinish);
        }

        public void Initialize(bool loopEachOnFinish = true) {
            this.rootParallel = new Parallel(loopEachOnFinish);

            if (this.stateMachine == null) {
                this.stateMachine = GetComponentInParent<StateMachine>();
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            if ( rootParallel==null) {
                rootParallel = new Parallel(loopEachOnFinish);
            }
        }
		
		/// <summary>
		/// Adds the task to the root parallel, to be run concurrently.
		/// </summary>
		/// <param name="task"></param>
		public void Add(ITask task) {
			rootParallel.Add(task);
		}
		
		private void Update() {
			rootParallel.UpdateTask();
		}
	}
	
}
