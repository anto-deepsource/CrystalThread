using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineBetweenTransforms : MonoBehaviour {

	public Transform[] targets;

	private LineRenderer lineRenderer;

	private void Awake() {
		lineRenderer = GetComponent<LineRenderer>();
	}

	private void Update() {
		if (lineRenderer == null) {
			lineRenderer = GetComponent<LineRenderer>();
		}

		lineRenderer.positionCount = targets.Length;
		
		for(int i = 0; i < targets.Length; i ++ ) {
			lineRenderer.SetPosition(i, targets[i].position);
		}
	}
}
