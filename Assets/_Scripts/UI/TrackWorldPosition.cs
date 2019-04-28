using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TrackWorldPosition : MonoBehaviour {

	public GameObject target;

	public Vector3 worldOffset = Vector3.zero;
	public Vector2 screenOffset = Vector2.zero;
	
	void Update () {
		if ( target!=null ) {
			TrackPosition(gameObject, target);
		}
		
	}

	public void TrackPosition(GameObject uiObject, GameObject target) {
		Vector3 newScreenPoint = Camera.main.WorldToScreenPoint(target.transform.TransformPoint(worldOffset));
		newScreenPoint.x += screenOffset.x;
		newScreenPoint.y += screenOffset.y;
		uiObject.transform.position = newScreenPoint;

		//float left = rect.rect.width * 0.5f + screenBounds;
		//float right = Screen.width - rect.rect.width * 0.5f - screenBounds;
		//float top = Screen.height - rect.rect.height * 0.5f - screenBounds;
		//float bottom = rect.rect.height * 0.5f + screenBounds;

		//bool outOfBounds = false;

		//if (transform.position.x < left) {
		//	transform.position = new Vector2(left, transform.position.y);
		//	outOfBounds = true;
		//}


		//if (transform.position.x > right) {
		//	transform.position = new Vector2(right, transform.position.y);
		//	outOfBounds = true;
		//}

		//if (transform.position.y > top) {
		//	transform.position = new Vector2(transform.position.x, top);
		//	outOfBounds = true;
		//}

		//if (transform.position.y < bottom) {
		//	transform.position = new Vector2(transform.position.x, bottom);
		//	outOfBounds = true;
		//}

		//if (visible) {

		//	if (outOfBounds && disappearWhenOffScreen) {
		//		CommonUtils.DisableChildren(transform);
		//		visible = false;
		//	}
		//	if (!outOfBounds && disappearWhenOnScreen) {
		//		CommonUtils.DisableChildren(transform);
		//		visible = false;
		//	}
		//} else {
		//	if (!outOfBounds && disappearWhenOffScreen) {
		//		CommonUtils.EnableChildren(transform);
		//		visible = true;
		//	}
		//	if (outOfBounds && disappearWhenOnScreen) {
		//		CommonUtils.EnableChildren(transform);
		//		visible = true;
		//	}
		//}
	}
}
