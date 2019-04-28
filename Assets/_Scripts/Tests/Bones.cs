using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Bones : MonoBehaviour {

	[Tooltip("The sprite for the face that has its normal parralel to the x-axis.")]
	public SpriteRenderer graphicX;

	[Tooltip("The sprite for the face that has its normal parralel to the y-axis.")]
	public SpriteRenderer graphicY;

	[Tooltip("The sprite for the face that has its normal parralel to the z-axis.")]
	public SpriteRenderer graphicZ;

	public Transform endPosition;

	[HideInInspector]
	public bool shouldLockDistance = false;

	[HideInInspector]
	public float lockDistance = 1f;

	public Vector3 BoneVector() {
		return endPosition.position - transform.position;
	}

	public void Update() {
		if (  endPosition ==null ) {
			return;
		}


		var vector = BoneVector();

		var midPoint = vector * 0.5f + transform.position;

		if (graphicX != null ) {
			graphicX.transform.position = midPoint;
			var crossX = Vector3.Cross(transform.right, vector);
			graphicX.transform.rotation = Quaternion.LookRotation(crossX, -vector);
		}
		if (graphicY != null) {
			graphicY.transform.position = midPoint;
			var crossY = Vector3.Cross(transform.forward, vector);
			graphicY.transform.rotation = Quaternion.LookRotation(crossY, -vector);
		}

		if (graphicZ != null) {
			graphicZ.transform.position = midPoint;
			graphicZ.transform.LookAt(transform);
			//var crossZ = Vector3.Cross(transform.forward, vector);
			//graphicZ.transform.rotation = Quaternion.LookRotation(crossZ, -vector);
		}
	}
}
