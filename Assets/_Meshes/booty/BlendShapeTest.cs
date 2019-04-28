using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeTest : MonoBehaviour {

	[Range(0,100)]
	public float weight = 0;

	SkinnedMeshRenderer skinnedMeshRenderer;
	Mesh skinnedMesh;

	void Start () {
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
		Debug.Log(skinnedMesh.blendShapeCount);
	}
	
	void Update () {
		skinnedMeshRenderer.SetBlendShapeWeight(0, weight);
	}
}
