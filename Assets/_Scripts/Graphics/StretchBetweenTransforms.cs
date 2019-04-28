using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class StretchBetweenTransforms : MonoBehaviour {

	public Transform targetStartPosition;
	public Transform targetEndPosition;


	//public Transform localAnchorA;
	public Transform localAnchorForEndPosition;

	private void Update() {

		if (targetEndPosition==null || targetStartPosition==null) {
			return;
		}

		transform.position = targetStartPosition.position;

		transform.LookAt(targetEndPosition.position);

		float localAnchorDistance = 1;

		if (localAnchorForEndPosition != null) {
			localAnchorDistance = localAnchorForEndPosition.localPosition.magnitude;
		}

		float targetDistance = Vector3.Distance(targetStartPosition.position, targetEndPosition.position);
		var newScale = transform.localScale;
		newScale.z = targetDistance/ localAnchorDistance;
		transform.localScale = newScale;

	}
}
