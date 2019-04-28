using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grabbables {

	public class Grabbable : MonoBehaviour {

		public ExtraEvent<GrabEvent> Events {
			get {
				if (_events == null) {
					_events = new ExtraEvent<GrabEvent>(this);
				}
				return _events;
			}
		}
		ExtraEvent<GrabEvent> _events;

		public void StartGrab() {
			Events.Fire(GrabEvent.GotGrabbed);
		}

		public void StopGrab() {
			Events.Fire(GrabEvent.StopGrabbed);
		}
		
	}
	public enum GrabEvent {
		GotGrabbed,
		StopGrabbed,
	}
}