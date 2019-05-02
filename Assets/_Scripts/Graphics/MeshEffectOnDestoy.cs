using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEffectOnDestoy : MonoBehaviour {

	public MeshRenderer meshRenderer;

	public HealthComponent healthComponent;

	public Material effectMaterial;

	public float effectTime = 1f;

	private bool doingEffect = false;

	private float effectTimer = 0;

	void Start () {
		if (healthComponent == null) {
			healthComponent = GetComponent<HealthComponent>();
		}

		healthComponent.Events.Add(this, HealthCallbacks);
	}

	private void HealthCallbacks(int eventCode, object data) {
		switch ((BlackboardEventType)eventCode) {
			case BlackboardEventType.Death:
				StartEffect();
				break;
		}
	}
	
	private void StartEffect() {
		if (meshRenderer == null) {
			meshRenderer = GetComponent<MeshRenderer>();
		}

		meshRenderer.material = effectMaterial;
		meshRenderer.sharedMaterial = effectMaterial;
		var mats = new Material[meshRenderer.materials.Length];
		for ( int i = 0; i < meshRenderer.materials.Length; i++) {
			mats[i] = effectMaterial;
			meshRenderer.sharedMaterials[i] = effectMaterial;
		}

		meshRenderer.materials = mats;

		doingEffect = true;
		effectTimer = 0;
	}

	private void Update() {
		if ( doingEffect) {
			effectTimer += Time.deltaTime;
			if ( effectTimer >= effectTime ) {
				Destroy(gameObject);
			} else {
				for (int i = 0; i < meshRenderer.materials.Length; i++) {
					meshRenderer.materials[i].SetFloat("_DissolveScale", effectTimer/ effectTime);
				}
			}
		}
	}
}
