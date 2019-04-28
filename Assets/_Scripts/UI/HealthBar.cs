using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	public HealthComponent healthComponent;

	private Slider current;
	private Slider max;

	private float deathAnimation = 1;

	void Start() {
		if ( healthComponent==null) {
			healthComponent = gameObject.GetHealthComponent();
		}
		
		current = transform.Find("Current").GetComponent<Slider>();
		max = transform.Find("Max").GetComponent<Slider>();
	}
	
	void Update () {
		float m = healthComponent.maxHealth;
		if ( healthComponent.IsDead ) {
			deathAnimation -= Time.deltaTime;
			deathAnimation = Mathf.Max(0, deathAnimation);
			m = Ease.EaseOut(healthComponent.maxHealth, 0, 1f-deathAnimation);
		} else {
			deathAnimation = 1;
		}
		current.value = healthComponent.currentHealth / healthComponent.fullHealth;
		max.value = m / healthComponent.fullHealth;
	}
}
