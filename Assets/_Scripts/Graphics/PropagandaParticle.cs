using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaParticle : MonoBehaviour {

	public float startX = 0;
	public float startY = 0;

	public float a = 1;

	public float C1 = 1;

	public Vector2 V1 = Vector2.one;

	public float b = 1;

	public float C2 = 1;

	public Vector2 V2 = Vector2.one;

	public float startTime = -6f;
	public float lifeTime = 6f;

	

	public ParticleType type = ParticleType.Saddle;

	private float t = -10;

	private void OnEnable() {
		t = startTime;
		
	}

	void Update () {
		t += Time.deltaTime;

		float x = 0;
		float y = 0;

		switch (type) {
			case ParticleType.Saddle:
				x = V1.x * C1 * Mathf.Exp(a * t) + V2.x * C2 * Mathf.Exp(b * t) + startX;
				y = V1.y * C1 * Mathf.Exp(a * t) + V2.y * C2 * Mathf.Exp(b * t) + startY;
				break;
			case ParticleType.Ellipse:

				x = V1.x * C1 * Mathf.Cos(a * t) + V2.x * C2 * Mathf.Sin(b * t) + startX;
				y = V1.y * C1 * Mathf.Cos(a * t) + V2.y * C2 * Mathf.Sin(b * t) + startY;

				break;
			case ParticleType.Spiral:
				break;
		}

		transform.position = new Vector3(x, y);

		if (lifeTime>0 && t > lifeTime) {
			Destroy(gameObject);
		}
	}

}
public enum ParticleType {
	Saddle,
	Ellipse,
	Spiral,
}