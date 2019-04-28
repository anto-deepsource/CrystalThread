using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchObjects : MonoBehaviour {

	public GameObject prefabObject;

	public float launchSpeed = 10f;

	public float objectsPerSecond = 1.1f;

	private float spawnTimer = 0;

	private void OnEnable() {
		spawnTimer = 0;
	}
	
	void Update () {
		spawnTimer += Time.deltaTime;

		if (spawnTimer > objectsPerSecond) {
			spawnTimer -= objectsPerSecond;
			SpawnObject();
		}
	}

	private void SpawnObject() {
		var newObject = Instantiate(prefabObject);
		newObject.transform.position = transform.position;

		var theirBody = newObject.GetComponentInChildren<Rigidbody>();
		theirBody.velocity = transform.forward * launchSpeed;
	}
}
