using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ResourcesComponent))]
public class HumanColony : MonoBehaviour {

	public Faction faction;

	public float radius = 10f;

	//public GameObject nextBuilding;

	public GameObject processingMachinePrefab;

	[HideInInspector]
	public ResourcesComponent resources;
	
	void Start () {
		resources = GetComponent<ResourcesComponent>();
	}
	
	
}
