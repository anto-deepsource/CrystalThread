using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public GameObject prefabObject;
	public GameObject folder;

	public float objectsPerSecond = 1;

	private float spawnRate;
	private float timer;

	// Use this for initialization
	void Start () {
		if ( objectsPerSecond <= 0 ) {
			enabled = false;
			return;
		}
		spawnRate = 1f / objectsPerSecond;
		timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;

		if ( timer > spawnRate ) {
			GameObject parentFolder = folder != null ? folder : gameObject;
			GameObject spawn = Object.Instantiate(prefabObject, parentFolder.transform);
			spawn.transform.position = transform.position;

			timer -= spawnRate;
		}
	}
}
