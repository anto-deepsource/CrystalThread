using UnityEngine;
using System.Collections;

public class SpriteCastShadows : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SpriteRenderer renderer = GetComponent<SpriteRenderer> ();
		renderer.enabled = true;
//		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
