using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour {

	public AnimationMaster master;
	
	virtual public void Play(AnimationKeys.Key key, float playLength, params AnimationKeys.Mod[] mods ) {

	}

	virtual public void StopAllAnimations() {

	}

	public void TriggerEvent(AnimationKeys.Event args) {
		if ( master!= null)
			master.TriggerEvent(this, args);
	}
}
