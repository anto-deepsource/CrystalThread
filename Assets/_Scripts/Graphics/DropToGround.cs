using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropToGround : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;
	public bool controlRotation = false;

	public Vector3 offset = Vector3.zero;

	public Vector3 castPositionOffset = Vector3.zero;
	
	void Update () {
		RaycastHit camRayhit;
		if (Physics.Raycast(transform.parent.position + castPositionOffset, 
				Vector3.down, out camRayhit, 100, groundLayer)) {
			transform.position = camRayhit.point + offset;

			if (controlRotation) {
				transform.rotation = Quaternion.LookRotation(camRayhit.normal);
			}
		}
	}
}
