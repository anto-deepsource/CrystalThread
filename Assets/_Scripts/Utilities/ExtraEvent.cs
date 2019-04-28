using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraEvent<T> {

	private System.Object owner;

	private Dictionary<object, ExtraCallback> listeners = new Dictionary<object, ExtraCallback>();

	public delegate void ExtraCallback(System.Object sender, T eventCode, System.Object data);

	public ExtraEvent(System.Object owner) {
		this.owner = owner;
	}

	public void Add(object key, ExtraCallback callback ) {
		listeners[key] = callback;
	}

	public void Remove(object key) {
		listeners.Remove(key);
	}

	public void Fire(T eventCode, object data = null) {
		// Copy the list and iterate through the copy in case the callback removes the listener
		var copyListenerPairs = new Dictionary<object, ExtraCallback>(listeners);

		foreach (var pair in copyListenerPairs) {
			var listenerKey = pair.Key;
			var listenerCallback = pair.Value;

			listenerCallback(owner, eventCode, data);
		}
	}
}
