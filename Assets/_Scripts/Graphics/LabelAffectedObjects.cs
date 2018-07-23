using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelAffectedObjects : MonoBehaviour {

	public Blackboard blackboard;

	public GameObject labelPrefab;

	public Vector3 offset = Vector3.zero;

	public float screenBounds = 10f;

	public bool disappearWhenOffScreen = false;
	public bool disappearWhenOnScreen = false;

	List<GameObject> labels = new List<GameObject>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		while (labels.Count < blackboard.affected.Length) {
			labels.Add(GameObject.Instantiate(labelPrefab, transform) );
		}

		int i = 0;
		foreach (var gameObject in blackboard.affected) {
			GameObject thisLabel = labels[i];
			labels[i].SetActive(true);
			TrackPosition(thisLabel, gameObject);
			i++;
		}
		for (; i < labels.Count; i++) {
			labels[i].SetActive(false);
		}
	}

	public void TrackPosition( GameObject uiObject, GameObject target) {
		uiObject.transform.position = Camera.main.WorldToScreenPoint(target.transform.position + offset);

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
