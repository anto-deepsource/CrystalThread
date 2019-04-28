
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class UnitSoundAnimator : AnimationHandler {

	public AudioClip damaged;

	private AudioSource audioSource;
	
	void Start() {
		audioSource = GetComponent<AudioSource>();
	}

	void Update() {

	}
	
	override public void Play(AnimationKey key, AnimationData data) {
		switch (key) {
			case AnimationKey.Damaged:
				audioSource.PlayOneShot(damaged);
				break;
			case AnimationKey.Death:
				break;
			case AnimationKey.Alerted:
				break;
		}
	}
}
