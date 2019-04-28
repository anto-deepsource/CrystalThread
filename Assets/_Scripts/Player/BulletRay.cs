using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRay : MonoBehaviour {

	public AudioClip fireSound;

	public float fireRate = 0.1f;

	public RectTransform retical;

	public LayerMask hitLayer;

	public float trailLifeTime = 0.01f;

	public Transform weaponTip;

	public GameObject bloodSprayPrefab;

	private Vector3 startRotation;
	Animator gunAnimator;

	LineRenderer lineRenderer;

	private float fireDelay = 0;
	private float trailLifeDelay = 0;

	private AudioSource audioSource;

	// Use this for initialization
	void Start () {
		gunAnimator = GetComponent<Animator>();
		lineRenderer = GetComponent<LineRenderer>();

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0.1f;

		startRotation = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update () {
		AimWeapon();

		if (trailLifeDelay > 0 && trailLifeDelay - Time.deltaTime < 0) {
			lineRenderer.positionCount = 0;
		}
		trailLifeDelay -= Time.deltaTime;

		fireDelay -= Time.deltaTime;
		if (fireDelay < 0 && Input.GetMouseButton(1)) {
			FireBullet();
		}
		
	}

	RaycastHit aimRayHit;
	private bool aimingHit = false;

	

	/// <summary>
	/// Perform the 'aiming' raycast.
	/// Done each frame to rotate the weapon
	/// </summary>
	private void AimWeapon() {
		// do one raycast from the ratical point on the screen out in the direction we're facing
		Vector2 reticalScreenPos = retical.position;

		Ray camRay = Camera.main.ScreenPointToRay(reticalScreenPos);

		Vector3 camThisVector = transform.position - camRay.origin;
		Vector3 projection = CommonUtils.Projection(camThisVector, camRay.direction);
		camRay.origin += projection;
		
		aimingHit = Physics.Raycast(camRay, out aimRayHit, 100, hitLayer);

		if ( aimingHit ) {
			transform.LookAt(aimRayHit.point);
			transform.Rotate(startRotation);
		}
	}

	private void FireBullet() {
		gunAnimator.SetTrigger("Fire");
		fireDelay = fireRate;
		//audioSource.clip = fireSound;
		audioSource.PlayOneShot(fireSound);
		
		if (aimingHit) {

			// the aiming retical hit something
			// do a second raycast from the tip of the weapon to the aim point to 
			// determine what actually gets hit

			RaycastHit hitRayHit;
			if (Physics.Raycast( weaponTip.position, aimRayHit.point-weaponTip.position, 
				out hitRayHit, 100, hitLayer)) {
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(0, weaponTip.position);
				lineRenderer.SetPosition(1, hitRayHit.point);
				trailLifeDelay = trailLifeTime;
				ApplyDamage(hitRayHit.collider.gameObject, hitRayHit);
			} else {
				// we should have at least hit the place as the aim point
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(0, weaponTip.position);
				lineRenderer.SetPosition(1, aimRayHit.point);
				trailLifeDelay = trailLifeTime;
			}

			
		} else {
			// our aiming raycast didn't hit anything
			lineRenderer.positionCount = 0;
		}
	}

	private void ApplyDamage( GameObject target, RaycastHit hit ) {
		HealthComponent theirHealth = target.GetHealthComponent();
		if (theirHealth != null) {
			DamageSource myDamage = new DamageSource() {
				isPlayer = true,
				sourceObject = gameObject,
				amount = 5 + Random.value * 3,
				deepAmount = 1 + Random.value * 1,
				pushBack = 2f * (hit.point - weaponTip.position).normalized,
				//myDamage.hitPoint = target.transform.InverseTransformPoint( hit.point),
				hitPoint = hit.point,
			};
			theirHealth.ApplyDamage(myDamage);

			GameObject newBlood = GameObject.Instantiate(bloodSprayPrefab, target.transform);
			newBlood.transform.position = hit.point;
			newBlood.transform.forward = hit.normal;
			Destroy(newBlood, 10f);
		} else {

			Rigidbody theirBody = target.GetComponent<Rigidbody>();

			if (theirBody!=null && !theirBody.isKinematic &&
					theirBody.constraints == RigidbodyConstraints.None ) {
				theirBody.AddForceAtPosition(2f * (hit.point - weaponTip.position).normalized,
					hit.point, ForceMode.Impulse);
			}
		}
		

	}
}
