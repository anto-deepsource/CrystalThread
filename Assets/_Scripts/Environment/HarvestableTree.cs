using Grabbables;
using HexMap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grabbable))]
public class HarvestableTree : MonoBehaviour {
	
	public ResourceType type;

	public int value;
	
	private bool brokenBase = false;

	private bool isBeingCarried = false;

	private UnitEssence hauler;

	private Grabbable grabbable;

	private void OnEnable() {
		grabbable = GetComponent<Grabbable>();
		grabbable.Events.Add(this, GrabCallback);
	}

	private void GrabCallback(object sender, GrabEvent eventCode, object data) {
		switch(eventCode) {
			case GrabEvent.GotGrabbed:
				SetBaseBroken();
				break;
		}
	}

	private void Update() {
		if ( isBeingCarried) {
			transform.position = hauler.transform.position + Vector3.up * 5f;
		}
	}
	
	/// <summary>
	/// Causes the rigidbody and constraints of the tree to act like its been broken at the base.
	/// </summary>
	public void SetBaseBroken() {
		if (brokenBase) {
			return;
		}
		Rigidbody myBody = GetComponent<Rigidbody>();
		if ( myBody==null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		{
			var joint = gameObject.AddComponent<CharacterJoint>();
			//joint.autoConfigureConnectedAnchor = true;
			joint.breakForce = 4000;
			joint.breakTorque = 4000;

			var jointLimit = joint.lowTwistLimit;
			jointLimit.limit = 8.7f;
			joint.lowTwistLimit = jointLimit;

			jointLimit = joint.highTwistLimit;
			jointLimit.limit = 50f;
			joint.highTwistLimit = jointLimit;

			var swingLimit = joint.swing1Limit;
			swingLimit.limit = 17f;
			joint.swing1Limit = swingLimit;

			swingLimit = joint.swing2Limit;
			swingLimit.limit = 17f;
			joint.swing2Limit = swingLimit;
		}

		{
			var joint = gameObject.AddComponent<CharacterJoint>();
			//joint.autoConfigureConnectedAnchor = true;
			joint.breakForce = 6000;
			joint.breakTorque = 6000;
		}

		var hexMap = HexNavMeshManager.GetHexMap();
		hexMap.NotifyOfStaticObstacleRemove(gameObject);

		brokenBase = true;

	}

	public void SetBeingCarried( UnitEssence hauler ) {
		Rigidbody myBody = GetComponent<Rigidbody>();
		if (myBody == null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		myBody.isKinematic = true;

		isBeingCarried = true;
		this.hauler = hauler;

		transform.rotation = Quaternion.identity;

		var hexMap = HexNavMeshManager.GetHexMap();
		hexMap.NotifyOfStaticObstacleRemove(gameObject);
	}

	public void SetNotBeingCarried() {
		Rigidbody myBody = GetComponent<Rigidbody>();
		if (myBody == null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		myBody.isKinematic = false;

		isBeingCarried = false;
		this.hauler = null;
	}
}
