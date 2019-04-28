using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class Sequence : ITask {

		private List<ITask> tasks = new List<ITask>();

		public bool LoopOnFinish { get; private set; }

		private int currentIndex = 0;

		public Sequence(bool loopOnFinish = true) {
			LoopOnFinish = loopOnFinish;
			currentIndex = 0;
        }

		public void Add(ITask task) {
			tasks.Add(task);
		}

         public bool UpdateTask() {
			if ( currentIndex < tasks.Count ) {
				ITask task = tasks[currentIndex];
				if (task.UpdateTask()) {
					currentIndex++;
					if ( currentIndex == tasks.Count ) {
						if (LoopOnFinish ) {
							currentIndex = 0;
						} else {
							return true;
						}
						
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Used by child classes to restart the sequence from the beginning.
		/// Useful for if conditionals.
		/// </summary>
		public void RestartSequence() {
			currentIndex = 0;
		}
	}
}