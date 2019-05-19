using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour {

	public float translationScale = 10;
	public Camera controlledCamera;

	private Vector3 targetPosition;

	void Start () {
		if ( controlledCamera == null ) {
			controlledCamera = Camera.main;
		}

		targetPosition = controlledCamera.transform.position;
	}
	
	void Update () {
		UpdateMouseInput();
		UpdatePosition();
	}
	
	private void UpdateMouseInput() {
		if (Input.GetMouseButtonDown(2)) {
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (Input.GetMouseButton(2)) {
			var mouseX = Input.GetAxis("Mouse X");
			var mouseY = Input.GetAxis("Mouse Y");

			targetPosition.x -= mouseX * translationScale;
			targetPosition.z -= mouseY * translationScale;
		}
		if (Input.GetMouseButtonUp(2)) {
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void UpdatePosition() {
		Vector3 targetCamPos = targetPosition;

		controlledCamera.transform.position = Vector3.Lerp(controlledCamera.transform.position, targetCamPos, 0.5f);

		//controlledCamera.transform.LookAt(target);
	}
}
