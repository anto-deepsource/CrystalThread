using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationKeys {

	public delegate void EventHandler(object sender, AnimationKeys.Event args);
	
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

namespace UnitAnimation {

	public enum AnimationKey {
		Attack,
		Staggered,
		StaggeredEnd,
		Lifted,
		LiftedEnd,
		Death,
		DeathEnd,
		Damaged,
		Alerted,
		Speaks,
	}

	/// <summary>
	/// Additional information about a triggered animation.
	/// </summary>
	public struct AnimationData {
		public AnimationKey key;

		public object data;
		
		public int Length {
			get {
				if (data is int) {
					return (int)data;
				}
				return 0; // TODO: throw an exception or something
			}
		}

		public string Message {
			get {
				if (data is string) {
					return (string)data;
				}
				return ""; // TODO: throw an exception or something
			}
		}

		public static AnimationData none;

		public static AnimationData NewMessage( string message ) {
			return new AnimationData() { key = AnimationKey.Speaks, data = message };
		}
	}
}