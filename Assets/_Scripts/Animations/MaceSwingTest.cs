using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class MaceSwingTest : AnimationHandler {

	public WeaponTrail leftSwipe;

	AnimationController animationController;

	bool swinging = false;

	// Use this for initialization
	void Awake() {
		animationController = GetComponent<AnimationController>();

	}
	void Start() {
		animationController.AddTrail(leftSwipe);

	}

	// Update is called once per frame
	void Update () {
		//float t = Mathf.Clamp(Time.deltaTime * 1.0f, 0, 0.066f);
		//animationController.SetDeltaTime(t);

		if (swinging && !GetComponent<Animation>().isPlaying) {
			AnimationEnded();
		}
	}

	override public void Play(AnimationKey key, AnimationData data) {
		if ( key==AnimationKey.Attack ) {
			//Debug.Log("Mace Swing Attack");
			AnimationState animState = GetComponent<Animation>()["Mace_Swing"];
			float length = data.Length;
			if (length > 0 )
				animState.speed =  animState.length/ length;
			if ( !animationController.enabled ) {
				animationController.enabled = true;
			}
			animationController.PlayAnimation(animState);
			leftSwipe.SetTime(length, 0, 1);
			swinging = true;
		}
	}

	override public void StopAllAnimations() {
		//animationController.enabled = false;
		//AnimationState animState = GetComponent<Animation>()["Mace_Swing"];
		//animState.enabled = false;
	}

	public void MaceSwingTestFunction() {
		TriggerEvent( AnimationKeys.Event.Damage );
	}

	public void AnimationEnded() {
		TriggerEvent(AnimationKeys.Event.End);
		swinging = false;
	}
}
