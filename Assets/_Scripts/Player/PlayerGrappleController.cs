using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappleController : MonoBehaviour {

	public float range = 6f;

	public float positionTolerance = 0.2f;

	public UnitEssence myEssence;

	//public Rigidbody myBody;

	public Transform grappleTarget;

	public Transform rightHand;
	public Transform leftHand;
	public Transform chestPoint;
	public Transform hipsPoint;

	public Transform leftHandHoldPosition;
	public Transform rightHandHoldPosition;

	public FullBodyBipedIK ik; // Reference to the FBBIK component

	private bool grappling = false;
	private bool movingIntoGrapple = false;
	private bool actualGrappling = false;

	private Grappable targetGrappable = null;

	private SpringForce chestSpring;
	private SpringForce hipsSpring;
	private SpringForce leftSpring;

	private void Update() {
		if (!grappling) {
			if (Input.GetButtonDown("Interact")) {
				Grappable[] allGrappables = GameObject.FindObjectsOfType<Grappable>();

				float nearestDistance = 0;
				Grappable nearestGrappable = null;

				foreach (var grappable in allGrappables) {
					float distance = (transform.position - grappable.transform.position).sqrMagnitude;
					if (nearestGrappable == null || distance < nearestDistance) {
						nearestDistance = distance;
						nearestGrappable = grappable;
					}
				}

				if (nearestGrappable != null && nearestDistance < range) {
					StartPerformGrapple(nearestGrappable);
					Debug.Log("Start Grapple");
				}
			}
		} else {
			// IS grappling
			if ( movingIntoGrapple ) {
				MoveIntoGrappleUpdate();
			} else if (!actualGrappling) {
				ActualPerformGrapple();
			}
			if (Input.GetButtonDown("Interact")) {
				StopGrapple();
			}
		}
		
	}

	private void StartPerformGrapple(Grappable grappable) {
		grappling = true;
		movingIntoGrapple = true ;
		targetGrappable = grappable;
		targetGrappable.PrepareForGrapple();
		myEssence.currentControllers.Push(this);
		targetGrappable.myEssence.currentControllers.Push(this);
	}

	/// <summary>
	/// Take over control of the character and move them into position
	/// </summary>
	private void MoveIntoGrappleUpdate() {
		var vector = targetGrappable.transform.position.JustXZ() - grappleTarget.position.JustXZ();
		if ( vector.sqrMagnitude < positionTolerance * positionTolerance) {
			movingIntoGrapple = false;
		}
		myEssence.MoveVector = vector.FromXZ();

		vector = targetGrappable.transform.position.JustXZ() - transform.position.JustXZ();
		myEssence.TurnVector = vector.normalized;
		
		targetGrappable.myEssence.MoveVector = Vector3.zero;
		targetGrappable.myEssence.TurnVector = -vector;


	}

	FixedJoint chestJoint;

	private void ActualPerformGrapple() {
		targetGrappable.Grapple();
		myEssence.StartGrappling(targetGrappable);
		myEssence.MoveVector = Vector3.zero;
		myEssence.TurnVector = Vector2.zero;

		var relativePosition = targetGrappable.chest.transform.InverseTransformPoint(chestPoint.transform.position);
		bool grapForward = relativePosition.z > 0;

		//leftSpring = leftHand.gameObject.AddComponent<SpringForce>();
		//if (grapForward) {
		//	leftSpring.connectedBody = targetGrappable.rightArm;
		//} else {
		//	leftSpring.connectedBody = targetGrappable.leftArm;
		//}
		//leftSpring.anchorPoint = new Vector3(-.5f, 0, 0);

		//leftSpring.maxForce = 10000;
		//leftSpring.force = 1000;
		//leftSpring.dampening = 10;

		chestJoint = chestPoint.gameObject.AddComponent<FixedJoint>();
		chestJoint.connectedBody = targetGrappable.chest;
		chestJoint.connectedAnchor = Vector3.zero;

		//chestSpring = chestPoint.gameObject.AddComponent<SpringForce>();
		//chestSpring.connectedBody = targetGrappable.chest;
		//chestSpring.maxForce = 10000;
		//chestSpring.force = 2000;
		//chestSpring.dampening = 10;
		////var relativePosition = targetGrappable.chest.transform.InverseTransformPoint(chestPoint.transform.position);
		//if (grapForward) {
		//	chestSpring.anchorPoint = new Vector3(0, 0, .3f);
		//} else {
		//	chestSpring.anchorPoint = new Vector3(0, 0, -.3f);
		//}

		//hipsSpring = hipsPoint.gameObject.AddComponent<SpringForce>();
		//hipsSpring.connectedBody = targetGrappable.hips;
		//hipsSpring.maxForce = 10000;
		//hipsSpring.force = 2000;
		//hipsSpring.dampening = 10;
		////relativePosition = targetGrappable.hips.transform.InverseTransformPoint(hipsPoint.transform.position);
		//if (grapForward) {
		//	hipsSpring.anchorPoint = new Vector3(0, 0, .4f);
		//}
		//else {
		//	hipsSpring.anchorPoint = new Vector3(0, 0, -.4f);
		//}

		actualGrappling = true;

		//targetGrappable.ik.enabled = false;
		//targetGrappable.ik.solver.leftHandEffector.target = leftHand;
		//targetGrappable.ik.solver.leftHandEffector.positionWeight = 1;
		//targetGrappable.ik.solver.rightHandEffector.target = rightHand;
		//targetGrappable.ik.solver.rightHandEffector.positionWeight = 1;

		//ik.solver.leftHandEffector.target = leftHandHoldPosition;
		//ik.solver.leftHandEffector.positionWeight = 1;
	}

	private void StopGrapple() {
		grappling = false;
		myEssence.StopGrappling(targetGrappable);

		targetGrappable.StopGrapple();
		targetGrappable.myEssence.currentControllers.Pop();
		targetGrappable = null;

		//Destroy(chestSpring);
		//chestSpring = null;

		//Destroy(hipsSpring);
		//hipsSpring = null;

		Destroy(chestJoint);

		//Destroy(leftSpring);
		//leftSpring = null;

		//ik.solver.leftHandEffector.positionWeight = 0;

		myEssence.currentControllers.Pop();
		actualGrappling = false;
	}
}
