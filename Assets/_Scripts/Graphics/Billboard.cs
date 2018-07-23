using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

	public bool lockY = true;

	public Transform target;

	// Update is called once per frame
	void Update () {
		Vector3 t = Camera.main.transform.position;

		if ( target!=null )
			t = target.transform.position;
		if ( lockY )
			t.y = transform.position.y;
		transform.LookAt(t, Vector3.up);
	}
}
