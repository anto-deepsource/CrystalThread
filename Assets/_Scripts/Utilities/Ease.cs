using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ease {

	public enum Type {
		IN,
		OUT,
		IN_OUT
	}

	const float PI_HALVES = Mathf.PI *0.5f;

	public static float Value(Type type, float cur_time, float from_value, float to_value, float total_time) {
		float t = cur_time;
		float b = from_value;
		float c = to_value - from_value;
		float d = total_time;
		switch (type) {
			case Type.IN:
			default:
				return -c * Mathf.Cos(t / d * PI_HALVES) + c + b;
			case Type.OUT:
				return c * Mathf.Sin(t / d * PI_HALVES) + b;
			case Type.IN_OUT:
				return -c / 2 * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
		}
	}
}
