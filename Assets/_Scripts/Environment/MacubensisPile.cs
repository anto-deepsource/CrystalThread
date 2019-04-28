using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ResourcesComponent))]
public class MacubensisPile : ResourceReceptical {

	public GameObject lifeGooPrefab;

	

	public float gooPerBody = 1f;

	public float gooPerSecond = 1 / 6f;

	public InWorldProgressBar progressBar;

	private ResourcesComponent resources;

	private float processTimer = 0;

	public float CurrentBodies { get { return resources.GetValueOf(ResourceType.BodyParts); } }

	private void Start() {
		ResetTimer();
		resources = GetComponent<ResourcesComponent>();
	}

	private void ResetTimer() {
		processTimer = 1f/gooPerSecond;
	}

	private void Update() {
		if (resources.CanAfford(ResourceType.BodyParts,1)) {
			processTimer -= Time.deltaTime;

			if (processTimer <= 0) {
				MakeNewGoo();
				ResetTimer();
			}
		}

		if (progressBar != null) {
			progressBar.value = 1f - processTimer* gooPerSecond;
		}
	}

	private void MakeNewGoo() {
		var newGoo = Instantiate(lifeGooPrefab, transform);
		newGoo.SetActive(true);
		newGoo.transform.position = 
			HexNavMeshManager.WorldPosToWorldPosWithGround(transform.position + Random.onUnitSphere * 2f);

		resources.Subtract(ResourceType.BodyParts, 1);

		//CurrentBodies -= 1f / gooPerBody;
		//CurrentBodies = Mathf.Max(0, CurrentBodies);

	}

	public override bool CanProcessResourceable(Resourceable resource) {
		return true;
	}

	public override void ProcessResourceable(Resourceable resource) {
		resources.Add(resource.rewards);
		Destroy(resource.gameObject);
	}
}
