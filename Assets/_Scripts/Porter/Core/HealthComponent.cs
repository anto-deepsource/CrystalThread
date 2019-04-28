using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : Damagable {

	public float regenHealth = 1; // health regenerated per second
	public float currentHealth = 5;
	public float maxHealth = 9;
	public float fullHealth = 10;

	public bool destroyOnDeath = false;

	[Header("Collision Damage")]
	public bool useCollisionDamage = true;
	public float minCollisionVelocity = 20f;
	public float collisionVelocityToDamageConstance = .05f;

	[Header("UI Display")]
	public bool useInGameText = false;
	//public Slider currentSlider;
	//public Slider maxSlider;

	#region Properties

	public bool IsDead { get; private set; }

	public QuickEvent Events { get { return _events; } }
	private QuickEvent _events = new QuickEvent();

	#endregion

	#region Private Members

	private Canvas inGameTextCanvas;
	private GameObject inGameTextPrefab;

	#endregion

	#region Monobehavior Methods

	public void Awake() {
		if (useInGameText) {
			// TODO: use the HUD canvas instead of the in-world canvas like a unit
			inGameTextCanvas = GetComponentInChildren<Canvas>(true);
			inGameTextPrefab = inGameTextCanvas.transform.Find("InGameTextPrefab").gameObject;
		}

		IsDead = false;
		
	}

	public void Update() {

		UpdateHealth();
		//UpdateUI();
	}

	#endregion

	#region Private Methods
	private void UpdateHealth() {
		if (!IsDead) {
			currentHealth += regenHealth * Time.deltaTime;
			if (currentHealth > maxHealth) {
				currentHealth = maxHealth;
			}
			if (currentHealth <= 0) {
				ApplyDeath();
			}
		}
	}

	//private void UpdateUI() {
	//	if (currentSlider != null) {
	//		currentSlider.value = currentHealth / fullHealth;
	//	}
	//	if (maxSlider != null) {
	//		maxSlider.value = maxHealth / fullHealth;
	//	}
	//}

	#endregion

	private void OnCollisionEnter(Collision collision) {
		if ( useCollisionDamage) {
			Rigidbody theirBody = collision.collider.GetComponent<Rigidbody>();
			if (collision.relativeVelocity.sqrMagnitude > minCollisionVelocity * minCollisionVelocity) {
				DamageSource collisionDamage = new DamageSource() {
					sourceObject = collision.gameObject,
					amount = (collision.relativeVelocity.magnitude - minCollisionVelocity)
						* collisionVelocityToDamageConstance,
					deepAmount = 0,
				};
				ApplyDamage(collisionDamage);
			}
		}
		
	}

	/// <summary>
	/// Used to apply damage to the blackboard. Events are fired.
	/// </summary>
	/// <param name="source"></param>
	public override void ApplyDamage(DamageSource source) {
		if (IsDead) {
			return;
		}
		Events.Fire(BlackboardEventType.Damaged, source);

		currentHealth -= source.amount;
		maxHealth -= source.deepAmount;
		
		if (useInGameText) {
			GameObject newFloatingTextObject = Instantiate(inGameTextPrefab, inGameTextCanvas.transform);
			FloatingText newFloatingText = FloatingText.NewDamageText(newFloatingTextObject, (int)source.amount);
		}

		if (currentHealth <= 0) {
			ApplyDeath();
		}
	}

	public void ApplyDeath() {
		Events.Fire(BlackboardEventType.Death, this);
		IsDead = true;

		if (destroyOnDeath) {
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Similar to ApplyDamage() but for positive sources
	/// </summary>
	/// <param name="source"></param>
	public void ApplyHealth(DamageSource source) {
		if (IsDead) {
			return;
		}
		Events.Fire(BlackboardEventType.Healed, source);

		currentHealth += source.amount;
		maxHealth += source.deepAmount;

		if (useInGameText) {
			GameObject newFloatingTextObject = Instantiate(inGameTextPrefab, inGameTextCanvas.transform);
			FloatingText newFloatingText = FloatingText.NewHealText(newFloatingTextObject, (int)source.amount);
		}

		if (currentHealth <= 0) {
			ApplyDeath();
		}
	}
}
