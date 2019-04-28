using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Faction {
	None = 0,
	Player =2,
	Herbivores = 4,
	BulliesAndJoblins = 8,
	Macabres = 16,
	Sentients = 32, // Humans, humanoids, or otherwise intelligent creatures
}

public static class FactionUtils {

	/// <summary>
	/// Using a system of pow2 numbers/types and a flag that can be any combination of those types,
	/// returns true if the given value/flag is of the given type.
	/// </summary>
	public static bool IsA(Faction value, Faction type) {
		return ((int)value & (int)type) > 0;
	}

	public static bool IsNotA(Faction value, Faction type) {
		return !IsA(value, type);
	}
}