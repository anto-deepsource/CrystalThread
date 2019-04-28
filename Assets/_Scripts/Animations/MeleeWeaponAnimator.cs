using Porter;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

/// <summary>
/// Describes an abstract melee weapon and allows a unit to call generic
/// animations on it without the unit knowing much about the weapon.
/// Further integrates with the Unity animator/animations to sequence
/// melee swings by defining two points and a rotation:
/// -The handle
/// -The tip of the weapon (or at least the direction)
/// The MeleeWeaponAnimator handles the mesh weapon object by putting
/// it at the handle and appropriatally rotating it, even while in the editor.
/// </summary>
[ExecuteInEditMode]
public class MeleeWeaponAnimator : AnimationHandler {

	public GameObject meshObject;

	public Vector3 handlePoint;

	public Vector3 tipPoint;

	public float rotation;

    public List<AnimationClip> animations;

    WeaponTrail weaponTrail;
	AnimationController animationController;
	Animation animationComp;

	DamageOnCollision damageComponent;

	bool swinging = false;

	bool dead = false;

	private void Start() {
		animationComp = GetComponent<Animation>();
        if(animations!=null) {
            foreach( var anim in animations) {
                animationComp.AddClip(anim, anim.name);
            }
        }

		animationController = GetComponent<AnimationController>();

		if ( Application.isPlaying ) {
			weaponTrail = GetComponentInChildren<WeaponTrail>();
			animationController.AddTrail(weaponTrail);

			damageComponent = meshObject.GetComponent<DamageOnCollision>();
		}
		
	}

    private AnimationState GetRandomAnimation() {
        int r = Mathf.FloorToInt(Random.value * animations.Count);
        return animationComp[animations[r].name];
    }

	override public void Play(AnimationKey key, AnimationData data) {
		if (key == AnimationKey.Attack) {
            AnimationState animState = GetRandomAnimation();
			float length = data.Length;
            if (length > 0)
				animState.speed = animState.length / length;
			if (!animationController.enabled) {
				animationController.enabled = true;
			}
			animationController.PlayAnimation(animState);
			weaponTrail.SetTime(length, 0, 1);
			//damageComponent.on = true;
			swinging = true;
		} else
		if (key == AnimationKey.Death) {
			animationController.enabled = false;
			//damageComponent.on = false;
			swinging = false;

			meshObject.AddComponent<Rigidbody>();
			var colliders = meshObject.GetComponents<Collider>();
			foreach( var collider in colliders ) {
				collider.enabled = true;
			}

			meshObject.transform.SetParent(null);

			dead = true;
		}
	}

	void Update () {
		if ( meshObject==null || dead ) {
			return;
		}

		meshObject.transform.localPosition = handlePoint;
		meshObject.transform.LookAt(transform.TransformPoint(tipPoint));
		var vector = (transform.TransformPoint(tipPoint) - transform.TransformPoint(handlePoint)).normalized;
		meshObject.transform.RotateAround( transform.position, vector, rotation);

		if (swinging && !animationComp.isPlaying) {
			AnimationEnded();
		}
	}

	private void AnimationEnded() {
		TriggerEvent(AnimationKeys.Event.End);
		swinging = false;
		//damageComponent.on = false;
	}
}
