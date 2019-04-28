using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queryable : MonoBehaviour {

	public UnitEssence unitEssence;

	public ObjectQuery query;

	public void Awake() {
		query = new ObjectQuery() { gameObject = gameObject };

		if (unitEssence == null) {
			unitEssence = gameObject.GetUnitEssence();
		}
		if (unitEssence != null) {
			query.isUnit = true;
			query.faction = unitEssence.faction;
		}

		Buildable buildable = gameObject.GetComponent<Buildable>();
		if ( buildable != null) {
			query.isStaticObstacle = buildable.Built;
		}
	
		query.hasBlackboard = false;
		
		query.isPlayer = false;

		ResourcePickUp resourcePickUp = GetComponent<ResourcePickUp>();
		if (resourcePickUp != null) {
			query.isPickup = true;
		}

		HarvestableTree harvestable = GetComponent<HarvestableTree>();
		if (harvestable != null) {
			query.isHarvestable = true;
		}

		Resourceable resourceable = GetComponent<Resourceable>();
		if (resourceable != null) {
			query.isResourceable = true;
		}

		ResourceReceptical resourceReceptical = GetComponent<ResourceReceptical>();
		if (resourceReceptical != null) {
			query.isResourceReceptical = true;
		}

		QueryManager.RegisterObject(query);
	}

	public void OnDestroy() {
		QueryManager.UnregisterObject(query);
	}
}

