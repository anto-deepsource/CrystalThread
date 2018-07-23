using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour {

	public float speed = 6f;
	public float radius = 0.5f;
	public float height = 1.5f;

	public Transform goal;
	NavMeshAgent navAgent;
	Rigidbody unitRigidbody;
	bool usingNavAgent = false;
	Vector2 smoothDeltaPosition = Vector2.zero;
	Vector3 movement = Vector3.zero; // movement speed if NOT usingNavAgent
	Vector3 velocity = Vector3.zero; // distance moved last frame

	void Start () {
		//~ navAgent = GetComponent<NavMeshAgent>();
		
		
		// create a navmesh agent component
		navAgent = gameObject.AddComponent<NavMeshAgent>();
		navAgent.radius = radius * 0.9f; // just slightly smaller than our capsule collider
		navAgent.height = height * 0.9f;
		navAgent.speed = speed;
		navAgent.angularSpeed = 270f;
		navAgent.acceleration = speed*1.99f;
		navAgent.stoppingDistance = radius * 5.5f;
		// Don't update position automatically (for animation purposes)
		navAgent.updatePosition = false;
		navAgent.autoTraverseOffMeshLink = true;
		
		navAgent.destination = goal.position;
		usingNavAgent = true;
		
		unitRigidbody = gameObject.AddComponent<Rigidbody>();
//		unitRigidbody.drag = Mathf.Infinity;
		unitRigidbody.angularDrag = Mathf.Infinity;
//		unitRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
		unitRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		
		
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (usingNavAgent) {
			velocity = new Vector3 (navAgent.nextPosition.x, unitRigidbody.position.y, navAgent.nextPosition.z) - unitRigidbody.position;
			unitRigidbody.MovePosition ( unitRigidbody.position + velocity );
		} else {
			
		}
		navAgent.destination = goal.position;
	}
}
