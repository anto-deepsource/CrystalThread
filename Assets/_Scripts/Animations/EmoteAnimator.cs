using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class EmoteAnimator : AnimationHandler {

	public float emoteTime = 3f;

	public Sprite exclamation;
	public Sprite death;

	private SpriteRenderer spriteRenderer;

	private float showDelay = 0;

	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.enabled = false;
	}
	
	void Update () {
		if ( showDelay > 0 && showDelay - Time.deltaTime <= 0 ) {
			spriteRenderer.enabled = false;
		}

		showDelay -= Time.deltaTime;
	}

	private void ShowEmote(Sprite newSprite ) {
		spriteRenderer.sprite = newSprite;
		spriteRenderer.enabled = true;
		showDelay = emoteTime;
	}

	override public void Play(AnimationKey key, AnimationData data) {
		switch (key) {
			case AnimationKey.Damaged:
				break;
			case AnimationKey.Death:
				ShowEmote(death);
				break;
			case AnimationKey.Alerted:
				ShowEmote(exclamation);
				break;
		}
	}
}
