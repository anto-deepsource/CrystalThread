using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class EffectAnimator : AnimationHandler {

	private GameObject damageSpray;

	// Use this for initialization
	void Start () {
		damageSpray = transform.Find("DamageSpray").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	override public void Play(AnimationKey key, AnimationData data) {
		switch (key) {
			case AnimationKey.Attack:
				break;
			case AnimationKey.Staggered:
				break;
			case AnimationKey.StaggeredEnd:
				break;
			case AnimationKey.Lifted:
				break;
			case AnimationKey.LiftedEnd:
				break;
			case AnimationKey.Death:
				break;
			case AnimationKey.DeathEnd:
				break;
			case AnimationKey.Damaged:
				damageSpray.SetActive(true);
				damageSpray.GetComponent<ParticleSystem>().Play();
				break;
		}
	}
}
