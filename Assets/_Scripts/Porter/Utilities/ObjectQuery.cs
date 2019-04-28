using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectQuery {

	public GameObject gameObject;
	public Faction faction;
	public bool hasBlackboard = false;
	public bool isUnit = false;
	public bool isPlayer = false;
	public bool isStaticObstacle = false;
	public bool isPickup = false;
	public bool isHarvestable = false;
	public bool isResourceable = false;
	public bool isResourceReceptical = false;
}
