using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistsOnlyWhenInSight : MonoBehaviour {

	public Collider visibleShape;

	public bool affectChildObjects = true;
	public bool affectSiblingComponents = true;

	bool visible = true;

	private void Start() {
		if (visibleShape == null) {
			visibleShape = GetComponent<Collider>();
			if (visibleShape == null) {
				visibleShape = GetComponentInParent<Collider>();
			}
			if (visibleShape == null) {
				visibleShape = GetComponentInChildren<Collider>();
			}
		}
		
	}

	void Update () {
		Camera camera = Camera.main;
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

		if (visibleShape != null && GeometryUtility.TestPlanesAABB(planes, visibleShape.bounds) ) {
			if (!visible) {
				Show();
			}
		} else {
			if (visible) {
				Hide();
			}
		}
	}

	private void Hide() {
		if (affectChildObjects) {
			CommonUtils.DisableChildren(transform);
		}
		if (affectSiblingComponents) {
			foreach( var component in gameObject.GetComponents<MeshRenderer>()) {
				if ( component != this ) {
					component.enabled = false;
				}
				
			}
		}

		visible = false;
	}

	private void Show() {
		if (affectChildObjects) {
			CommonUtils.EnableChildren(transform);
		}
		if (affectSiblingComponents) {
			foreach (var component in gameObject.GetComponents<MeshRenderer>()) {
				if (component != this) {
					component.enabled = true;
				}

			}
		}
		visible = true;
	}
}
