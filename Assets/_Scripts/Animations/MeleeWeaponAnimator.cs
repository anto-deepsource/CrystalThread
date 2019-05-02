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

	//DamageOnCollision damageComponent;

	bool swinging = false;

	public bool manualFire = false;

	public ExtraEvent<MeleeWeaponEvent> Events {
		get {
			if (_events == null) {
				_events = new ExtraEvent<MeleeWeaponEvent>(this);
			}
			return _events;
		}
	}
	ExtraEvent<MeleeWeaponEvent> _events;

	//bool dead = false;

	private void Start() {
		animationComp = GetComponent<Animation>();
		if (animations != null) {
			foreach (var anim in animations) {
				animationComp.AddClip(anim, anim.name);
			}
		}

		animationController = GetComponent<AnimationController>();

		//if (Application.isPlaying) {
		weaponTrail = GetComponentInChildren<WeaponTrail>();
		animationController.AddTrail(weaponTrail);

		//	damageComponent = meshObject.GetComponent<DamageOnCollision>();
		//}

	}

	private void InitializeMaybe() {
		if (animationController == null) {
			animationController = GetComponent<AnimationController>();
		}
		if (animationComp == null) {
			animationComp = GetComponent<Animation>();
		}
		if (weaponTrail == null) {
			weaponTrail = GetComponentInChildren<WeaponTrail>();
			animationController.AddTrail(weaponTrail);
		}
	}

	//private AnimationState GetRandomAnimation() {
	//	int r = Mathf.FloorToInt(Random.value * animations.Count);
	//	return animationComp[animations[r].name];
	//}

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
			//weaponTrail.SetTime(1, 0, 1);
			//damageComponent.on = true;
			swinging = true;
		//} else
		//if (key == AnimationKey.Death) {
		//	animationController.enabled = false;
		//	//damageComponent.on = false;
		//	swinging = false;

		//	meshObject.AddComponent<Rigidbody>();
		//	var colliders = meshObject.GetComponents<Collider>();
		//	foreach( var collider in colliders ) {
		//		collider.enabled = true;
		//	}

		//	meshObject.transform.SetParent(null);

		//	dead = true;
		}
	}

	public void Play(AnimationClip clip) {
		InitializeMaybe();
		if (!animationController.enabled) {
			animationController.enabled = true;
		}
		var animationState = animationComp[clip.name];
		//weaponTrail.SetTime(1, 0, 1);
		animationController.PlayAnimation(animationState);
	}

	void Update () {
		if ( meshObject==null ) {
			return;
		}

		if (manualFire) {
			manualFire = false;
			Play(AnimationKey.Attack, AnimationData.none);
		}

		meshObject.transform.localPosition = handlePoint;
		meshObject.transform.LookAt(transform.TransformPoint(tipPoint));
		var vector = (transform.TransformPoint(tipPoint) - transform.TransformPoint(handlePoint)).normalized;
		meshObject.transform.RotateAround(meshObject.transform.position, vector, rotation);

		if (swinging && !animationComp.isPlaying) {
			AnimationEnded();
		}
	}

	private void AnimationEnded() {
		TriggerEvent(AnimationKeys.Event.End);
		swinging = false;
		//damageComponent.on = false;
		Events.Fire(MeleeWeaponEvent.EndSwing);
	}

	public void StartWeaponTrail() {
		weaponTrail.SetTime(1, 0.1f, 1);
		//Events.Fire(MeleeWeaponEvent.StartSwing);
		
	}

	public void StopWeaponTrail() {
		weaponTrail.SetTime(0, 0.1f, 1);
		//Events.Fire(MeleeWeaponEvent.EndSwing);
	}
}
public enum MeleeWeaponEvent {
	StartSwing,
	EndSwing,
}