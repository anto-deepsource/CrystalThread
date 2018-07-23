using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HexMap.Pathfinding;

public static class Utilities {

	// Gets and returns the stated component or adds an appropriate one

	public static NavMeshAgent NavAgent(GameObject targetObject) {
		NavMeshAgent agent = targetObject.GetComponent<NavMeshAgent>();

		if (agent == null) {
			agent = targetObject.AddComponent<NavMeshAgent>();
			//agent.radius = radius * 0.9f; // just slightly smaller than our capsule collider
			//agent.height = height * 0.9f;
			//agent.speed = speed;
			agent.angularSpeed = 270f;
			//agent.acceleration = speed * 1.99f;
			//agent.stoppingDistance = radius * 5.5f;
			// Don't update position automatically (for animation purposes)
			agent.updatePosition = false;
			agent.updateRotation = true;
			agent.autoTraverseOffMeshLink = false;

			//agent.destination = goal.position;
		}
		return agent;
	}

	public static HexNavAgent HexNavAgent(GameObject targetObject) {
		HexNavAgent agent = targetObject.GetComponent<HexNavAgent>();

		if (agent == null) {
			agent = targetObject.AddComponent<HexNavAgent>();
			agent.moveSpeed = 6;
			
		}
		return agent;
	}

	public static HexNavLocalAgent HexNavLocalAgent(GameObject targetObject) {
		HexNavLocalAgent agent = targetObject.GetComponent<HexNavLocalAgent>();

		if (agent == null) {
			agent = targetObject.AddComponent<HexNavLocalAgent>();
			agent.moveSpeed = 6;

		}
		return agent;
	}

	public static Rigidbody Rigidbody(GameObject targetObject) {
		Rigidbody myBody = targetObject.GetComponent<Rigidbody>();

		if (myBody == null) {
			myBody = targetObject.AddComponent<Rigidbody>();
			myBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		}
		return myBody;
	}

	public static AnimationMaster AnimationMaster(GameObject targetObject) {
		AnimationMaster aniMaster = targetObject.GetComponent<AnimationMaster>();

		if (aniMaster == null) {
			aniMaster = targetObject.AddComponent<AnimationMaster>();
		}
		return aniMaster;
	}

	public static ResourcesComponent Resources(GameObject targetObject) {
		ResourcesComponent resources = targetObject.GetComponent<ResourcesComponent>();

		//if (aniMaster == null) {
		//	aniMaster = targetObject.AddComponent<AnimationMaster>();
		//}
		return resources;
	}

	public static void GetComponentMaybe<T>(this GameObject gameObject,  ref T reference ) {
		if ( reference == null ) {
			reference = gameObject.GetComponent<T>();
		}
	}
	
}
