using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

/// <summary>
/// Adds 'attack' or 'ability' behaviors to a game object.
/// Provides info about the 
/// -range of the attack 
/// -the immobilizing cooldown of the attack on the caster,
/// -the recast cooldown
/// 
/// Provides an 'Activate' method that causes the unit to use this attack
/// </summary>
public class AbstractAttack : MonoBehaviour {

	#region Public Members

	[Tooltip("The animation that the attack triggers on the unit.")]
	public AnimationKey triggersAnimation;

	[Tooltip("The animation mods, if any, that it uses on the triggered animation.")]
	public List<AnimationKeys.Mod> animationMods;

	public float amount = 3;

	public float deepAmount = 1;

	//public float range = 4;

	public float speed = 2.5f;

	public bool friendlyFire = false;

	[Tooltip("The time, in seconds, that the attack takes to refresh after being cast.")]
	public float recastCooldown = 2;

	[Tooltip("The time, in seconds, that the attack immobilizes the caster immediately after being cast.")]
	public float immobilizingCooldown = 1;

	public Transform targetPoint;

	public LayerMask hitLayer;

	public PolyShape hitBoxShape;

	#endregion

	#region Properties

	public bool IsOnCooldown { get { return cooldownTimer > 0; } }

	#endregion

	#region Private Members

	private float cooldownTimer = 0;

	//private float castTimer = 0;

	#endregion

	#region Private Component References

	private UnitEssence essence;

	#endregion

	private void Update() {
		if (IsOnCooldown) {
			cooldownTimer -= Time.deltaTime;
		}
		//if (castTimer>0 ) {
		//	castTimer -= Time.deltaTime;
		//	if ( castTimer <= 0 ) {
		//		ApplyDamage();
		//	}
		//}
	}

	public float IdealRange() {
		return targetPoint.localPosition.magnitude;
	}

	public virtual void Activate() {
		// refuse to activate if the cooldown is still decaying
		if (IsOnCooldown) {
			//Debug.Log("Attack still on cooldown");
			return;
		}

		if ( essence==null) {
			essence = gameObject.GetUnitEssence();
		}

		var data = new AnimationData() {
			key = triggersAnimation,
			data = 1f / speed, // playLength
		};

		essence.Play(triggersAnimation, data);
		cooldownTimer = recastCooldown;
		//castTimer = speed * 0.5f;
	}

	private HashSet<UnitEssence> hitUnits = new HashSet<UnitEssence>();

	public virtual void ApplyDamage() {

		hitUnits.Clear();

		GameObject[] results = hitBoxShape.Cast(hitLayer);

		foreach (var result in results) {
			UnitEssence target = result.GetUnitEssence();
			if (target != null && target!=essence && !hitUnits.Contains(target)) {
				hitUnits.Add(target);

				if ( !friendlyFire && target.faction==essence.faction) {
					continue;
				}

				Damagable damagable = target.GetComponent<Damagable>();
				if (damagable != null) {
					DamageSource damage = new DamageSource() {
						amount = amount,
						deepAmount = deepAmount,
						sourceObject = essence.gameObject
					};
					damagable.ApplyDamage(damage);
				}
				
			}
		}

		// Alert nearby units to the attack
		if (hitUnits.Count>0) {

		}
	}

	/// <summary>
	/// Calculates and returns the world position where the caster
	/// should stand in order to hit the target unit
	/// </summary>
	/// <returns></returns>
	public Vector3 GetCastPosition(Vector3 target) {
		Vector2 ourCurrentPosition = transform.position.JustXZ();
		Vector2 theirPosition = target.JustXZ();
		Vector2 vector = ourCurrentPosition - theirPosition;
		float idealTargetDistance = targetPoint.localPosition.magnitude;
		return (theirPosition + vector.normalized * idealTargetDistance).FromXZ();
	}

	public bool CanUseOn(GameObject target ) {
		return hitBoxShape.CheckAgainstObject(target);
	}
}
