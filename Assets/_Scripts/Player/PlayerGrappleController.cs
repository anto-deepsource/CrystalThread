using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappleController : AbstractInteractableActuator {

	public float range = 6f;

	//public float positionTolerance = 0.2f;

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
	public Grounder grounder;


	public float springStrength = 100000f;
	public float springPullInSpeed = 1;

	//public Transform missionaryTargetPosition;
	//public Transform missionaryInteractionPosition;
	//public Transform missionaryInteractionInPosition;
	//public Transform missionaryVictimPosition;
	//public float missionarySpeed = 1f;

	private bool grappling = false;
	//private bool movingIntoGrapple = false;
	private bool actualGrappling = false;
	//private bool inMissionary = false;

	private Grappable targetGrappable = null;

	//private SpringForce chestSpring;
	//private SpringForce hipsSpring;
	//private SpringForce leftSpring;

	//private void Update() {
	//	if (grappling && Input.GetButtonDown("Secondary Interact")) {
	//		MoveIntoMissionary();
	//	}
	//	if (inMissionary) {
	//		ik.solver.bodyEffector.position = MissionaryTopPelvisPosition();
	//	}
	//}

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
				return new ActuatorTarget(nearestGrappable, this, nearestDistance, "Grapple");
			}
		}
		return ActuatorTarget.None();
	}

	public override bool IsImmediateInteraction() {
		if (grappling) {
			return true;
		}
		return false;
	}

	public override string GetImmediateInteractionLabel() {
		if (grappling) {
			return "Release";
		}
		return "";
	}

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
	
	List<Component> temporaryJoints = new List<Component>();
	
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

	IEnumerator TweenEffectorPositionWeight(IKEffector effector, float targetWeight, float tweenSpeed) {
		float startWeight = effector.positionWeight;
		float t = 0;
		float difference = targetWeight - startWeight;
		float tweenTime = Mathf.Abs( difference ) * tweenSpeed;
		while ( t < tweenTime) {
			t += Time.deltaTime;
			effector.positionWeight = Mathf.Lerp(startWeight, targetWeight, t / tweenTime);
			yield return null;
		}
	}

	//private void MoveIntoMissionary() {
	//	targetGrappable.MoveIntoMissionary();
	//	targetGrappable.transform.position = missionaryTargetPosition.position;

	//	targetGrappable.ik.solver.bodyEffector.target = missionaryVictimPosition;
	//	StartCoroutine(TweenEffectorPositionWeight(targetGrappable.ik.solver.bodyEffector, 1, .4f));

	//	targetGrappable.ik.solver.rightFootEffector.target = leftHandHoldPosition;
	//	StartCoroutine(TweenEffectorPositionWeight(targetGrappable.ik.solver.rightFootEffector, 1, .4f));
	//	//targetGrappable.ik.solver.bodyEffector.positionWeight = 1;

	//	ik.enabled = true;

		
	//	StartCoroutine(TweenEffectorPositionWeight(ik.solver.bodyEffector, 1, .4f));
	//	//ik.solver.bodyEffector.positionWeight = 1;

	//	grounder.enabled = false;
	//	myEssence.Inform(BlackboardEventType.ManualFireAnimationTrigger, "MissionaryTop");

	//	inMissionary = true;
	//}

	//private Vector3 MissionaryTopPelvisPosition() {
	//	var outPosition = missionaryInteractionPosition.position;
	//	var inPosition = missionaryInteractionInPosition.position;
	//	float t = Mathf.Sin(Time.time * missionarySpeed) * 0.5f + 0.5f;
	//	return Vector3.Lerp(outPosition, inPosition, t);
	//}
}
