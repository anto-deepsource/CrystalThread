using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropToGround : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;
	public bool controlRotation = false;
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit camRayhit;
		if (Physics.Raycast(transform.parent.position, Vector3.down, out camRayhit, 100, groundLayer)) {
			transform.position = camRayhit.point + Vector3.up * 0.1f;

			if (controlRotation) {
				transform.rotation = Quaternion.LookRotation(camRayhit.normal);
			}
		}
	}
}
