using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableWeapon : MonoBehaviour {

	public InteractionObject interactionObject;

	public Action pickupAnimationEventCallback;

	public Transform root;

	public Transform grabPoint;

	public Rigidbody myBody;
	public Collider myCollider;

	public int pickedUpLayer;

	public bool damageOnCollision = false;

	public DamageSource damage;

	public bool damageOncePerSwing = true;

	/// <summary>
	/// A set of damagables that we've already done damage to this swing, in order to only damage it once.
	/// </summary>
	private HashSet<Damagable> hitDamagables = new HashSet<Damagable>();

	private int startLayer;
	private Transform startParent;

	private void OnEnable() {
		startLayer = gameObject.layer;
		startParent = transform.parent;
	}

	public void Unequip() {
		myCollider.isTrigger = false;
		myBody.isKinematic = false;

		gameObject.layer = startLayer;

		root.SetParent(startParent);
	}

	public void EquipAnimationEvent() {
		
		pickupAnimationEventCallback?.Invoke();

		myCollider.isTrigger = true;
		myBody.isKinematic = true;

		gameObject.layer = pickedUpLayer;
	}

	private void OnTriggerEnter(Collider other) {
		if (!damageOnCollision) {
			return;
		}

		Damagable damagable = other.GetComponentInParent<Damagable>();
		if (damagable != null) {
			if (!hitDamagables.Contains(damagable)) {
				damagable.ApplyDamage(damage);
				hitDamagables.Add(damagable);
			}
			
		}

	}

	public void StartSwing() {
		damageOnCollision = true;
		hitDamagables.Clear();
	}

	public void StopSwing() {
		damageOnCollision = false;
	}
}
