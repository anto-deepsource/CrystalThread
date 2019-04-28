using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public GameObject prefabObject;
	public GameObject folder;

	public int maxObjects = 10;

	public int initialObjects = 3;

	public float radius = 15f;

	public float objectsPerSecond = 1;

	public Vector3 initialScale = Vector3.one;
	public Vector3 scaleVariance = Vector3.zero;

	private int objectCount = 0;

	private float spawnRate;
	private float timer;

	// Use this for initialization
	void Start () {
		prefabObject.SetActive(false);

		if ( objectsPerSecond <= 0 ) {
			enabled = false;
			return;
		}
		spawnRate = 1f / objectsPerSecond;
		timer = 0;

		while( objectCount < initialObjects) {
			Spawn();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if ( objectCount < maxObjects ) {
			timer += Time.deltaTime;

			if (timer > spawnRate) {
				Spawn();

				timer -= spawnRate;
			}
		}
		
	}

	private void Spawn() {
		GameObject parentFolder = folder != null ? folder : gameObject;
		GameObject spawn = Object.Instantiate(prefabObject, parentFolder.transform);
		spawn.SetActive(true);

		Vector3 position = HexNavMeshManager.GetAnyPointInArea(transform.position, radius);
		position = HexNavMeshManager.WorldPosToWorldPosWithGround(position);
		spawn.transform.position = position;

		Vector3 scale = initialScale;
		if ( scaleVariance != Vector3.zero ) {
			var random = new Vector3(Random.Range(-1f,1f), 
				Random.Range(-1f, 1f), Random.Range(-1f, 1f));

			scale.x = scale.x + scaleVariance.x * random.x;
			scale.y = scale.y + scaleVariance.y * random.y;
			scale.z = scale.z + scaleVariance.z * random.z;
		}

		spawn.transform.localScale = scale;

		objectCount++;
	}
}
