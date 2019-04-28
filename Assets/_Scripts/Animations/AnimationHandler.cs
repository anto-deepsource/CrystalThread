using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAnimation {
	public class AnimationHandler : MonoBehaviour {

		public AnimationMaster master;

		virtual public void Play(AnimationKey key, AnimationData data) {

		}

		virtual public void StopAllAnimations() {

		}

		public void TriggerEvent(AnimationKeys.Event args) {
			if (master != null)
				master.TriggerEvent(this, args);
		}
	}
}