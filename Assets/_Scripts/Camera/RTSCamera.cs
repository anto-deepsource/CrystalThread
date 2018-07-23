using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour {

	public float smoothing = 5f;
	public Vector3 offset = new Vector3(0.0f, 15.0f, -22.0f);
	public Camera controlledCamera;
	public Transform target;

	// Use this for initialization
	void Start () {
		if ( controlledCamera == null ) {
			controlledCamera = Camera.main;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// called after every physics update
	void FixedUpdate(){
		//Transform playerTransform = PlayerManager.GetPlayer().transform;
		UpdatePosition(smoothing * Time.deltaTime);
	}

	public void UpdatePosition( float smooth) {
		Vector3 targetCamPos = target.position + offset;

		controlledCamera.transform.position = Vector3.Lerp(controlledCamera.transform.position, targetCamPos, smooth);

		controlledCamera.transform.LookAt(target);
	}
}
