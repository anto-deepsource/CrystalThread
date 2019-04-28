using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickEvent {

	private List<QuickListener> listeners = new List<QuickListener>();

	private Dictionary<object, QuickListener> objectLinks = new Dictionary<object, QuickListener>();

	public void Add( QuickListener listener ) {
		listeners.Add(listener);
	}

	public void Remove( QuickListener listener ) {
		listeners.Remove(listener);
	}

	public void Fire( int eventCode, object data =null ) {
		// Copy the list and iterate through the copy in case the callback removes the listener
		List<QuickListener> temp = new List<QuickListener>(listeners);

		foreach (var listener in temp) {
			// The listener can be set to hear about all events or only certain events
			if (listener.registeredTypes == 0 || IsA(listener.registeredTypes, eventCode)) {
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
	public void Fire( object eventCode, object data = null) {
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

	/// <summary>
	/// Allows an alternative approach to equipping a listener to this event:
	/// Instead of instantiating a QuickListener in the subscribing system
	/// and adding that to the event, pass in the subscribing system/object
	/// and the callback method and a listener will be made automatically. 
	/// </summary>
	/// <param name="key"></param>
	/// <param name="callback"></param>
	/// <param name="types"></param>
	public void Add( object key, QuickListener.QuickCallback callback, 
			int types = QuickListener.QUICK_TYPE_ALL) {
		QuickListener newListener = new QuickListener(callback, types);
		objectLinks[key] = newListener;
		Add(newListener);
	}

	public void Add(object key, QuickListener.QuickCallback callback, params object[] types) {
		QuickListener newListener = new QuickListener(callback, types);
		objectLinks[key] = newListener;
		Add(newListener);
	}

	/// <summary>
	/// Use the same subscribing system/object that was used to add using
	/// Add( object key, QuickListener.QuickCallback callback)
	/// </summary>
	/// <param name="key"></param>
	public void RemoveKey( object key ) {
		QuickListener listener = objectLinks[key];
		Remove(listener);
		objectLinks.Remove(key);
	}
}
