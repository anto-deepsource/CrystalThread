using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquippableWeaponController : AbstractInteractableActuator {

	public float range = 6f;

	public UnitEssence myEssence;

	public InteractionSystem interactionSystem;
	public FullBodyBipedIK ik; // Reference to the FBBIK component

	public Transform idealInteractionPosition;
	public float idealInteractionPositionTolerance = 0.2f;
	public float idealInteractionVelocityTolerance = 0.5f;

	public Transform rightHand;
	public Transform rightHandTarget;
	public Transform rightHandTargetIdlePosition;

	public float handPositionSmoothing = 0.1f;
	public float smoothingTime = 1f;
	private float smoothTimer = 0;

	public MeleeWeaponAnimator weaponAnimator;

	public AnimationClip idleAnimation;
	public AnimationClip attackAnimation;

	private bool performingPickup = false;
	//private bool movingIntoPosition = false;

	EquippableWeapon targetPickup = null;

	private bool hasSomethingEquipped = false;

	private EquippableWeapon currentlyEquippedWeapon = null;
	private IKEffector handEffector;

	private bool smoothHandPosition = false;

	private void Start() {
		weaponAnimator.Play(idleAnimation);
		weaponAnimator.Events.Add(this, WeaponAnimatorCallbacks);
	}

	private void WeaponAnimatorCallbacks(object sender, MeleeWeaponEvent eventCode, object data) {
		switch (eventCode) {
			case MeleeWeaponEvent.StartSwing:
				//currentlyEquippedWeapon.damageOnCollision = true;
				break;
			case MeleeWeaponEvent.EndSwing:
				currentlyEquippedWeapon.damageOnCollision = false;
				break;
		}
	}

	public override ActuatorTarget GetBestTarget() {
		if (!hasSomethingEquipped) {
			// look for equippable weapons nearby
			EquippableWeapon[] allWeapons = GameObject.FindObjectsOfType<EquippableWeapon>();

			float nearestDistance = 0;
			EquippableWeapon nearestWeapon = null;

			foreach (var weapon in allWeapons) {
				float distance = (transform.position - weapon.transform.position).sqrMagnitude;
				if (nearestWeapon == null || distance < nearestDistance) {
					nearestDistance = distance;
					nearestWeapon = weapon;
				}
			}

			if (nearestWeapon != null && nearestDistance < range) {
				return new ActuatorTarget(nearestWeapon, this, nearestDistance, "Pick up" );
			}
		}
		return ActuatorTarget.None();
	}

	public override bool IsImmediateInteraction() {
		if (hasSomethingEquipped) {
			return true;
		}
		return false;
	}

	public override string GetImmediateInteractionLabel() {
		if (hasSomethingEquipped) {
			return "Drop";
		}
		return "";
	}

	public override bool UseInteractionEventImmediateMaybe() {

		if (hasSomethingEquipped) {
			UnequipCurrentlyEquippedWeapon();
			return true;
		}
		return false;
	}

	override public void StartMoveIntoIdealPositionBestTargetEvent(MonoBehaviour target) {
		performingPickup = true;
		myEssence.OverrideControl(this);
		targetPickup = target as EquippableWeapon;
	}

	override public void UseInteractionBestTargetEvent(MonoBehaviour target) {
		ActualPerformPickup();
		//StartMoveIntoIdealPosition(target as EquippableWeapon);
	}
	
	void Update () {
		if (stopInteractionAnimation) {
			interactionSystem.StopInteraction(FullBodyBipedEffector.RightHand);
			stopInteractionAnimation = false;
		}
		
		if (performingPickup) {

			//if (movingIntoPosition) {
			//	MoveIntoIdealPosition();
			//}
			
		}
		else
		if (hasSomethingEquipped) {

			if (Input.GetMouseButtonDown(1)) {
				StartWeaponSwing();
			}
			else {
				if (smoothHandPosition) {
					SmoothHandPositionUpdate();
				} else {
					RigidHandPositionUpdate();
				}
			}

			
			//if ( !Mathf.Approximately(0, handEffector.positionWeight)) {
			//	handEffector.positionWeight *= .8f;
			//} else {
			//	Debug.Log("Remove effector");
			//	handEffector.target = null;
			//}

		}

	}


	//private void StartMoveIntoIdealPosition(EquippableWeapon nearestWeapon) {
	//	performingPickup = true;
	//	//movingIntoPosition = true;
	//	myEssence.OverrideControl(this);
	//	targetPickup = nearestWeapon;
	//}

	///// <summary>
	///// Take over control of the character and move them into position
	///// </summary>
	//private void MoveIntoIdealPosition() {
	//	//bool moveIntoNextPhase = false;

	//	var currentVelocity = myEssence.GetCurrentVelocity();
	//	bool withinIdealVelocity = currentVelocity.sqrMagnitude < idealInteractionVelocityTolerance * idealInteractionVelocityTolerance;

	//	var vector = targetPickup.transform.position.JustXZ() - idealInteractionPosition.position.JustXZ();

	//	// if the distance from the target point to the pickup is less than a certain distance -> move into next phase
	//	float tolerance = idealInteractionPositionTolerance * idealInteractionPositionTolerance;
	//	bool withinIdealPosition = vector.sqrMagnitude < tolerance;
	//	if (withinIdealPosition) {
	//		myEssence.MoveVector = Vector3.zero;
	//	} else {
	//		myEssence.MoveVector = vector.FromXZ();
	//	}
		

	//	vector = targetPickup.transform.position.JustXZ() - transform.position.JustXZ();
	//	myEssence.TurnVector = vector.normalized;

	//	//// if the distance from us to the pickup is less than the distance to the ideal position -> move into next phase
	//	//var idealPositionRange = (idealInteractionPosition.position.JustXZ() - transform.position.JustXZ()).sqrMagnitude;
	//	//if (vector.sqrMagnitude < idealPositionRange) {
	//	//	moveIntoNextPhase = true;
	//	//}

	//	if (withinIdealVelocity && withinIdealPosition) {
	//		ActualPerformPickup();
	//	}
	//}

	private void ActualPerformPickup() {
		//movingIntoPosition = false;
		myEssence.MoveVector = Vector3.zero;
		myEssence.TurnVector = Vector2.zero;
		myEssence.PickUpEqippableWeapon(targetPickup);
		interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, targetPickup.interactionObject, true);

		weaponAnimator.Play(idleAnimation);

		targetPickup.pickupAnimationEventCallback = PickUpAnimationEventCallback;
	}

	bool stopInteractionAnimation = false;

	private void PickUpAnimationEventCallback() {
		//Debug.Log("EquipAnimationEvent");

		rightHandTarget.position = targetPickup.grabPoint.position;

		targetPickup.root.SetParent(rightHand);
		//targetPickup.root.position = targetPickup.root.TransformPoint(targetPickup.grabPoint.position);
		//targetPickup.root.localPosition = Vector3.zero;
		targetPickup.root.localPosition = -targetPickup.grabPoint.localPosition * targetPickup.root.localScale.x;
		//targetPickup.root.localRotation = Quaternion.identity;
		targetPickup.root.localRotation = Quaternion.Inverse(targetPickup.grabPoint.localRotation);

		stopInteractionAnimation = true;
		//interactionSystem.StopInteraction(FullBodyBipedEffector.RightHand);

		myEssence.RelinquishControl(this);
		performingPickup = false;


		hasSomethingEquipped = true;
		currentlyEquippedWeapon = targetPickup;

		smoothHandPosition = true;
		smoothTimer = 0;

		handEffector = ik.solver.GetEffector(FullBodyBipedEffector.RightHand);
		handEffector.target = rightHandTarget;
		handEffector.positionWeight = 0.9f;
	}

	private void UnequipCurrentlyEquippedWeapon() {
		currentlyEquippedWeapon.Unequip();
		hasSomethingEquipped = false;
		currentlyEquippedWeapon = null;

		//handEffector = ik.solver.GetEffector(FullBodyBipedEffector.RightHand);
		handEffector.target = null;
		handEffector.positionWeight = 0f;
		handEffector.rotationWeight = 0f;
	}
	
	private void SmoothHandPositionUpdate() {
		rightHandTarget.position = Vector3.Slerp(rightHandTarget.position, rightHandTargetIdlePosition.position, handPositionSmoothing);
		rightHandTarget.rotation = Quaternion.Slerp(rightHandTarget.rotation, rightHandTargetIdlePosition.rotation, handPositionSmoothing);
		handEffector.target = rightHandTarget;
		handEffector.positionWeight = .9f;
		handEffector.rotationWeight = .9f;
		smoothTimer += Time.deltaTime;
		if (smoothTimer>=smoothingTime) {
			smoothHandPosition = false;
		}
	}

	private void RigidHandPositionUpdate() {
		handEffector.target = rightHandTargetIdlePosition;
		handEffector.positionWeight = .9f;
		handEffector.rotationWeight = .9f;

	}

	private void StartWeaponSwing() {
		weaponAnimator.Play(attackAnimation);
		currentlyEquippedWeapon.StartSwing();
		//currentlyEquippedWeapon.damageOnCollision = true;

	}
}
