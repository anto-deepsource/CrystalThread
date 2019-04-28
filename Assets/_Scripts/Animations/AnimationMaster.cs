using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAnimation {
	public class AnimationMaster : MonoBehaviour {

		public AnimationHandler[] handlers;
		public event AnimationKeys.EventHandler EventTriggered;

		// Use this for initialization
		void Awake() {
			handlers = GetComponentsInChildren<AnimationHandler>();
			foreach (var handler in handlers) {
				handler.master = this;
			}
		}

		public void Play(AnimationKey key, AnimationData data) {
			foreach (var handler in handlers) {
				handler.Play(key, data);
			}
			//Debug.Log("Play: " + handlers.Length);
		}

		public void StopAll() {
			foreach (var handler in handlers) {
				handler.StopAllAnimations();
			}
		}

		public void TriggerEvent(object sender, AnimationKeys.Event args) {
			if (EventTriggered != null)
				EventTriggered(sender, args);
		}
	}
}