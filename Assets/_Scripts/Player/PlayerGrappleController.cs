using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappleController : AbstractInteractableActuator {

	public float range = 6f;

	public float positionTolerance = 0.2f;

	public UnitEssence myEssence;

	//public Rigidbody myBody;

	//public Transform grappleTarget;

	//public Transform idealInteractionPosition;
	//public float idealInteractionPositionTolerance = 0.2f;
	//public float idealInteractionVelocityTolerance = 0.5f;

	public Transform rightHand;
	public Transform leftHand;
	public Transform chestPoint;
	public Transform hipsPoint;

	public Transform leftHandHoldPosition;
	public Transform rightHandHoldPosition;

	public FullBodyBipedIK ik; // Reference to the FBBIK component

	public float springStrength = 100000f;
	public float springPullInSpeed = 1;

	private bool grappling = false;
	//private bool movingIntoGrapple = false;
	private bool actualGrappling = false;

	private Grappable targetGrappable = null;

	private SpringForce chestSpring;
	private SpringForce hipsSpring;
	private SpringForce leftSpring;

	public override ActuatorTarget GetBestTarget() {
		if (!grappling) {
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
				return new ActuatorTarget(nearestGrappable, this, nearestDistance);
			}
		}
		return ActuatorTarget.None();
	}
	
	//public override bool IsBlocking() {
	//	// if we're moving into grapple -> override other actuators
	//	if (movingIntoGrapple) {
	//		return true;
	//	}
	//	return false;
	//}

	public override bool UseInteractionEventImmediateMaybe() {
		if (grappling) {
			StopGrapple();
			return true;
		}
		return false;
	}

	override public void UseInteractionBestTargetEvent(MonoBehaviour target) {
		//StartPerformGrapple(target as Grappable);
		ActualPerformGrapple();
	}

	override public void StartMoveIntoIdealPositionBestTargetEvent(MonoBehaviour target) {
		grappling = true;
		myEssence.OverrideControl(this);
		targetGrappable = target as Grappable;
		targetGrappable.PrepareForGrapple();
		targetGrappable.myEssence.OverrideControl(this);
	}

	//private void Update() {
	//	if (!grappling) {
			
	//	} else {
	//		// IS grappling
	//		if ( movingIntoGrapple ) {
	//			MoveIntoIdealPosition();
	//		//} else if (!actualGrappling) {
	//		//	ActualPerformGrapple();
	//		//} else
	//		//if (Input.GetButtonDown("Interact")) {
	//		//	StopGrapple();
	//		}
	//	}
		
	//}

	//private void StartPerformGrapple(Grappable grappable) {
	//	grappling = true;
	//	movingIntoGrapple = true ;
	//	targetGrappable = grappable;
	//	targetGrappable.PrepareForGrapple();
	//	myEssence.currentControllers.Push(this);
	//	targetGrappable.myEssence.currentControllers.Push(this);
	//}

	///// <summary>
	///// Take over control of the character and move them into position
	///// </summary>
	//private void MoveIntoGrappleUpdate() {
		
	//	var vector = targetGrappable.transform.position.JustXZ() - grappleTarget.position.JustXZ();
	//	if ( vector.sqrMagnitude < positionTolerance * positionTolerance) {
	//		movingIntoGrapple = false;
	//	}
	//	myEssence.MoveVector = vector.FromXZ();

	//	vector = targetGrappable.transform.position.JustXZ() - transform.position.JustXZ();
	//	myEssence.TurnVector = vector.normalized;
		
	//	targetGrappable.myEssence.MoveVector = Vector3.zero;
	//	targetGrappable.myEssence.TurnVector = -vector;


	//}

	///// <summary>
	///// Take over control of the character and move them into position
	///// </summary>
	//private void MoveIntoIdealPosition() {
	//	//bool moveIntoNextPhase = false;

	//	var currentVelocity = myEssence.GetCurrentVelocity();
	//	bool withinIdealVelocity = currentVelocity.sqrMagnitude < idealInteractionVelocityTolerance * idealInteractionVelocityTolerance;

	//	var vector = targetGrappable.transform.position.JustXZ() - idealInteractionPosition.position.JustXZ();

	//	// if the distance from the target point to the pickup is less than a certain distance -> move into next phase
	//	float tolerance = idealInteractionPositionTolerance * idealInteractionPositionTolerance;
	//	bool withinIdealPosition = vector.sqrMagnitude < tolerance;
	//	if (withinIdealPosition) {
	//		myEssence.MoveVector = Vector3.zero;
	//	}
	//	else {
	//		myEssence.MoveVector = vector.FromXZ();
	//	}


	//	vector = targetGrappable.transform.position.JustXZ() - transform.position.JustXZ();
	//	myEssence.TurnVector = vector.normalized;

	//	//// if the distance from us to the pickup is less than the distance to the ideal position -> move into next phase
	//	//var idealPositionRange = (idealInteractionPosition.position.JustXZ() - transform.position.JustXZ()).sqrMagnitude;
	//	//if (vector.sqrMagnitude < idealPositionRange) {
	//	//	moveIntoNextPhase = true;
	//	//}

	//	if (withinIdealVelocity && withinIdealPosition) {
	//		ActualPerformGrapple();
	//	}
	//}

	List<Component> temporaryJoints = new List<Component>();

	//FixedJoint chestJoint;

	public float grappleJointWidth = 0.5f;

	private void ActualPerformGrapple() {
		//movingIntoGrapple = false;

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

		//var chestJoint = chestPoint.gameObject.AddComponent<FixedJoint>();
		//chestJoint.connectedBody = targetGrappable.chest;
		//chestJoint.connectedAnchor = Vector3.zero;
		//temporaryJoints.Add(chestJoint);



		{
			var chestJoint = chestPoint.gameObject.AddComponent<SpringJoint>();
			//chestJoint.autoConfigureConnectedAnchor = false;
			chestJoint.connectedBody = targetGrappable.chest;
			chestJoint.anchor = Vector3.right * grappleJointWidth;
			//chestJoint.connectedAnchor = Vector3.right * grappleJointWidth;

			chestJoint.spring = springStrength;
			chestJoint.maxDistance = 0.1f;
			chestJoint.enableCollision = true;
			chestJoint.enablePreprocessing = false;
			temporaryJoints.Add(chestJoint);
			var anchorVector = Vector3.right * grappleJointWidth;
			StartCoroutine(TweenConnectedAnchor(chestJoint, anchorVector, springPullInSpeed));
		}

		{
			var chestJoint = chestPoint.gameObject.AddComponent<SpringJoint>();
			//chestJoint.autoConfigureConnectedAnchor = false;
			chestJoint.connectedBody = targetGrappable.chest;
			chestJoint.anchor = Vector3.left * grappleJointWidth;
			//chestJoint.connectedAnchor = Vector3.left * grappleJointWidth;
			chestJoint.spring = springStrength;
			chestJoint.maxDistance = 0.1f;
			chestJoint.enableCollision = true;
			chestJoint.enablePreprocessing = false;
			temporaryJoints.Add(chestJoint);
			StartCoroutine(TweenConnectedAnchor(chestJoint, Vector3.left * grappleJointWidth, springPullInSpeed));
		}

		{
			var chestJoint = chestPoint.gameObject.AddComponent<SpringJoint>();
			//chestJoint.autoConfigureConnectedAnchor = false;
			chestJoint.connectedBody = targetGrappable.chest;
			if (grapForward) {
				//chestJoint.connectedAnchor = Vector3.down * grappleJointWidth;
				StartCoroutine(TweenConnectedAnchor(chestJoint, Vector3.down * grappleJointWidth, springPullInSpeed));
			}
			else {
				//chestJoint.connectedAnchor = Vector3.up * grappleJointWidth;
				StartCoroutine(TweenConnectedAnchor(chestJoint, Vector3.up * grappleJointWidth, springPullInSpeed));
			}
			chestJoint.anchor = Vector3.up * grappleJointWidth;
			chestJoint.spring = springStrength;
			chestJoint.maxDistance = 0.1f;
			chestJoint.enablePreprocessing = false;
			chestJoint.enableCollision = true;

			temporaryJoints.Add(chestJoint);
		}

		{
			var chestJoint = chestPoint.gameObject.AddComponent<SpringJoint>();
			//chestJoint.autoConfigureConnectedAnchor = false;
			chestJoint.connectedBody = targetGrappable.chest;
			if (grapForward) {
				//chestJoint.connectedAnchor = Vector3.up * grappleJointWidth;
				StartCoroutine(TweenConnectedAnchor(chestJoint, Vector3.up * grappleJointWidth, springPullInSpeed));
			}
			else {
				//chestJoint.connectedAnchor = Vector3.down * grappleJointWidth;
				StartCoroutine(TweenConnectedAnchor(chestJoint, Vector3.down * grappleJointWidth, springPullInSpeed));
			}
			chestJoint.anchor = Vector3.down * grappleJointWidth;
			chestJoint.spring = springStrength;
			chestJoint.maxDistance = 0.1f;
			chestJoint.enablePreprocessing = false;
			chestJoint.enableCollision = true;
			temporaryJoints.Add(chestJoint);
		}

		//chestSpring = chestPoint.gameObject.AddComponent<SpringForce>();
		//chestSpring.connectedBody = targetGrappable.chest;
		//chestSpring.maxForce = 10000;
		//chestSpring.force = 2000;
		//chestSpring.dampening = 10;
		////var relativePosition = targetGrappable.chest.transform.InverseTransformPoint(chestPoint.transform.position);
		//if (grapForward) {
		//	chestSpring.anchorPoint = new Vector3(0, 0, .3f);
		//}
		//else {
		//	chestSpring.anchorPoint = new Vector3(0, 0, -.3f);
		//}
		//temporaryJoints.Add(chestSpring);

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

		targetGrappable.ik.enabled = true;
		if (grapForward) {
			targetGrappable.ik.solver.leftHandEffector.target = rightHandHoldPosition;
		}
		else {
			targetGrappable.ik.solver.leftHandEffector.target = leftHandHoldPosition;
		}
		targetGrappable.ik.solver.leftHandEffector.positionWeight = 1;
		//targetGrappable.ik.solver.rightHandEffector.target = rightHand;
		//targetGrappable.ik.solver.rightHandEffector.positionWeight = 1;

		//ik.solver.leftHandEffector.target = leftHandHoldPosition;
		//ik.solver.leftHandEffector.positionWeight = 1;
	}

	private void StopGrapple() {
		grappling = false;
		myEssence.StopGrappling(targetGrappable);

		targetGrappable.ik.solver.leftHandEffector.target = null;
		targetGrappable.ik.solver.leftHandEffector.positionWeight = 0;

		targetGrappable.StopGrapple();
		targetGrappable.myEssence.currentControllers.Pop();
		targetGrappable = null;

		//Destroy(chestSpring);
		//chestSpring = null;

		//Destroy(hipsSpring);
		//hipsSpring = null;

		foreach( var joint in temporaryJoints) {
			Destroy(joint);
		}
		temporaryJoints.Clear();

		//Destroy(chestJoint);

		//Destroy(leftSpring);
		//leftSpring = null;

		
		//ik.solver.leftHandEffector.positionWeight = 0;

		myEssence.currentControllers.Pop();
		actualGrappling = false;
	}

	IEnumerator TweenConnectedAnchor( SpringJoint joint, Vector3 targetAnchor, float tweenSpeed ) {
		Vector3 startAnchor = joint.connectedAnchor;
		float t = 0;
		Vector3 vector = targetAnchor - startAnchor;
		float tweenTime = vector.magnitude * tweenSpeed;
		while ( joint!=null && t < tweenTime ) {
			t += Time.deltaTime;
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = Vector3.Lerp(startAnchor, targetAnchor, t / tweenTime);
			yield return null;
		}
	}
}
