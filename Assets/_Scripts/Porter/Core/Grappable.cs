using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappable : MonoBehaviour {

	public Rigidbody unitBody;
	public Collider unitCollider;

	public Rigidbody leftArm;
	public Rigidbody rightArm;
	public Rigidbody chest;
	public Rigidbody hips;

	public Transform root;
	public Rigidbody pelvis;

	public RagdollUtility ragdollUtility;
	public Grounder grounder;
	public FullBodyBipedIK ik; // Reference to the FBBIK component

	public UnitEssence myEssence;

	public void PrepareForGrapple() {
		//unitBody.isKinematic = true;
		unitCollider.enabled = false;
	}

	public void Grapple() {
		unitBody.isKinematic = true;
		unitCollider.enabled = false;

		if (myEssence == null) {
			myEssence = gameObject.GetUnitEssence();
		}
		myEssence.Incapacitate();

		ragdollUtility.EnableRagdoll();

		grounder.weight = 0;
		grounder.enabled = false;
	}

	public void StopGrapple() {
		Vector3 toPelvis = pelvis.position - root.position;
		root.position += toPelvis;
		pelvis.transform.position -= toPelvis;

		ragdollUtility.DisableRagdoll();

		grounder.weight = 1;
		grounder.enabled = true;

		unitBody.isKinematic = false;
		unitCollider.enabled = true;

		myEssence.Capacitate();
	}
}
