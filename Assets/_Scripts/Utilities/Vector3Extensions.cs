using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions {

	public static bool ApproxEquals( this Vector3 self, Vector3 other, float threshold ) {
		float diff = (self - other).sqrMagnitude;
		return diff < threshold * threshold;
	}
}
