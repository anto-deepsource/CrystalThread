using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialProcessingPoint : MonoBehaviour {
	
	public List<ResourceType> acceptableResourceTypes;

	public HumanColony colony;

	#region Queryable Object things
	
	void Start() {

		HexNavMeshManager.GetHexMap().NotifyOfStaticObstacleAdd(gameObject);
	}
	
	#endregion

	public void ProcessTree( HarvestableTree tree ) {

		colony.resources.Add(tree.type, tree.value);
		Destroy(tree.gameObject);
	}
}
