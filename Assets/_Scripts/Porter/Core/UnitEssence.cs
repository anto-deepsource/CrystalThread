using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an actor in the game that behaves like a unit or creature.
/// Handles faction.
/// Handles health, being damaged, stunned, and immobilized.
/// Handles animations.
/// Fires events that other systems can subscribe to.
/// </summary>
public class UnitEssence : MonoBehaviour {

	[Header("Basic Info")]
	public Faction faction;

	//[Header("Health")]
	//public float regenHealth = 1; // health regenerated per second
	//public float currentHealth = 5;
	//public float maxHealth = 9;
	//public float fullHealth = 10;

	[Header("Physical Properties")]
	public float radius = 0.5f;
	public float height = 2;

	[Header("Movement")]
	[Tooltip("The layer used for checking whether the character is grounded")]
	public LayerMask groundLayer;
	public float moveSpeed = 12;
	public float turnSpeed = 12;
	public float brakeForce = 2f;

	//[Header("Collision Damage")]
	//public float minCollisionVelocity = 20f;
	//public float collisionVelocityToDamageConstance = .05f;

	//[Header("UI Display")]
	//public Slider currentSlider;
	//public Slider maxSlider;
	//public bool useInGameText = false;

	//private ObjectQuery query;

	public float waterHeight = 0;
	public float waterResistance = 0.6f;
	public float boyancy = 0.6f;

	public Carryable currentlyCarriedObject;

	public Grounded groundedComponent;

	public Stack<MonoBehaviour> currentControllers = new Stack<MonoBehaviour>();

	private Vector3 pendingForces = Vector3.zero;

	#region Properties
	
	public Vector3 MoveVector { get; set; }
	
	/// <summary>
	/// An arrow representing the direction the body should turn relative to the X-Z plane.
	/// Zero means no target direction.
	/// </summary>
	public Vector2 TurnVector { get; set; }

	public bool IsDead { get { return healthComponent.IsDead; } }

	/// <summary>
	/// A status effect that causes the unit to be unable to move themselves.
	/// A unit remains incapacitated until specifically capacitated.
	/// </summary>
	public bool IsIncapacitated { get; private set; }

	/// <summary>
	/// A status effect that causes the unit to be unable to move themselves.
	/// A unit is automatically made off-balance if they:
	/// -Take damage while in air
	/// -become Incapacitated
	/// A unit automatically regains their balance after contact with the ground.
	/// A unit takes damage if they are off-balance and collide with the ground (damage is based on force).
	/// </summary>
	public bool IsOffBalance { get; private set; }

	//public bool IsGrounded { get; private set; }

	public bool IsRunning { get; set; }

	public bool IsTurning { get; set; }

	public bool IsSwimming { get; set; }

	public PostureState Posture { get; set; }

	public Vector3 CurrentVelocity { get { return myBody.velocity; } }

	public float CurrentTurnSpeed { get; private set; }

	public QuickEvent Events { get { return _events; } }
	private QuickEvent _events = new QuickEvent();

	#endregion
	
	#region Private Component References

	private AnimationMaster aniMaster;
	public Rigidbody myBody;
	//private Canvas inGameTextCanvas;
	//private GameObject inGameTextPrefab;
	public Collider myCollider;

	public HealthComponent healthComponent;

	#endregion



	public void Awake() {

		aniMaster = Utilities.AnimationMaster(gameObject);
		if (myBody == null) {
			myBody = GetOrCreateRigidBody();
		}
		
		if (myCollider == null) {
			myCollider = GetComponent<Collider>();
		}
		

		if (healthComponent == null) {
			healthComponent = GetComponent<HealthComponent>();
		}
		healthComponent.Events.Add(this, HealthEvents, BlackboardEventType.Damaged, BlackboardEventType.Death);

		//if (useInGameText) {
		//	// TODO: use the HUD canvas instead of the in-world canvas like a unit
		//	inGameTextCanvas = GetComponentInChildren<Canvas>(true);
		//	inGameTextPrefab = inGameTextCanvas.transform.Find("InGameTextPrefab").gameObject;
		//}

		//IsDead = false;
		IsIncapacitated = false;

	}
	
	public void FixedUpdate() {
		if (healthComponent.IsDead) {
			return;
		}

		if (pendingForces!=Vector3.zero) {
			myBody.AddForce(pendingForces, ForceMode.Impulse);

			pendingForces = Vector3.zero;
		}


		if (myCollider.enabled) {
			CorporealMovement();
		}
		else {
			IntangibleMovement();
		}


		HexNavMeshManager.EnsureAboveMap(transform);
	}

	private void CorporealMovement() {

		if (IsIncapacitated || IsOffBalance ) {
			IsRunning = false;
			if ( !IsIncapacitated && IsOffBalance && groundedComponent.IsGrounded) {
				bool canCatchBalance = myBody.velocity.sqrMagnitude < moveSpeed * moveSpeed;
				if ( canCatchBalance ) {
					IsOffBalance = false;
				}
			}
		}
		else if (IsSwimming) {
			Vector3 newVelocity = MoveVector * moveSpeed;
			myBody.velocity = MoveVector * moveSpeed + myBody.velocity.JustY();
			myBody.AddForce(-Physics.gravity, ForceMode.Acceleration);

			float changeInY = -myBody.velocity.y * waterResistance;
			changeInY += (waterHeight - transform.position.y) * boyancy;
			//myBody.AddForce(Vector3.up * (waterHeight - transform.position.y) * boyancy, ForceMode.Impulse);
			myBody.velocity = myBody.velocity + Vector3.up * changeInY;
			//myBody.velocity = myBody.velocity + Vector3.up * (waterHeight - transform.position.y) * boyancy;
		}
		else if (!groundedComponent.IsGrounded) {
			IsRunning = false;
		}
		else {
			//Vector3 newVelocity = MoveVector * moveSpeed;

			//IsRunning = newVelocity.sqrMagnitude > 0.001f;

			//if (IsRunning) {
			//	myBody.velocity = MoveVector * moveSpeed + myBody.velocity.JustY();
			//} else {
			//	myBody.velocity = myBody.velocity.JustY();
			//}

			var moveVector = MoveVector.DropY().normalized;

			if (moveVector.sqrMagnitude > 0.01f) {
				IsRunning = true;
				var idealVelocity = moveVector * moveSpeed;
				var currentVelocity = myBody.velocity;
				var idealDifference = idealVelocity - currentVelocity;
				myBody.AddForce(idealDifference);
			}
			else {
				IsRunning = false;
				var currentVelocity = myBody.velocity;
				var idealDifference = -currentVelocity;
				myBody.AddForce(idealDifference * brakeForce);
			}

			if (TurnVector.sqrMagnitude > 0) {
				TurnToDirection(TurnVector);
				//Vector3 forward = Camera.main.transform.forward.DropY();
				//transform.rotation = Quaternion.LookRotation(forward);
			}
			else
			if (moveVector.sqrMagnitude > 0.01f) {
				TurnToDirection(moveVector.JustXZ());

				//transform.rotation = Quaternion.LookRotation(myBody.velocity.DropY());
			}

			var currentTorque = myBody.angularVelocity;
			//Debug.Log(currentTorque);
			myBody.AddTorque(0, -currentTorque.y * 0.6f, 0);

			//myBody.angularVelocity = Vector3.zero;

			//if (TurnVector.sqrMagnitude > 0) {
			//	//float difference = CommonUtils.AngleBetweenVectors(transform.forward.JustXZ(), TurnVector);

			//	Vector3 rotation = myBody.rotation.eulerAngles;
			//	float targetTheta = Mathf.Atan2(TurnVector.y, TurnVector.x);
			//	//float targetTheta = targetRotation % 360f * Mathf.Deg2Rad;
			//	//float currentTheta = rotation.y * Mathf.Deg2Rad;
			//	var currentVector = transform.forward.JustXZ().normalized;
			//	float currentTheta = Mathf.Atan2(currentVector.y, currentVector.x);
			//	float difference = CommonUtils.AngleBetweenThetas(currentTheta, targetTheta);
			//	float epsilon = 0.1f;
			//	if (difference < -epsilon) {
			//		CurrentTurnSpeed = -Mathf.Max(-turnSpeed, difference);
			//		IsTurning = true;
			//	} else if (difference > epsilon) {
			//		CurrentTurnSpeed = -Mathf.Min(turnSpeed, difference);
			//		IsTurning = true;
			//	} else {
			//		CurrentTurnSpeed = 0;
			//		IsTurning = false;
			//	}
			//	rotation.y += CurrentTurnSpeed;
			//	//rotation.y += turnForce * turnSpeed;
			//	myBody.rotation = Quaternion.Euler(rotation);
			//}
		}
	}

	public void TurnToDirection(Vector2 xzVector) {
		float targetTheta = Mathf.Atan2(xzVector.y, xzVector.x);
		var currentVector = transform.forward.JustXZ().normalized;
		float currentTheta = Mathf.Atan2(currentVector.y, currentVector.x);
		float difference = CommonUtils.AngleBetweenThetas(targetTheta, currentTheta);

		myBody.AddTorque(0, difference, 0);
	}

	private void IntangibleMovement() {
		Vector3 movement = MoveVector;
		movement.y = 0.0f;

		movement = movement.normalized * moveSpeed;
		
		myBody.MovePosition(transform.position + movement * Time.deltaTime);
		myBody.angularVelocity = Vector3.zero;
		myBody.velocity = Vector3.zero;
		
	}

	//private void OnCollisionStay(Collision collision) {
	//	if ( IsGrounded && collision.gameObject == groundCollidingObject ) {
	//		Debug.Log("OnCollisionStay: " + collision.contacts[0].normal );
	//	}
	//}

	//GameObject groundCollidingObject;

	///// <summary>
	///// Used in tandem with the FootContact to keep track of jump/grounded
	///// </summary>
	///// <param name="collider"></param>
	//public void OnTriggerStay(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		IsGrounded = true;
	//		//groundCollidingObject = collider.gameObject;
	//		bool canCatchBalance = myBody.velocity.sqrMagnitude < moveSpeed * moveSpeed;
	//		if (!IsIncapacitated && canCatchBalance) {
	//			IsOffBalance = false;
	//		}
	//	}
	//}

	//public void OnTriggerExit(Collider collider) {
	//	if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
	//		IsGrounded = false;
			
	//	}
	//}

	private void OnDestroy() {
		Events.Fire(BlackboardEventType.GameobjectDestroyed, this);
	}

	private void HealthEvents(int eventCode, object data) {

		// pass the event up
		Events.Fire(eventCode, data);

		switch ((BlackboardEventType)eventCode) {
			case BlackboardEventType.Damaged:
				Play(AnimationKey.Damaged, AnimationData.none);
				// apply knockback
				DamageSource source = (DamageSource)data;
				Vector3 point = source.hitPoint;
				myBody.AddForceAtPosition(source.pushBack, point, ForceMode.Impulse);
				break;
			case BlackboardEventType.Death:
				Play(AnimationKey.Death, AnimationData.none);

				StopCarrying();
				break;
		}
	}

	/// <summary>
	/// Sets this unit to 'incapacitated' which makes them unable to move themselves.
	/// </summary>
	public void Incapacitate() {
		IsIncapacitated = true;
		IsOffBalance = true;
		Events.Fire(BlackboardEventType.Incapacitated, this);
	}

	/// <summary>
	/// Sets this unit to not incapacitated, meaning they can move themselves again.
	/// </summary>
	public void Capacitate() {
		IsIncapacitated = false;
		Events.Fire(BlackboardEventType.Capacitated, this);
	}

	public void ApplyDamage(DamageSource source) {
		if (healthComponent.IsDead) {
			return;
		}
		healthComponent.ApplyDamage(source);
	}

	public void ApplyForce(Vector3 force, ForceMode mode = ForceMode.Impulse) {
		//pendingForces += force;
		myBody.AddForce(force * Time.deltaTime, mode);
	}

	public void ApplyBrakeForce(float rate = 1f) {
		//pendingForces += -myBody.velocity * rate;
		myBody.AddForce(-myBody.velocity * Time.deltaTime * rate, ForceMode.Impulse);
	}

	public Vector3 GetCurrentVelocity() {
		return myBody.velocity;
	}

	public void SetVerticalVelocity(float y ) {
		myBody.velocity = new Vector3(myBody.velocity.x, y, myBody.velocity.z);
	}

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
		
		return myBody;
	}

	public void StartCarrying(Carryable carryable) {
		if (currentlyCarriedObject != null) {
			StopCarrying();
		}
		currentlyCarriedObject = carryable;
		carryable.SetBeingCarried(this);
		var theirEssence = carryable.gameObject.GetUnitEssence();
		if (theirEssence != null) {
			theirEssence.Incapacitate();
		}

	}

	public void StopCarrying() {
		if (currentlyCarriedObject != null) {
			currentlyCarriedObject.SetNotBeingCarried();
			var theirEssence = currentlyCarriedObject.gameObject.GetUnitEssence();
			if (theirEssence != null) {
				theirEssence.Capacitate();
			}
			currentlyCarriedObject = null;
		}
	}

	public void Inform(BlackboardEventType eventType, object data) {
		Events.Fire(eventType, data);
		Debug.Log(eventType);
	}

	public void StartGrappling( Grappable grappable ) {
		Events.Fire(BlackboardEventType.Grappling, null);
	}

	public void StopGrappling(Grappable grappable) {
		Events.Fire(BlackboardEventType.StopGrappling, null);
	}

	public void PickUpEqippableWeapon(EquippableWeapon nearestWeapon) {
		Events.Fire(BlackboardEventType.PickUpFromGround, null);
	}

	public void OverrideControl(MonoBehaviour overridingController) {
		currentControllers.Push(overridingController);
	}

	public void RelinquishControl(MonoBehaviour controller) {
		if (ControllerIsInControl(controller)) {
			currentControllers.Pop();
		} else {
			// the given controller is not currently in control,
			// but we want to remove it from the stack still
			if ( currentControllers.Contains(controller) ) {
				Debug.Log("Given controller not currenty in control: " +controller.name);
			}
		}
	}

	public bool ControllerIsInControl(MonoBehaviour controller) {
		if (currentControllers.Count == 0) {
			return false;
		}
		return currentControllers.Peek() == controller;
	}
}
public enum PostureState {
	Idle,
	Alert,
	Engaging,
	Sleeping,

}