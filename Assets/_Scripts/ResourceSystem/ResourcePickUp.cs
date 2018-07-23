using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResourcePickUp : MonoBehaviour {

	public LayerMask layer;

	public ResourceType type;

	public int value;

	public GameObject pickupEffect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void TriggerPickup(GameObject targetObject) {
		ResourcesComponent theirResources = Utilities.Resources(targetObject);

		if (theirResources != null) {

			theirResources.Add(type, value);
			Destroy(gameObject);

			if (pickupEffect != null) {
				GameObject newEffect = GameObject.Instantiate(pickupEffect);
				newEffect.transform.position = transform.position;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		if (layer == (layer | (1 << other.gameObject.layer)) ) {
			TriggerPickup(other.gameObject);
		}
	}
}
