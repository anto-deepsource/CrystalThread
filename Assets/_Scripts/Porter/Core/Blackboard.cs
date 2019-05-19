using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

[RequireComponent(typeof(UnitEssence))]
public class Blackboard : MonoBehaviour {
	//[Tooltip("The layer used for checking whether the character is grounded")]
	//public LayerMask groundLayer;

	//public Faction faction;

	//public Vector3 targetPoint;
	public GameObject target;
	
	//// Health
	//public float regenHealth = 1; // health regenerated per second
	//public float currentHealth = 5;
	//public float maxHealth = 9;
	//public float fullHealth = 10;

	// status effects
	public float staggerTime = -0.1f;

	public float radius = 0.5f;
	//public float height = 2;
	public float moveSpeed = 12;
	public float turnSpeed = 12;
	public float moveForce = 60;
	public float brakeRate = 8.0f;
	public float weight = 50;

	public Vector3 MoveVector {
		get { return essence.MoveVector; }
		set { essence.MoveVector = value; }
	}

	/// <summary>
	/// An arrow representing the direction the body should turn relative to the X-Z plane.
	/// Zero means no target direction.
	/// </summary>
	public Vector2 TurnVector {
		get { return essence.TurnVector; }
		set { essence.TurnVector = value; }
	}

	public bool IsGrounded {
		get { return essence.groundedComponent.IsGrounded; }
	}
	//public bool isDead = false;

	///// <summary>
	///// Stored here as attacks per second.
	///// </summary>
	//public float attackSpeed = 0.6f;

	public QuickEvent events = new QuickEvent();

	//private ObjectQuery query;

	private AnimationMaster aniMaster;

	private UnitEssence essence;
	//private Rigidbody myBody;

	//private Canvas inGameTextCanvas;
	//private GameObject inGameTextPrefab;

	//public void Awake() {
	//	query = new ObjectQuery() {
	//		gameObject = gameObject,
	//		hasBlackboard = true,
	//		isUnit = true,
	//		faction = faction,
	//	};
	//	QueryManager.RegisterObject(query);
	//}

	public void Start() {
		aniMaster = Utilities.AnimationMaster(gameObject);
		//myBody = GetOrCreateRigidBody();
		//myBody.mass = weight;
		essence = gameObject.GetUnitEssence();
		//inGameTextCanvas = GetComponentInChildren<Canvas>();
		//inGameTextPrefab = inGameTextCanvas.transform.Find("InGameTextPrefab").gameObject;
	}

	//public void OnDestroy() {
	//	QueryManager.UnregisterObject(query);
	//}

	public void SetStagger( float newTime ) {
		staggerTime = Mathf.Max(staggerTime, newTime);
		events.Fire(BlackboardEventType.Staggered, this);
	}

	//public void Update() {
	//	if ( essence.IsDead ) {
	//		return;
	//	}

	//	if (staggerTime > 0 && staggerTime - Time.deltaTime <= 0) {
	//		events.Fire(BlackboardEventType.StaggeredEnd, this);
	//	}
	//	staggerTime = Mathf.Max(0, staggerTime - Time.deltaTime);

	//	//currentHealth += regenHealth * Time.deltaTime;
	//	//if ( currentHealth > maxHealth ) {
	//	//	currentHealth = maxHealth;
	//	//}

	//	//isGrounded = Physics.CheckSphere(transform.position, radius * 0.5f, groundLayer);
	//}

	//public void FixedUpdate() {
	//	if (essence.IsDead) {
	//		return;
	//	}

	//	Collider myCollider = GetComponent<Collider>();

	//	if (myCollider.enabled) {
	//		CorporealMovement();
	//	}
	//	else {
	//		IntangibleMovement();
	//	}


	//	HexNavMeshManager.EnsureAboveMap(transform);
	//}

	///// <summary>
	///// Used in tandem with the FootContact to keep track of jump/grounded
	///// </summary>
	///// <param name="collider"></param>
	//public void OnTriggerStay(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		isGrounded = true;
	//	}
	//}

	//public void OnTriggerExit(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		isGrounded = false;
	//	}
	//}

	//private void CorporealMovement() {

	//	//if (isGrounded) {
	//	//	Vector3 movement = moveVector;
	//	//	movement.y = 0.0f;

	//	//	//if (movement.magnitude > 0.01f) {
	//	//	//	//transform.rotation = Quaternion.LookRotation(movement);
	//	//	//} else
	//	//	//if (isGrounded) {
	//	//	//	//if (target != null) {
	//	//	//	//	transform.LookAt(target.transform.position.DropY() + transform.position.y * Vector3.up);
	//	//	//	//}
	//	//	//}

	//	//	Vector3 currentVelocity = myBody.velocity.DropY();

	//	//	if (isGrounded) {
	//	//		Vector3 opposite = -currentVelocity * brakeRate;

	//	//		float dot = Vector3.Dot(currentVelocity.normalized, movement.normalized);
	//	//		if (dot < 0.5f) {
	//	//			myBody.AddForce(opposite * Time.deltaTime, ForceMode.Impulse);
	//	//		} else {

	//	//		}

	//	//	}

	//	//	//else
	//	//	//if (dot > 0) {
	//	//	//	movement = Vector3.Lerp(movement.normalized * moveForce, opposite, dot);
	//	//	//}
	//	//	//else {
	//	//		if (myBody.velocity.magnitude < moveSpeed) {
	//	//			movement = movement.normalized * moveForce;
	//	//		}
	//	//		else {
	//	//			movement = Vector3.zero;
	//	//		}
	//	//	//}


	//	//	if (moveVector.magnitude < 0.001f && isGrounded) {
	//	//		movement = -currentVelocity * brakeRate;
	//	//	}
	//	//	if (!isGrounded)
	//	//		movement.y -= weight;

	//	//	myBody.AddForce(movement * Time.deltaTime, ForceMode.Impulse);

	//	//	//Vector3 x = Vector3.Cross(oldPoint.normalized, newPoint.normalized);
	//	//	//float theta = Mathf.Asin(x.magnitude);
	//	//	//Vector3 w = x.normalized * theta / Time.fixedDeltaTime;

	//	//	//Quaternion q = transform.rotation * myBody.inertiaTensorRotation;
	//	//	//Vector3 T = q * Vector3.Scale(myBody.inertiaTensor, (Quaternion.Inverse(q) * w));

	//	//	//myBody.AddTorque(T, ForceMode.Impulse)

	//	//	if ( turnVector.sqrMagnitude >0) {
	//	//		float turnForce = CommonUtils.AngleBetweenVectors(turnVector, transform.forward.JustXZ());

	//	//		myBody.angularVelocity = new Vector3(0, turnForce * turnSpeed, 0);
	//	//	} else {
	//	//		myBody.angularVelocity = new Vector3(0, 0, 0);
	//	//	}


	//	//	//myBody.angularVelocity = Vector3.zero;
	//	//}

	//	//Vector3 movement = moveVector;
	//	//movement.y = 0.0f;

	//	//movement = movement * moveSpeed;

	//	//myBody.MovePosition(transform.position + movement * Time.deltaTime);
	//	//myBody.angularVelocity = Vector3.zero;
	//	myBody.velocity = moveVector * moveSpeed + myBody.velocity.JustY();

	//	//RaycastHit aimRayHit;
	//	//if (Physics.Raycast(transform.position + Vector3.up*3f, Vector3.down, out aimRayHit, 100, groundLayer)) {

	//	//	myBody.MovePosition(new Vector3(myBody.position.x, aimRayHit.point.y, myBody.position.z));
	//	//}

	//	if (turnVector.sqrMagnitude > 0) {
	//		float turnForce = CommonUtils.AngleBetweenVectors(turnVector, transform.forward.JustXZ());
	//		Vector3 rotation = myBody.rotation.eulerAngles;
	//		rotation.y += turnForce * turnSpeed;
	//		myBody.rotation = Quaternion.Euler(rotation);
	//	//	myBody.angularVelocity = new Vector3(0, turnForce * turnSpeed, 0);
	//	//}
	//	//else {
	//	//	myBody.angularVelocity = new Vector3(0, 0, 0);
	//	}

	//}

	//private void IntangibleMovement() {
	//	Vector3 movement = moveVector;
	//	movement.y = 0.0f;

	//	movement = movement.normalized * moveSpeed;

	//	//var position = HexNavMeshManager.WorldPositionPlusMapHeight(transform.position);

	//	myBody.MovePosition(transform.position + movement * Time.deltaTime);
	//	myBody.angularVelocity = Vector3.zero;
	//	myBody.velocity = Vector3.zero;

	//	//HexNavMeshManager.EnsureAboveMap(transform);
	//}

	public void Play(AnimationKey key, AnimationData data) {
		aniMaster.Play(key, data);
	}

	private Rigidbody GetOrCreateRigidBody() {
		Rigidbody myBody = GetComponent<Rigidbody>();

		if (myBody == null) {
			myBody = gameObject.AddComponent<Rigidbody>();
		}

		//myBody.isKinematic = true;

		myBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		//myBody.angularDrag = .9f;
		//myBody.drag = .9f;
		return myBody;
	}

	//public void PlayBasicAttack( params AnimationKeys.Mod[] mods) {
	//    aniMaster.Play(AnimationKeys.Key.Attack, 1f/attackSpeed, mods);
	//}

	//   /// <summary>
	//   /// Used to apply damage to the blackboard. Events are automatically fired for the blackboard.
	//   /// </summary>
	//   /// <param name="source"></param>
	//   public override void ApplyDamage(DamageSource source) {
	//	if (!isDead) {
	//		events.Fire(BlackboardEventType.Damaged, this);
	//		Play(AnimationKeys.Key.Damaged);

	//		currentHealth -= source.amount;
	//		maxHealth -= source.deepAmount;

	//		GameObject newFloatingTextObject = Instantiate(inGameTextPrefab, inGameTextCanvas.transform);
	//		FloatingText newFloatingText = FloatingText.NewDamageText(newFloatingTextObject, (int)source.amount);


	//		if (currentHealth <= 0) {
	//			ApplyDeath();
	//		}
	//	}


	//	//Vector3 point = transform.InverseTransformPoint(source.hitPoint);
	//	Vector3 point = source.hitPoint;
	//	myBody.AddForceAtPosition(source.pushBack, point, ForceMode.Impulse);
	//	//myBody.AddTorque(Vector3.forward*100);


	//}

	//public void ApplyDeath() {
	//	events.Fire(BlackboardEventType.Death, this);
	//	Play(AnimationKeys.Key.Death);
	//	isDead = true;
	//}

	/// <summary>
	/// Simple way of firing events on the blackboard's listeners
	/// </summary>
	public void Inform(BlackboardEventType eventType, object data ) {
		events.Fire(eventType, data);
	}
	
}
public enum BlackboardEventType {
	Staggered			= 1 << 1,
	StaggeredEnd		= 1 << 2,
	Damaged				= 1 << 3,
	ResourcesAdded		= 1 << 4,
	ResourcesRemoved	= 1 << 5,
	Death				= 1 << 6,
	Incapacitated		= 1 << 7,
	PositiveUnitInteraction		= 1 << 8,
	NeutralUnitInteraction		= 1 << 9,
	NegativeUnitInteraction		= 1 << 10,
	AlertToDangerInteraction		= 1 << 11,
	EnemySpotted		= 1 << 12,
	Healed		= 1 << 13,
	GameobjectDestroyed		= 1 << 14,
	Capacitated = 1 << 15,
	Grappling = 1 << 16,
	StopGrappling = 1 << 17,
	PickUpFromGround = 1 << 18,
	MoveIntoMissionary = 1 << 19,
	ManualFireAnimationTrigger = 1 << 20,
}
