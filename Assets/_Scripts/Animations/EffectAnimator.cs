using System.Collections;
using System.Collections.Generic;
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

	override public void Play(AnimationKeys.Key key, float playLength, params AnimationKeys.Mod[] mods) {
		switch (key) {
			case AnimationKeys.Key.Attack:
				break;
			case AnimationKeys.Key.Staggered:
				break;
			case AnimationKeys.Key.StaggeredEnd:
				break;
			case AnimationKeys.Key.Lifted:
				break;
			case AnimationKeys.Key.LiftedEnd:
				break;
			case AnimationKeys.Key.Death:
				break;
			case AnimationKeys.Key.DeathEnd:
				break;
			case AnimationKeys.Key.Damaged:
				damageSpray.SetActive(true);
				damageSpray.GetComponent<ParticleSystem>().Play();
				break;
		}
	}
}
