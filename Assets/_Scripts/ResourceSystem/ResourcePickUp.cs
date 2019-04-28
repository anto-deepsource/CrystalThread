using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResourcePickUp : MonoBehaviour {



	public LayerMask layer;

	public ResourceType type;

	public int value;

	public GameObject pickupEffect;
	
	public void OnTriggerEnter(Collider other) {
		if (layer == (layer | (1 << other.gameObject.layer))) {
			TriggerPickup(other.gameObject);
		}
	}

	void TriggerPickup(GameObject targetObject) {
		ResourcesComponent theirResources = Utilities.Resources(targetObject);

		if (theirResources != null) {

			theirResources.Add(type, value);
			Destroy(gameObject);

			// maybe play an effect
			if (pickupEffect != null) {
				GameObject newEffect = GameObject.Instantiate(pickupEffect);
				newEffect.transform.position = transform.position;
			}

			// maybe tell their blackboard about it
			Blackboard theirBlackboard = targetObject.GetComponent<Blackboard>();
			if ( theirBlackboard!=null ) {
				theirBlackboard.Inform(BlackboardEventType.ResourcesAdded, null);
			}
			
		}
	}

}
