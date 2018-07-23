using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds and handles all possible 'resources' that a unit can have and carry.
/// </summary>
public class ResourcesComponent : MonoBehaviour {

	private Dictionary<ResourceType, int> quantities = new Dictionary<ResourceType, int>();

	[HideInInspector]
	public QuickEvent events = new QuickEvent();

	public int Count {
		get {
			return quantities.Count;
		}
	}

	public Dictionary<ResourceType, int> Quantities {
		get {
			return quantities;
		}
	}

	#region Arithmetic

	/// <summary>
	/// Returns true if this unit has, at least, the given value of resource.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public bool CanAfford( ResourceType type, int value ) {
		int stored;
		bool result = value <= 0;
		if ( quantities.TryGetValue(type, out stored ) ) {
			result = value <= stored;
		}
		if (result) {
			events.Fire((int)ResourceEvent.CanAfford, type);
		} else {
			events.Fire((int)ResourceEvent.CantAfford, type);
		}
		return result;
	}

	public bool CanAfford(ResourceTypeValuePair pair) {
		return CanAfford(pair.type, pair.value);
	}

	public bool CanAfford(List<ResourceTypeValuePair> costs) {
		foreach( var pair in costs ) {
			if ( !CanAfford(pair.type, pair.value) ) {
				return false;
			}
		}
		return true;
	}

	public void Add( ResourceType type, int value ) {
		int stored;
		if (quantities.TryGetValue(type, out stored)) {
			quantities[type] = value + stored;
		} else {
			quantities[type] = value;
		}
		events.Fire((int)ResourceEvent.Add, type);
	}

	public void Add(ResourceTypeValuePair pair) {
		Add(pair.type, pair.value);
	}

	public void Add(List<ResourceTypeValuePair> pairs) {
		foreach (var pair in pairs) {
			Add(pair.type, pair.value);
		}
	}

	public void Subtract(ResourceType type, int value) {
		int stored;
		if (quantities.TryGetValue(type, out stored)) {
			quantities[type] = stored - value;
		} else {
			// TODO: Negative quantities? maybe thrown an error or do something better
			quantities[type] = -value;
		}
		events.Fire((int)ResourceEvent.Subtract, type);
	}

	public void Subtract(ResourceTypeValuePair pair) {
		Subtract(pair.type, pair.value);
	}

	public void Subtract(List<ResourceTypeValuePair> pairs) {
		foreach (var pair in pairs) {
			Subtract(pair.type, pair.value);
		}
	}

	#endregion
	
}

public enum ResourceType {
	Lightstones,
	Rockpaste,

}

public enum ResourceEvent {
	Add = 2,
	Subtract = 4,
	CanAfford = 8,
	CantAfford = 16
}