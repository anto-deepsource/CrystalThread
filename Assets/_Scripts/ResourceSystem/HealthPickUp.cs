using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUp : MonoBehaviour {
	
	public float normalAmount = 10;
	public float normalVariance = 1;

	public float deepAmount = 3;
	public float deepVariance = 3;

	public GameObject pickupEffect;

	public void OnTriggerEnter(Collider other) {
		HealthComponent theirHealthComponent = other.gameObject.GetComponent<HealthComponent>();
		if (theirHealthComponent != null) {

			DamageSource myHealthRestore = new DamageSource() {
				sourceObject = gameObject,
				amount = normalAmount + Random.Range(-1f,1f) * 3,
				deepAmount = deepAmount + Random.Range(-1f, 1f) * 1,
			};

			theirHealthComponent.ApplyHealth(myHealthRestore);
			Destroy(gameObject);

			// maybe play an effect
			if (pickupEffect != null) {
				GameObject newEffect = GameObject.Instantiate(pickupEffect);
				newEffect.transform.position = transform.position;
			}
		}
	}
	
}
