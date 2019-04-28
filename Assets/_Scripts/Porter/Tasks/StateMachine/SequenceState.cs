using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class SequenceState : IState {

		public enum FlowChoice {
			Continue,
			Halt,
			Restart
		}

		#region Public Settings

		public bool restartOnReEnter = true;

		public bool loopOnFinish = true;

		public FlowChoice onAnySuccess = FlowChoice.Continue;

		public FlowChoice onAnyFailure = FlowChoice.Halt;

		#endregion

		#region Private Members

		private List<PrimitiveTask> tasks;

		private bool initialized = false;

		private int currentIndex = 0;
		private PrimitiveTask currentTask;

		#endregion


		private void Start() {
			InitializeMaybe();
		}

		/// <summary>
		/// Returns true if we DID initialize. False if we already were initialized.
		/// </summary>
		/// <returns></returns>
		public bool InitializeMaybe() {
			if ( initialized) {
				return false;
			}
			tasks = new List<PrimitiveTask>();

			foreach (var task in GetComponentsInChildren<PrimitiveTask>()) {
				// only care about direct children
				if (task.transform.parent != transform) {
					continue;
				}
				// only care about initialy enabled children
				if ( !task.gameObject.activeSelf ) {
					continue;
				}
				tasks.Add(task);
				task.Stop();
			}
			StartFirstTask();

			return initialized = true;
		}

		private void StartFirstTask() {
			currentIndex = -1;
			StartNextTask();
		}

		private void StartNextTask() {
			if ( currentTask!=null ) {
				currentTask.Stop();
			}
			if ( tasks.Count == 0 ) {
				Complete();
				return;
			}
			currentIndex++;
			if (currentIndex >= tasks.Count ) {
				// we've reached the end of our sequence
				if (loopOnFinish) {
					currentIndex = 0;
				} else {
					Complete();
					return;
				}
			}
			currentTask = tasks[currentIndex];
			currentTask.Begin();
		}

		private void Complete() {
			Running = false;
			currentTask = null;
			currentIndex = -1;
		}

		public override void Begin() {
			base.Begin();
			if ( InitializeMaybe() ) {

			} else {
				// We were already initialized
				if (restartOnReEnter) {
					StartFirstTask();
				}
			}
			
		}

		public override void Stop() {
			base.Stop();
		}

		private void Update() {
			if ( !Running ) {
				Stop();
				return;
			}

			// One of our tasks is running
			// Check on it -> if it is not running -> start the next task
			if ( !currentTask.Running ) {
				if (currentTask.Successful ) {
					UseFlowChoice(onAnySuccess);
				}
				else {
					UseFlowChoice(onAnyFailure);
				}

			}

		}

		private void UseFlowChoice( FlowChoice choice ) {
			switch (choice) {
				case FlowChoice.Continue:
					StartNextTask();
					break;
				case FlowChoice.Halt:
					Complete();
					break;
				case FlowChoice.Restart:
					StartFirstTask();
					break;
			}
		}
	}
}
