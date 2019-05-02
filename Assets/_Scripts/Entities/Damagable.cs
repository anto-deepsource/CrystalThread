using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an abstract component class that further components can implement.
/// Anything in the game world that has health and can therefore be 'damaged'
/// and destroyed.
/// This includes the player, any units, structures, obstacles, some items, etc.
/// 
/// </summary>
public abstract class Damagable : MonoBehaviour {
	
	public abstract void ApplyDamage(DamageSource source);
}
