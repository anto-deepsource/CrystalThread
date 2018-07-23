using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationKeys {

	public delegate void EventHandler(object sender, AnimationKeys.Event args);

	public enum Key {
		Attack,
		Staggered,
		StaggeredEnd,
		Lifted,
		LiftedEnd,
		Death,
		DeathEnd,
		Damaged
	}

	public enum Mod {
		Var1,
		Var2,
		Var3,

	}

	public enum Event {
		None,
		Start,
		End,
		DeathEnd,
		Action,
		Damage,
	}
}
