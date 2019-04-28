using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propaganda : MonoBehaviour {

	public float startVariance = 1;

	public float a = -1;
	public float aDelta = .1f;
	public float minA = -2;
	public float maxA = -0.1f;

	public float C1 = 1;
	public float C1Variance = 1;

	public Vector2 V1 = Vector2.one;
	public float V1delta = 0.1f;

	public float b = 1;
	public float bDelta = .1f;
	public float minB = 0.1f;
	public float maxB = 2f;

	public float C2 = 1;
	public float C2Variance = 1;

	public Vector2 V2 = Vector2.one;
	public float V2delta = 0.1f;

	public GameObject prefab;

	public float particleSpawnTime = .2f;

	public ParticleType spawnType = ParticleType.Saddle;

	private float spawnTimer = 0;

	public Transform particleFolder;

	public float hueChangeScale = 0.1f;
	public HSBColor particleColor = HSBColor.FromColor(Color.blue);

	public SpriteRenderer backgroundSprite;

	private void Start() {
		prefab.SetActive(false);
	}

	void Update() {
		spawnTimer += Time.deltaTime;
		if ( spawnTimer > particleSpawnTime ) {
			SpawnParticle();
			spawnTimer -= particleSpawnTime;
		}

		b += (UnityEngine.Random.value * 2f - 1f) * bDelta;
		b = Mathf.Min(b, maxB);
		b = Mathf.Max(b, minB);
		a += (UnityEngine.Random.value * 2f - 1f) * aDelta;
		a = Mathf.Min(a, maxA);
		a = Mathf.Max(a, minA);
		V1.x += (UnityEngine.Random.value * 2f - 1f) * V1delta;
		V1.y += (UnityEngine.Random.value * 2f - 1f) * V1delta;
		V1.Normalize();

		V2.x += (UnityEngine.Random.value * 2f - 1f) * V2delta;
		V2.y += (UnityEngine.Random.value * 2f - 1f) * V2delta;
		V2.Normalize();

	}

	private void SpawnParticle() {
		var newParticle = Instantiate(prefab, particleFolder);
		newParticle.SetActive(true);
		var propagandaParticle = newParticle.GetComponent<PropagandaParticle>();
		propagandaParticle.startX = transform.position.x + (UnityEngine.Random.value * 2f - 1f) * startVariance;
		propagandaParticle.startY = transform.position.y + (UnityEngine.Random.value * 2f - 1f) * startVariance;
		propagandaParticle.a = a;
		propagandaParticle.C1 = C1 + (UnityEngine.Random.value * 2f - 1f) * C1Variance;
		propagandaParticle.V1 = V1;
		propagandaParticle.b = b;
		propagandaParticle.C2 = C2 + (UnityEngine.Random.value * 2f - 1f) * C2Variance;
		propagandaParticle.V2 = V2;
		propagandaParticle.type = spawnType;

		particleColor.h += hueChangeScale;
		if (particleColor.h > 1f) {
			particleColor.h -= 1f;
		}
		//particleColor.H( Mathf.Sin(hueChangeScale * spawnTimer)*0.5f + 0.5f, ref particleColor);
		propagandaParticle.GetComponent<SpriteRenderer>().color = particleColor.ToColor();

		if (backgroundSprite != null) {
			backgroundSprite.color = Color.HSVToRGB((particleColor.h + 0.3f)%1f, 1f, 1f);
		}
	}
}
