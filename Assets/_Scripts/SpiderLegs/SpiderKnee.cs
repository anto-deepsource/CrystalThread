using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderKnee : MonoBehaviour {

	public Transform spiderLeg;

	public Transform targetPosition;

	public float legLength = 1f;

	private void Update() {
		Vector3 verticalOffset = Vector3.up * legLength;
		Vector3 midPoint = (spiderLeg.position + targetPosition.position) * 0.5f;
		transform.position = verticalOffset + midPoint;
	}
}
