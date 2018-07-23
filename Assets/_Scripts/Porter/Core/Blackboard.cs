using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour {
	 

	public Vector3 targetPoint;
	public GameObject target;

	public GameObject[] affected;

	public int team = 0;

	//public Dictionary<StateTransition, bool> triggers = new Dictionary<StateTransition, bool>();

	// Health
	public float regenHealth = 1; // health regenerated per second
	public float currentHealth = 5;
	public float maxHealth = 9;
	public float fullHealth = 10;

	// status effects
	public float staggerTime = -0.1f;
	public bool lifted = false;

	public float moveSpeed = 6;
	public float weight = 50;

	/// <summary>
	/// Stored here as attacks per second.
	/// </summary>
	public float attackSpeed = 0.6f;

	public QuickEvent events = new QuickEvent();

	private ObjectQuery query;

	private AnimationMaster aniMaster;

	public void Awake() {
		query = new ObjectQuery() {
			gameObject = gameObject,
			hasBlackboard = true,
			team = team,
		};
		QueryManager.RegisterObject(query);
	}

	public void Start() {
		aniMaster = Utilities.AnimationMaster(gameObject);
	}

	public void OnDestroy() {
		QueryManager.UnregisterObject(query);
	}

	public void SetStagger( float newTime ) {
		staggerTime = Mathf.Max(staggerTime, newTime);
		events.Fire(BlackboardEventType.Staggered, this);
	}

	public void Update() {
		if (staggerTime > 0 && staggerTime - Time.deltaTime <= 0) {
			events.Fire(BlackboardEventType.StaggeredEnd, this);
		}
		staggerTime = Mathf.Max(0, staggerTime - Time.deltaTime);

		currentHealth += regenHealth * Time.deltaTime;
		if ( currentHealth > maxHealth ) {
			currentHealth = maxHealth;
		}
	}

	public void Play(AnimationKeys.Key key, float playLength = -1, params AnimationKeys.Mod[] mods) {
		aniMaster.Play(key, playLength, mods);
	}
}
public enum BlackboardEventType {
	Staggered = 2,
	StaggeredEnd = 4,

}