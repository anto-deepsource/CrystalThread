using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTerrainTrigger : MonoBehaviour {

	//private void OnTriggerStay(Collider other) {
	//	if ( other.gameObject.GetComponent<UnitEssence>() != null ) {
	//		Debug.Log("Trigger unit");
	//	}
	//}

	private void OnTriggerEnter(Collider other) {
		var theirEssence = other.gameObject.GetComponent<UnitEssence>();
		if (theirEssence != null) {
			theirEssence.IsSwimming = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		var theirEssence = other.gameObject.GetComponent<UnitEssence>();
		if (theirEssence != null) {
			theirEssence.IsSwimming = false;
		}
	}
}
