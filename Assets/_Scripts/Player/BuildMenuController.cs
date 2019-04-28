using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildMenuController : MonoBehaviour {

	public GameObject buildMenu;
	public HitWorldCrossHairs crossHairs;

	public float placeRotateSpeed = 3;

	private bool showingMenu = false;

	private bool placingBuilding = false;
	private GameObject placingObject;
	

	void Start() {
		
		HideBuildMenu();
	}

	private void Update() {
		if (placingBuilding) {
			if (Input.GetButtonDown("Interact")) {
				PlaceBuilding();
			}
			else if (Input.GetButton("SpinCW")) {
				placingObject.transform.Rotate(new Vector3(0, placeRotateSpeed, 0));
			}
			else if (Input.GetButton("SpinCCW")) {
				placingObject.transform.Rotate(new Vector3(0, -placeRotateSpeed, 0));
			}
		}
		else if(crossHairs.HittingSomething) {

			// test the colliding object for various components and handle looking at them and interacting with them
			GameObject targetObject = crossHairs.aimRayHit.collider.gameObject;

			Buildable buildable = targetObject.GetComponentInParent<Buildable>();

			if (buildable != null) {
				buildable.OnPlayerLookAt(gameObject);

				if (Input.GetButtonDown("Interact")) {
					buildable.OnPlayerInteract(gameObject);
				}
			}
		}
		if (Input.GetButtonDown("BuildMenu")) {
			if (!showingMenu) {
				ShowBuildMenu();
			}
			else {
				HideBuildMenu();
			}
		}


	}

	public void ShowBuildMenu() {
		buildMenu.SetActive(true);
		showingMenu = true;
	}

	public void HideBuildMenu() {
		if (buildMenu != null) {
			buildMenu.SetActive(false);
		}
		showingMenu = false;
	}


	public void StartSettingBuilding(StructureData data) {
		placingObject = GameObject.Instantiate(data.gameObject);
		placingObject.SetActive(true);
		//placingObject.transform.SetParent(crossHairs.transform);
		//placingObject.transform.localPosition = Vector3.zero;
		placingObject.GetComponent<Buildable>().StartSetting(crossHairs.transform);

		placingBuilding = true;
		crossHairs.ShowSpinIndicators();
		HideBuildMenu();
	}

	public void ClearSettingBuilding() {
		if (placingObject != null) {
			Destroy(placingObject);
			placingObject = null;
		}
		placingBuilding = false;
		crossHairs.HideSpinIndicators();
	}

	public void PlaceBuilding() {
		placingObject.transform.SetParent(null);
		placingObject.GetComponent<Buildable>().FinishSetting();
		placingBuilding = false;
		placingObject = null;
		crossHairs.HideSpinIndicators();
	}
}
