using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	public Blackboard blackboard;

	private Slider current;
	private Slider max;

	void Start() {
		current = transform.Find("Current").GetComponent<Slider>();
		max = transform.Find("Max").GetComponent<Slider>();
	}
	
	void Update () {
		current.value = blackboard.currentHealth / blackboard.fullHealth;
		max.value = blackboard.maxHealth / blackboard.fullHealth;
	}
}
