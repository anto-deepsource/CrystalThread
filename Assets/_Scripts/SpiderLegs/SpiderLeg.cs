
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLeg : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;

	public Transform targetPosition;

	public float stepHeight = 0.8f;

	public float stepSpeed = 2f;

	public float stepDelay = 0f;

	public float groundOffset = 0.1f;

	public float maxLateralStepDistance = 0.5f;

	public float touchingGroundHeight = 0.1f;

	public float tween = .3f;

	public float maxLegLength = 1f;

	public UnitEssence myEssence;

	private float stepTime = 0f;

	private float lastDistanceFromGround;

	private bool correctingStep = false;

	private bool lockHorizontalPosition = false;

	private Vector2 lastWorldXZ;

	private float forwardDistance = 0f;

	private float lastGroundY;

	private void Start() {
		if (myEssence == null) {
			myEssence = gameObject.GetUnitEssence();
		}
		
		lastWorldXZ = transform.position.JustXZ();
	}

	void Update() {
		
		float distanceFromGround = 0.0f;

		transform.forward = targetPosition.forward;
		var localPosition = targetPosition.InverseTransformPoint(transform.position);

		// if we're standing still but our foot is a certain distance from center -> correct it
		forwardDistance = localPosition.z;

		float newY = transform.position.y;
		Vector2 newXZ = transform.position.JustXZ();

		if (!correctingStep && 
				localPosition.sqrMagnitude > maxLateralStepDistance * maxLateralStepDistance) {
			correctingStep = true;
		}

		if (myEssence.IsIncapacitated || myEssence.IsOffBalance || myEssence.IsDead) {
			newY = targetPosition.position.y;
			newXZ = targetPosition.position.JustXZ();
		}
		else {

			if (myEssence.IsRunning || myEssence.IsTurning || correctingStep) {
				float moveSpeed = Mathf.Max(myEssence.MoveVector.magnitude,
					myEssence.CurrentTurnSpeed / myEssence.turnSpeed);

				stepTime += Time.deltaTime;
				float x = Mathf.Max(0, stepTime - stepDelay);
				float h = stepHeight * moveSpeed;
				// h * ( cos(s x+pi)+1 )
				distanceFromGround = h * (Mathf.Cos(stepSpeed * x + Mathf.PI) + 1f);

				lockHorizontalPosition = distanceFromGround < touchingGroundHeight;

				if (myEssence.IsRunning) {
					forwardDistance = -maxLateralStepDistance * Mathf.Sin(stepSpeed * x);

					if (forwardDistance > 0.6f) {
						correctingStep = false;
					}
				}
				else {
					forwardDistance = 0;
					correctingStep = false;
				}


			}
			else {
				distanceFromGround = lastDistanceFromGround * tween;
				stepTime = 0;

				if (localPosition.sqrMagnitude > maxLateralStepDistance * maxLateralStepDistance * 0.5f) {
					correctingStep = true;
				}

				lockHorizontalPosition = true;
			}
			lastDistanceFromGround = distanceFromGround;

			var tryingToPutFootPoint = targetPosition.position + myEssence.MoveVector * forwardDistance;
			RaycastHit camRayhit;
			bool isGroundWithinReach =
				Physics.Raycast(tryingToPutFootPoint, Vector3.down, out camRayhit, maxLegLength, groundLayer);
			
			if (isGroundWithinReach) {
				var groundY = lastGroundY + (camRayhit.point.y - lastGroundY) * 0.9f;
				lastGroundY = camRayhit.point.y;

				var targetY = groundY + distanceFromGround + groundOffset;
				newY = targetY;
				//

				if (lockHorizontalPosition) {
					// leave it at the current x,z
					newXZ = lastWorldXZ;
				}
				else {
					Vector2 targetXZ = camRayhit.point.JustXZ();
					newXZ += (targetXZ - newXZ) * tween;
				}
			}
			else {
				newY = targetPosition.position.y;
				newXZ = targetPosition.position.JustXZ();
			}
		}
		

		lastWorldXZ = newXZ;
		transform.position = newXZ.FromXZ() + Vector3.up * newY;
	}

	//private float PickNewFootPosition() {

	//}

	/// <summary>
	/// Maps an input x [0,1] value to a curve: starts at origin, ends at (1.0, endHeight),
	/// and arcs upwards in a bowed curve between the two points.
	/// </summary>
	/// <param name="endHeight"></param>
	/// <param name="arcHeight"></param>
	/// <param name="x"></param>
	/// <returns></returns>
	public float UpwardsArc(float endHeight, float arcHeight, float x ) {
		// Linear system solutions give the curve
		// (2c - 4b)x**2 + (4b-c)x
		// Where c is endHeight
		// and b = c + arcHeight

		float b = endHeight + arcHeight;
		float fourB = 4f * b;
		return (2*endHeight - fourB) * x * x + (fourB - endHeight) * x;
	}
}
