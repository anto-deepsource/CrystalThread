using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearIfInCamerasWay : MonoBehaviour {

	public Material hideMaterial;

	private GameObject player;
	private GameObject cam;

	private MeshRenderer meshRenderer;
	private Material originalMaterial;
	private bool showing = true;

	/// <summary>
	/// Static hack to reduce the number raycasts per frame while allowing multiple objects to be hidden this way
	/// </summary>
	private static int count = 0;
	private static int delay = 0;
	private static RaycastHit[] hits;
	/// <summary>
	/// Disappear is plenty fast, but its still not necessary to recall the raycast every frame, my opinion
	/// </summary>
	private static int frameSkip = 3; // 1 is no skip

	private void OnEnable() {
		count++;
	}

	private void OnDisable() {
		count--;
	}

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		cam = Camera.main.gameObject;

		meshRenderer = GetComponent<MeshRenderer>();
		originalMaterial = meshRenderer.material;
		showing = true;
	}
	
	// Update is called once per frame
	void Update () {

		UpdateRaycast();

		if ( hits == null ) {
			return;
		}

		bool show = true;

		foreach (var hit in hits) {
			try {
				if (hit.collider.gameObject == gameObject) {
					float camPlayerDistance = CommonUtils.Distance(player.transform.position, cam.transform.position);
					float camThisDistance = CommonUtils.Distance(hit.point, cam.transform.position);
					if (camThisDistance < camPlayerDistance) {
						show = false;
						break;
					}
				}
			} catch( MissingReferenceException e ) {
				// if, somewhen between the time we last updated the ray hits and now, one of the hit objects was destroyed (it happens sometimes)
				// trying to access the collider will throw an exception.
				// if we get hit with one, lets force the raycasts to update and just exit for now
				UpdateRaycast(true);
				return;
			}
		}
		
		if ( show!=showing) {
			ToggleShowing();
		}
	}

	private void ToggleShowing() {
		if ( showing ) {
			meshRenderer.material = hideMaterial;
		} else {
			meshRenderer.material = originalMaterial;
		}

		showing = !showing;
	}



	private static void UpdateRaycast(bool force = false) {
		delay ++ ;
		if (force || delay == count*frameSkip) {
			delay = 0;
			hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)));
		}
	}
}
