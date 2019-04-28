using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class HumanoidMeshAnimator : AnimationHandler {

	public float forwardSpeedToAnimationScale = 8f;
	public float sidewaysSpeedToAnimationScale = 8f;
	public float turnSpeedToAnimationScale = 8f;

	//[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	//public LayerMask groundLayer;
	//public float maxFootReach = 2;
	//public Transform rightFootRayCastOrigin;
	//public Transform leftFootRayCastOrigin;
	//public Vector3 footOffset = Vector3.zero;

	public Grounded groundedComponent;

	[Tooltip("A component that, if assigned, will fire events that the animator can react to.")]
	public PlayerWhip playerWhip;

	public float maxUpperBodyWeight = 1f;

	public HitWorldCrossHairs crossHairs;
	public FullBodyBipedIK ik; // Reference to the FBBIK component

	public UnitEssence unitEssence;

	Animator animator;

	Vector3 lastPosition;
	float lastRotationY = 0;

	bool inLocomotion = false;

	bool jumpBoosting = false;

	//bool grounded = false;

	//Vector3 leftFootTargetPosition;
	//float lastLeftFootWeight = 0;
	//Vector3 rightFootTargetPosition;
	//float lastRightFootWeight = 0;

	public int upperBodyLayerIndex = 2;

	public float upperBodyWeightChangeScale = 0.1f;

	private float targetUpperBodyWeight = 0f;

	ThirdPersonPlayerController controller;

	private void Start() {
		animator = GetComponent<Animator>();
		lastPosition = transform.position;
		lastRotationY = transform.rotation.eulerAngles.y;
		
	}

	private void OnEnable() {
		controller = GetComponentInParent<ThirdPersonPlayerController>();
		controller?.Events.Add(this, ControllerCallbacks);

		groundedComponent?.Events.Add(this, GroundedCallbacks);

		playerWhip?.Events.Add(this, WhipCallbacks);

		unitEssence?.Events.Add(this, EssenceCallbacks);
	}

	private void EssenceCallbacks(int eventCode, object data) {
		switch ((BlackboardEventType)eventCode) {
			case BlackboardEventType.Staggered:
				break;
			case BlackboardEventType.StaggeredEnd:
				break;
			case BlackboardEventType.Damaged:
				break;
			case BlackboardEventType.ResourcesAdded:
				break;
			case BlackboardEventType.ResourcesRemoved:
				break;
			case BlackboardEventType.Death:
				break;
			case BlackboardEventType.Incapacitated:
				animator.SetTrigger("Incapacitated");
				break;
			case BlackboardEventType.PositiveUnitInteraction:
				break;
			case BlackboardEventType.NeutralUnitInteraction:
				break;
			case BlackboardEventType.NegativeUnitInteraction:
				break;
			case BlackboardEventType.AlertToDangerInteraction:
				break;
			case BlackboardEventType.EnemySpotted:
				break;
			case BlackboardEventType.Healed:
				break;
			case BlackboardEventType.GameobjectDestroyed:
				break;
			case BlackboardEventType.Grappling:
				animator.SetTrigger("Grapple");
				break;
			case BlackboardEventType.Capacitated:
				animator.SetTrigger("Capacitated");
				break;
			case BlackboardEventType.StopGrappling:
				animator.SetTrigger("StopGrab");
				break;
		}
	}

	private void GroundedCallbacks(object sender, GroundedEvent eventCode, object data) {
		switch (eventCode) {
			case GroundedEvent.BecomeGrounded:
				animator.SetTrigger("Land");
				animator.ResetTrigger("Jump");
				animator.ResetTrigger("Fall");
				jumpBoosting = false;
				break;
			case GroundedEvent.BecomeUngrounded:
				animator.SetTrigger("Fall");
				animator.ResetTrigger("Jump");
				animator.ResetTrigger("Land");
				jumpBoosting = false;
				break;
		}
	}

	private void OnDisable() {
		controller = GetComponentInParent<ThirdPersonPlayerController>();
		if ( controller!=null) {
			controller.Events.Remove(this);
		}
		playerWhip?.Events.Remove(this);
	}

	private void ControllerCallbacks(System.Object sender, 
			ThirdPersonControllerEvent eventCode, System.Object data) {
		switch (eventCode) {
			case ThirdPersonControllerEvent.Jump:
				animator.SetTrigger("Jump");
				animator.ResetTrigger("Fall");
				animator.ResetTrigger("Land");
				jumpBoosting = true;
				//grounded = false;
				break;
			case ThirdPersonControllerEvent.Fall:
				animator.SetTrigger("Fall");
				animator.ResetTrigger("Jump");
				animator.ResetTrigger("Land");
				jumpBoosting = false;
				//grounded = false;
				break;
			case ThirdPersonControllerEvent.Land:
				animator.SetTrigger("Land");
				animator.ResetTrigger("Jump");
				animator.ResetTrigger("Fall");
				jumpBoosting = false;
				//grounded = true;
				break;
		}
	}

	private void WhipCallbacks(System.Object sender,
			PlayerWhipEvent eventCode, System.Object data) {
		switch (eventCode) {
			case PlayerWhipEvent.StartGrabbing:
				targetUpperBodyWeight = maxUpperBodyWeight;
				animator.SetTrigger("UseAttack");
				break;
			case PlayerWhipEvent.StopGrabbing:
				targetUpperBodyWeight = 0;
				animator.SetTrigger("StopGrab");
				break;
		}
	}

	private void FixedUpdate() {
		var velocity = transform.InverseTransformVector(GetVelocity().DropY());

		float horizontalMovement = velocity.x;
		float verticalMovement = velocity.z;
		var currentTorque = GetTurnSpeed();

		if ( Mathf.Approximately(0, horizontalMovement) && Mathf.Approximately(0, verticalMovement) && Mathf.Approximately(0, currentTorque)) {
			if ( inLocomotion ) {
				animator.SetTrigger("StopLocomotion");
				inLocomotion = false;
			}
		} else {
			if (!inLocomotion) {
				animator.SetTrigger("StartLocomotion");
				inLocomotion = true;
			}
			
			//animator.SetFloat("TurnSpeed", currentTorque * turnSpeedToAnimationScale);

			animator.SetFloat("ForwardSpeed", verticalMovement * forwardSpeedToAnimationScale);
			animator.SetFloat("SidewaysSpeed", horizontalMovement * sidewaysSpeedToAnimationScale + currentTorque * turnSpeedToAnimationScale);
		}

		//var currentTorque = GetTurnSpeed();
		//animator.SetFloat("TurnSpeed", currentTorque * turnSpeedToAnimationScale);

		lastPosition = transform.position;
		lastRotationY = transform.rotation.eulerAngles.y;

		//if (controller != null) {
		//	animator.SetBool("Grounded", controller.IsGrounded);
		//} else
		if (groundedComponent != null) {
			animator.SetBool("Grounded", groundedComponent.IsGrounded);
		}


		float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex);
		float difference = targetUpperBodyWeight - currentWeight;

		animator.SetLayerWeight(upperBodyLayerIndex, currentWeight+ difference* upperBodyWeightChangeScale);
	}
	
	//void OnAnimatorIK() {

	//	{
	//		// perform a raycast to see if there is something blocking the upper region

	//		RaycastHit hitResult;
	//		var targetFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
	//		var targetFootPosition = targetFootTransform.position;
	//		Vector3 footVector = (targetFootPosition - footOffset) - rightFootRayCastOrigin.position;
	//		if (Physics.Raycast(rightFootRayCastOrigin.position, footVector, out hitResult, maxFootReach, groundLayer)) {
	//			float hitDistanceToCastOrigin = (rightFootRayCastOrigin.position - hitResult.point).magnitude;

	//			if (hitDistanceToCastOrigin < footVector.magnitude || footVector.magnitude > 0.6f * maxFootReach) {
	//				lastRightFootWeight = Mathf.Lerp(lastRightFootWeight, 1, 0.5f);
	//				animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, lastRightFootWeight);
	//				//animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
	//				rightFootTargetPosition = Vector3.Lerp(rightFootTargetPosition, hitResult.point + footOffset, 1);
	//				animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTargetPosition);
	//				// 
	//				//animator.SetIKRotation(AvatarIKGoal.RightFoot,
	//				//	Quaternion.LookRotation(-targetFootTransform.right, hitResult.normal));
					
	//			}

	//		}
	//		else {
	//			lastRightFootWeight = Mathf.Lerp(lastRightFootWeight, 0, 0.5f);
	//			animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, lastRightFootWeight);
	//			animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
				
	//			rightFootTargetPosition = targetFootPosition;
	//		}
	//	}
	//	{
	//		// perform a raycast to see if there is something blocking the upper region

	//		RaycastHit hitResult;
	//		var targetFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
	//		var targetFootPosition = targetFootTransform.position;
	//		Vector3 footVector = (targetFootPosition - footOffset) - leftFootRayCastOrigin.position;
	//		if (Physics.Raycast(leftFootRayCastOrigin.position, footVector, out hitResult, maxFootReach, groundLayer)) {
	//			float hitDistanceToCastOrigin = (leftFootRayCastOrigin.position - hitResult.point).magnitude;

	//			if (hitDistanceToCastOrigin < footVector.magnitude ||
	//					footVector.magnitude > 0.6f * maxFootReach ||
	//					jumpBoosting ) {
	//				lastLeftFootWeight = Mathf.Lerp(lastLeftFootWeight, 1, 0.5f);
	//				animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
	//				//animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
	//				leftFootTargetPosition = Vector3.Lerp(leftFootTargetPosition, hitResult.point + footOffset, 1);
	//				animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTargetPosition);
	//				// 
	//				//animator.SetIKRotation(AvatarIKGoal.LeftFoot,
	//				//Quaternion.LookRotation(-targetFootTransform.right, hitResult.normal));
	//			}

	//		}
	//		else {
	//			lastLeftFootWeight = Mathf.Lerp(lastLeftFootWeight, 0, 0.5f);
	//			animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lastLeftFootWeight);
	//			animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);

	//			leftFootTargetPosition = targetFootPosition;
	//		}
	//	}

	//	animator.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.identity);
	//}

	private void PerformFootRaycast() {

	}

	private float GetVelocityMagnitudeSqrd() {
		return GetVelocity().sqrMagnitude;
	}

	private Vector3 GetVelocity() {
		return transform.position - lastPosition;
	}

	private float GetTurnSpeed() {
		return transform.rotation.eulerAngles.y - lastRotationY;
	}


	//override public void Play(AnimationKey key, AnimationData data) {
	//	switch (key) {
	//		case AnimationKey.Lifted:
	//			StartCoroutine(ColorAnimation(materialPack.staggeredAnimation));

	//			break;
	//		case AnimationKey.LiftedEnd:
	//			StartCoroutine(ColorAnimation(materialPack.resetAnimation));

	//			break;
	//		case AnimationKey.Damaged:
	//			StartCoroutine(ColorAnimation(materialPack.damagedAnimation));

	//			break;
	//		case AnimationKey.Staggered:
	//			StartCoroutine(ColorAnimation(materialPack.staggeredAnimation));

	//			break;
	//		case AnimationKey.StaggeredEnd:
	//			StartCoroutine(ColorAnimation(materialPack.resetAnimation));

	//			break;

	//		case AnimationKey.Death:
	//			StartCoroutine(ColorAnimation(materialPack.deathAnimation, AnimationKeys.Event.DeathEnd));
	//			IsRunning = false;
	//			IsInAir = true;
	//			break;
	//	}
	//}
}
