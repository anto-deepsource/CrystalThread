using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTransformToGround : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;

	public float offset = 0;

	public float maxReach = 10f;

	private Vector3 startLocalPosition;

	void Start () {
		startLocalPosition = transform.localPosition;
	}
	
	void Update () {
		RaycastHit rayhit;
		bool isGroundWithinReach =
				Physics.Raycast(transform.parent.TransformPoint(startLocalPosition), Vector3.down, out rayhit, maxReach, groundLayer);

		transform.position = rayhit.point + Vector3.up * offset;
	}
}
