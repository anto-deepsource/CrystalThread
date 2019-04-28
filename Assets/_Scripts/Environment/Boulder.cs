using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a dynamic rigid body certain properties:
/// -Area of effect damage on high velocity impacts
/// -Collision sounds on certain conditions
/// </summary>
public class Boulder : MonoBehaviour {

	public float corporealRange = 200f;

	public float minImpactForce = 18f;

	public LayerMask groundLayer;

	public AudioClip[] rockCollisionSounds;

	public GameObject collisionParticleEffect;

	public float minTimeBetweenSFX = 0.3f;

	private Rigidbody myBody;

	private AudioSource audioSource;

	private bool corporeal = true;

	private float soundDelay = 0;
	void Start () {
		myBody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (soundDelay > 0 ) {
			soundDelay -= Time.deltaTime;
		}

		if ( corporealRange > 0 ) {
			var targetObject = HexNavMeshManager.TargetObject;
			float d = CommonUtils.DistanceSquared(transform.position, targetObject.transform.position);

			if ( corporeal ) {
				if ( d > corporealRange * corporealRange + 1.0f ) {
					SetIntangible();
				}
			} else {
				if (d < corporealRange * corporealRange - 1.0f) {
					SetCorporeal();
				}
			}
		}
	}

	private void SetCorporeal() {
		//Debug.Log("SetCorporeal");
		corporeal = true;
		myBody.constraints = RigidbodyConstraints.None;
		GetComponent<Collider>().enabled = true;
	}

	private void SetIntangible() {
		//Debug.Log("SetIntangible");
		corporeal = false;
		myBody.constraints = RigidbodyConstraints.FreezeAll;
		GetComponent<Collider>().enabled = false;
	}

	public void OnCollisionEnter(Collision collision) {
		if (soundDelay <= 0 && CommonUtils.IsOnLayer(collision.gameObject, groundLayer)) {
			
			float force = collision.relativeVelocity.magnitude;
			if (force > minImpactForce ) {
				PlayCollisionSoundMaybe();
				CreateCollisionParticleEffect(collision.impulse);
			}
		}
	}

	private void PlayCollisionSoundMaybe() {
		var sound = CommonUtils.RandomChoice<AudioClip>(rockCollisionSounds);
		//var sound = rockCollisionSounds[0];
		audioSource.PlayOneShot(sound);
		soundDelay = minTimeBetweenSFX;
	}

	private void CreateCollisionParticleEffect(Vector3 normal) {
		var newEffect = Instantiate(collisionParticleEffect);
		newEffect.transform.position = transform.position;
		newEffect.transform.up = normal;

		Destroy(newEffect, 4f);
	}
}
