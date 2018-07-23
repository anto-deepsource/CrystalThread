using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerWhip : MonoBehaviour {

	public Transform endObject;
	public Transform crossHairs;
	public Transform hitBox;

	public float camAngleOffset = 20;

	public float whipLength = 4;

	private LineRenderer lineRenderer;

	/// <summary>
	/// The end/tail point of the whip. Moves in space relative
	/// to the player and is used to calculate collisions.
	/// </summary>
	private Vector3 whipEnd;

	private Blackboard currentTarget;

	private PolyShape hitBoxShape;

	private float delta = 0;

	void Start () {

		lineRenderer = GetComponent<LineRenderer>();
		hitBoxShape = hitBox.GetComponent<PolyShape>();
	}
	
	void Update () {
		UpdateTargetedUnit();

		if (Input.GetMouseButton(0)) {

			WhipGrab();
		} else {
			
		}
		//whipEnd = Vector3.right* whipLength;

		//if ( Input.GetMouseButton(0)) {
		//// In the direction the camera is pointing
		//whipEnd = transform.InverseTransformDirection(Camera.main.transform.forward) * whipLength;

		//Quaternion direction = Quaternion.RotateTowards(
		//	Quaternion.Euler(transform.InverseTransformDirection(Camera.main.transform.forward)),
		//	Quaternion.Euler(transform.InverseTransformDirection(Camera.main.transform.up)),
		//	delta);
		//whipEnd = direction.eulerAngles * whipLength;

		//// Vector pointing from the player's position towards the player's crosshairs
		//whipEnd = transform.InverseTransformDirection((crossHairs.position - transform.position).normalized) * whipLength;
		//}

		Vector3 camDir = Camera.main.transform.localRotation.eulerAngles;

		hitBox.localRotation = Quaternion.identity;
		hitBox.RotateAround(hitBox.position, transform.right, camDir.x - 360 - camAngleOffset);

		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, transform.TransformPoint( whipEnd) );
		
	}

	private void UpdateTargetedUnit() {
		endObject.gameObject.SetActive(false);

		for (int i = 0; i + 1 < hitBoxShape.PointCount; i++) {
			for (int j = i + 1; j + 1 < hitBoxShape.PointCount; j++) {
				RaycastHit hitInfo;
				if (Physics.Linecast(
						hitBoxShape.GetPointWorldPosition(i),
						hitBoxShape.GetPointWorldPosition(j), out hitInfo, 1 << LayerMask.NameToLayer("Units"))) {
					endObject.gameObject.SetActive(true);
					GameObject hitObject = hitInfo.collider.gameObject;
					endObject.transform.position = hitObject.transform.position;
					Blackboard target = hitObject.GetComponent<Blackboard>();
					if ( target !=null  ) {
						SetCurrentTarget(target);
					}
					
					break;
				}
			}
		}
	}

	private void SetCurrentTarget( Blackboard newTarget ) {
		currentTarget = newTarget;
	}

	private void WhipGrab() {
		if ( currentTarget ==null ) {
			return;
		}

		whipEnd = transform.InverseTransformPoint( currentTarget.transform.position );
	}
}
