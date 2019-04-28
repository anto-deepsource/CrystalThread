using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {
	
	public static UnitEssence GetUnitEssence( this GameObject gameObject ) {
		var target = gameObject.GetComponent<UnitEssence>();
		if ( target==null ) {
			target = gameObject.GetComponentInParent<UnitEssence>();
		}
		if (target == null) {
			target = gameObject.GetComponentInChildren<UnitEssence>();
		}
		// TODO: check that there is one and throw a proper warning
		return target;
	}


	public static HealthComponent GetHealthComponent(this GameObject gameObject) {
		var target = gameObject.GetComponent<HealthComponent>();
		if (target == null) {
			target = gameObject.GetComponentInParent<HealthComponent>();
		}
		// TODO: check that there is one and throw a proper warning
		return target;
	}
}
