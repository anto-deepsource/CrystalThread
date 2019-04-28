using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageSource {
	public float amount;
	public float deepAmount;
	public bool isPlayer;
	public GameObject sourceObject;

	public Vector3 pushBack;
	public Vector3 hitPoint;
}