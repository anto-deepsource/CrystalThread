using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrajectoryUtils {

	public static Vector3 GetInitialVelocityToHitMovingTargetWithLateralSpeed(
			Vector3 startPosition, Vector3 target, Vector3 targetVelocity, float lateralSpeed ) {
		// rename some of the important values for readability
		float F = lateralSpeed;
		float Px = target.x - startPosition.x;
		float Py = target.y - startPosition.y;
		float Pz = target.z - startPosition.z;
		float Pvx = targetVelocity.x;
		float Pvy = targetVelocity.y;
		float Pvz = targetVelocity.z;

		// the main one to find is the vertical component
		// which is 
		// Vy = (Py + Pvy * t - .5 * g * t^2 )/t
		// (notice that its the only one that deals with gravity)
		// the other two are the x and z components:
		// Vx = (Px + Pvx * t)/t and
		// Vz = (Pz + Pvz * t)/t
		// where t = time, and
		// which we found by taking the equation for motion,
		// plugging in what we know about the final displacement,
		// (namely that it is the target position translated by
		// the target's velocity scaled by the time)
		// and solving for the initial velocity in terms of t.
		// the key is everything depends on time, specifically the time it takes
		// to move laterally to the target.
		// we can solve t by using what we know about the lateral force:
		// F^2 = Vx^2 + Vz^2
		// because whatever our initial lateral velocity is, it's magnitude
		// must be the given lateral force (just a choice we made).
		// Pluggin Vx and Vz into our equation for F^2 and solving for t
		// gives an ugly equation:

		float rootable = F * F  * (Px*Px + Pz*Pz) - Mathf.Pow(Pvz*Px - Pvx*Pz, 2 );
		float numerator = Mathf.Sqrt(rootable) + Pvx * Px + Pvz * Pz;
		float denominator = F * F - Pvx * Pvx - Pvz * Pvz;
		float t = numerator / denominator;

		// Once we know t its just pluggin it into our equations for Vx, Vy, and Vz
		float Vy = (Py + Pvy*t - 0.5f * Physics.gravity.y * t * t) / t;

		float Vx = (Px + Pvx * t) / t;
		float Vz = (Pz + Pvz * t) / t;

		return new Vector3(Vx, Vy, Vz);
	}

	public static Vector3 ProjectedPoint( Vector3 initialVelocity, float time ) {
		return initialVelocity * time + 0.5f * Physics.gravity * time * time;
	}

	public static Vector3 GetInitialVelocityToHitStationaryTargetWithLateralSpeed(
			Vector3 startPosition, Vector3 target, float lateralSpeed) {
		// find the time it takes to move lateraly to the target
		float Px = target.x - startPosition.x;
		float Py = target.y - startPosition.y;
		float Pz = target.z - startPosition.z;
		float F = lateralSpeed;
		float t = Mathf.Sqrt(Px * Px + Pz * Pz) / F;

		// use t to find the initial vertical velocity
		float g = Physics.gravity.y;
		float vy = Py / t - 0.5f * g * t;

		return new Vector3(Px / t, vy, Pz / t);

	}
}
