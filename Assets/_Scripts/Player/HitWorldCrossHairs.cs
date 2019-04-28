using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWorldCrossHairs : MonoBehaviour {

	public LayerMask hitLayer;
	public RectTransform retical;
	[Tooltip("The main character or object who's shoulder we are looking over." +
		"Used to adjust the start of the ray cast and ignore anything between the camera and the main character.")]
	public Transform targetObject;

	public float range = 1000;

	public float interactionRange = 100;

	private GameObject spinIndicators;


	public float minInteractableDot = .6f;

	[HideInInspector]
	public GameObject closestInteractable;

	public bool HasCloseInteractable { get { return closestInteractable != null;  } }

	private void Start() {
		spinIndicators = transform.Find("SpinIndicators").gameObject;
	}

	public bool HittingSomething { get; private set; }

	[HideInInspector]
	public RaycastHit aimRayHit;


	void Update () {
		
		Vector2 reticalScreenPos = retical.position;
		Ray camRay = Camera.main.ScreenPointToRay(reticalScreenPos);

		Vector3 camThisVector = targetObject.position - camRay.origin;
		Vector3 projection = CommonUtils.Projection(camThisVector, camRay.direction);
		camRay.origin += projection;

		
		if (Physics.Raycast(camRay, out aimRayHit, range, hitLayer)) {
			HittingSomething = true;
			transform.position = aimRayHit.point;
			transform.rotation = Quaternion.LookRotation(aimRayHit.normal);

		} else {
			HittingSomething = false;
		}

		ChooseBestInteractionObject();
	}

	public void ShowSpinIndicators() {
		spinIndicators.SetActive(true);
	}

	public void HideSpinIndicators() {
		spinIndicators.SetActive(false);
	}

	private void ChooseBestInteractionObject() {
		var units = QueryManager.GetNearbyUnits(targetObject.transform.position, interactionRange);

		Vector3 cameraVector = Camera.main.transform.forward;

		// choose the one closest to where we're looking
		float highestDot = 0;
		closestInteractable = null;
		foreach( var unit in units) {
			if ( unit.transform != targetObject ) {
				Vector3 unitVector = unit.transform.position - Camera.main.transform.position;
				float dot = Vector3.Dot(unitVector.normalized, cameraVector);
				if ( dot > minInteractableDot && (closestInteractable == null || dot > highestDot) ) {
					highestDot = dot;
					closestInteractable = unit;
				}
			}
			
		}
	}
	
}
