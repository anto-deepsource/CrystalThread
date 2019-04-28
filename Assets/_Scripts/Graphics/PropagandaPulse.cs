using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaPulse : MonoBehaviour {

	public Vector3 startScale;

	public float scaleVariance = 1;

	public float scaleFactor = 0.01f;

	private float t;

	private void OnEnable() {
		startScale = transform.localScale;
	}

	void Update () {
		t += Time.deltaTime;

		float timeValue = Mathf.Sin(t * scaleFactor);
		transform.localScale = scaleVariance * timeValue*Vector3.one + startScale;
	}
}
