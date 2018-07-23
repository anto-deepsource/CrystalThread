using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickEvent {

	private List<QuickListener> listeners = new List<QuickListener>();

	public void Add( QuickListener listener ) {
		listeners.Add(listener);
	}

	public void Remove( QuickListener listener ) {
		listeners.Remove(listener);
	}

	public void Fire( int eventCode, object data ) {
		foreach( var listener in listeners ) {
			// The listener can be set to hear about all events or only certain events
			if (listener.registeredTypes ==0 || IsA( listener.registeredTypes, eventCode ) ) {
				listener.callback(eventCode, data);
			}
		}
	}

	/// <summary>
	/// Convenience overload method that takes an "Object" instead of an int, but assumes
	/// that it is an enum or something else that can be considered an int using only its HashCode.
	/// </summary>
	/// <param name="eventCode"></param>
	/// <param name="data"></param>
	public void Fire( object eventCode, object data) {
		Fire(eventCode.GetHashCode(), data);
	}

	/// <summary>
	/// Using a system of pow2 numbers/types and a flag that can be any combination of those types,
	/// returns true if the given value/flag is of the given type.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="compare"></param>
	/// <returns></returns>
	public static bool IsA(int value, int type) {
		return (value & type) > 0;
	}
}
