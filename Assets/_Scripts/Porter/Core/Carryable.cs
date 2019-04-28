using HexMap;
using UnityEngine;

/// <summary>
/// Describes an object that can be 'carried' by another (probably a unit).
/// </summary>
public class Carryable : MonoBehaviour {

	private bool isBeingCarried = false;

	private UnitEssence hauler;

	private Transform originalParent;

	private void Start() {
		originalParent = transform.parent;
	}

	public void SetBeingCarried(UnitEssence hauler) {
		Rigidbody myBody = GetComponent<Rigidbody>();
		if (myBody == null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		myBody.isKinematic = true;

		isBeingCarried = true;
		this.hauler = hauler;

		transform.SetParent(hauler.transform);
		//transform.rotation = Quaternion.identity;

		var hexMap = HexNavMeshManager.GetHexMap();
		hexMap.NotifyOfStaticObstacleRemove(gameObject);
	}

	public void SetNotBeingCarried() {
		Rigidbody myBody = GetComponent<Rigidbody>();
		if (myBody == null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		myBody.isKinematic = false;

		isBeingCarried = false;
		this.hauler = null;

		transform.SetParent(originalParent);
	}
}
