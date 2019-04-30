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
}
