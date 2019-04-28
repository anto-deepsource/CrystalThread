
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class WaitFor : ITask {

		private float duration = -1;

		private float timer = -1f;

		public WaitFor(float duration) {
			this.duration = duration;
		}

        public bool UpdateTask( ) {
			// assume that if timer is lteq to zero this is start/enter
			if ( timer <= 0 ) {
				timer = duration;
			}

			// reduce by the delta time
			timer -= Time.deltaTime;

			// if the timer is still above zero then we're still waiting
			return timer <= 0;
		}
	}
}