using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {

	public class ParallelComponent : PrimitiveTask {

		public bool loopEachOnFinish = true;

		private List<PrimitiveTask> tasks;

		private void Awake() {
			SetupTasksMaybe();
		}

		public void SetupTasksMaybe() {
			if (tasks == null) {
				tasks = new List<PrimitiveTask>();

				foreach (var task in GetComponentsInChildren<PrimitiveTask>()) {
					// only care about direct children
					if (task.transform.parent != transform) {
						continue;
					}
					tasks.Add(task);
				}
			}
		}

		public override void Begin() {
			base.Begin();
			Running = true;
			SetupTasksMaybe();
			foreach (var task in tasks) {
				task.Begin();
			}
		}

		public override void Stop() {
			base.Stop();
			SetupTasksMaybe();
			foreach (var task in tasks) {
				task.Stop();
			}
			Running = false;
		}

		//private void Update() {
		//	if (!Running) {
		//		return;
		//	}
		//	foreach (var task in tasks) {
		//		if (loopEachOnFinish || !taskResults[task]) {
		//			task.UpdateTask(blackboard);
		//		}
		//	}
		//}
	}
}