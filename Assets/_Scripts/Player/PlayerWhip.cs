using Grabbables;
using HexMap;
using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerWhip : MonoBehaviour {

	public float damage = 3;

	public float pushBackForce = 10;

	public LayerMask hitLayer;

	//public Transform endObject;
	//public Transform crossHairs;
	public Transform whipEndObject;
	public Transform targetRetical;
	public Transform targetReticalEnemies;
	public Transform hitBox;

	public HitWorldCrossHairs crossHairs;

	public Transform grabPosition;
	public Transform whipHandlePosition;
	public float grabDistance = 2;
	public float grabForce = 12;
	public float brakeForce = 10;

	public float jabSpeed = 12f;

	/// <summary>
	/// The amount of time, in secs, to wait after recieiving a press down event to wait before
	/// deciding that it is a long press/hold, not a short press.
	/// </summary>
	public float longPressPause = 0.2f;

	//public Transform leftIdlePoint;
	//public Transform rightIdlePoint;

	public float camAngleOffset = 20;

	public float resourceableDepositDistance = 3f;
	public Transform depositResourcesIndicator;

	[Header("Grappling Hook")]
	public float grapplingHookForce = 10f;
	public float grapplingHookForceMax = 30f;
	public float grapplingHookDampening = 1f;

	public ExtraEvent<PlayerWhipEvent> Events {
		get {
			if (_events == null) {
				_events = new ExtraEvent<PlayerWhipEvent>(this);
			}
			return _events;
		}
	}
	ExtraEvent<PlayerWhipEvent> _events;

	private GameObject closestReceptical;

	//[Tooltip("The time, in seconds, that a press must be held before it turns into a Long press.")]
	//public float pressTime = 0.2f;

	//[Tooltip("The time, in seconds, between presses before they are no longer a combo.")]
	//public float pressPauseTime = 0.8f;

	//public float whipEndDelay = 0.3f;
	//public float whipLength = 4;

	private LineRenderer lineRenderer;
	
	/// <summary>
	/// The end/tail point of the whip. Moves in space relative
	/// to the player and is used to calculate collisions.
	/// </summary>
	private Vector3 whipEnd;

	private Vector3 whipTarget;

	private List<UnitEssence> currentTargets = new List<UnitEssence>();
	//private List<HarvestableTree> currentAvailableTrees = new List<HarvestableTree>();

	private UnitEssence currentGrabbedTarget;

	private Rigidbody currentGrabbedBody;
	private Vector3 currentGrabbedPointOffset;

	private PolyShape hitBoxShape;

	//private float whipEndDelayTimer = 0;

	private DamageSource myDamage;

	private UnitEssence myEssence;

	private bool doingPress = false;
	private float longPressTimer = 0.0f;

	private bool doingSpin = false;


	private bool doingGrapplingHook = false;
	private Vector3 grapplingHookPoint;
	private Vector3 currentGrapplingPointOffset;
	private Rigidbody currentGrapplingBody;

	///// <summary>
	///// A list that is usually empty that stores any recent presses in the order they were received.
	///// </summary>
	//private List<PressType> recentPresses = new List<PressType>();

	//private float pressTimer = 0f;
	//private bool possibleLongPress = false;

	//private PressType currentPress = PressType.None;

	void Start () {

		lineRenderer = GetComponent<LineRenderer>();
		hitBoxShape = hitBox.GetComponent<PolyShape>();
		myDamage = new DamageSource() {
			isPlayer = true,
			sourceObject = gameObject
		};
		myEssence = gameObject.GetUnitEssence();
	}
	
	void Update () {
		UpdateTargetedUnit();
		UpdatePress();
		UpdateWhipEnd();
		
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, whipHandlePosition.position);
		lineRenderer.SetPosition(1, whipEnd );
		
	}

	private void UpdateTargetedUnit() {
		Vector3 camDir = Camera.main.transform.localRotation.eulerAngles;

		hitBox.localRotation = Quaternion.identity;
		hitBox.RotateAround(hitBox.position, transform.right, camDir.x - 360 - camAngleOffset);

		//endObject.gameObject.SetActive(false);
		//SetCurrentTarget(null);
		currentTargets.Clear();
		//currentAvailableTrees.Clear();
		GameObject[] results = hitBoxShape.Cast(hitLayer);

		foreach( var result in results ) {
			UnitEssence target = result.GetUnitEssence();
			if (target != null && target != myEssence) {
				currentTargets.Add(target);
			//} else {
			//	HarvestableTree tree = result.GetComponent<HarvestableTree>();
			//	if ( tree!=null) {
			//		currentAvailableTrees.Add(tree);
			//	}
			}
		}
		
		// Try to target the closest enemy
		if ( currentTargets.Count > 0 ) {
			targetRetical.gameObject.SetActive(false);
			targetReticalEnemies.gameObject.SetActive(true);
			var closest = GetClosestTarget();
			targetReticalEnemies.position = closest.transform.position;
		} else 
		// If that doesn't work, try to target the closest tree
		if (GetClosestTree()!=null) {
			targetReticalEnemies.gameObject.SetActive(false);
			SetTargetToClosestTree();
		}
		else {
			targetReticalEnemies.gameObject.SetActive(false);
			targetRetical.gameObject.SetActive(false);
		}

		bool showGrabbedIndicator = false;
		TurnOffDepositResourceIndicator();


		if ( currentGrabbedTarget!=null ) {
			Vector3 vector = grabPosition.position - currentGrabbedTarget.transform.position;
			Vector3 vectorDirection = vector.normalized;
			float ease = SigmoidCurve(vector.sqrMagnitude / (grabDistance * grabDistance));
			//float ease = Mathf.Min(1, vector.sqrMagnitude / (grabDistance*grabDistance));

			if (vector.magnitude < grabDistance) {
				currentGrabbedTarget.ApplyBrakeForce( (1-ease * 0.5f)* brakeForce);

			}

			//currentGrabbedTarget.ApplyForce(vectorDirection * ease * grabForce);
			currentGrabbedTarget.ApplyForce(vector * grabForce);
			showGrabbedIndicator = true;

			targetRetical.gameObject.SetActive(false);

			Resourceable resourceable = currentGrabbedTarget.GetComponent<Resourceable>();
			TryToPutResourceInReceptical(resourceable);
		}

		if (currentGrabbedBody != null) {
			var grabPoint = currentGrabbedBody.transform.TransformPoint(currentGrabbedPointOffset);

			Vector3 vector = grabPosition.position - grabPoint;
			Vector3 vectorDirection = vector.normalized;
			float ease = SigmoidCurve(vector.sqrMagnitude / (grabDistance * grabDistance));
			//float ease = Mathf.Min(1, vector.sqrMagnitude / (grabDistance*grabDistance));

			if (vector.magnitude < grabDistance) {
				float rate = (1 - ease * 0.5f) * brakeForce;
				currentGrabbedBody.AddForce(-currentGrabbedBody.velocity * Time.deltaTime * rate,
					ForceMode.Impulse);
			}

			//currentGrabbedBody.AddForce(vector * grabForce * Time.deltaTime, ForceMode.Impulse);

			//currentGrabbedBody.AddForceAtPosition(vector * grabForce * Time.deltaTime, grabPoint, ForceMode.Impulse);
			currentGrabbedBody.AddForceAtPosition(vector * grabForce, grabPoint);


			if (doingSpin) {
				var throwable = currentGrabbedBody.GetComponent<Throwable>();
				throwable.Spin(grabPoint);
			}

			showGrabbedIndicator = true;

			targetRetical.gameObject.SetActive(false);

			Resourceable resourceable = currentGrabbedBody.GetComponent<Resourceable>();
			TryToPutResourceInReceptical(resourceable);
		}

		whipEndObject.gameObject.SetActive(showGrabbedIndicator);

		if ( doingGrapplingHook ) {
			var myBody = GetComponentInParent<Rigidbody>();
			Vector3 vector;
			if (currentGrapplingBody != null) {
				vector = currentGrapplingBody.transform.TransformPoint(currentGrapplingPointOffset) - transform.position;
			} else {
				vector = grapplingHookPoint - transform.position;
			}
			var displacementForce = vector * grapplingHookForce;
			if ( displacementForce.sqrMagnitude > grapplingHookForceMax* grapplingHookForceMax) {
				displacementForce = vector.normalized * grapplingHookForceMax;
			}
			myBody.AddForce(displacementForce);

			var changeInVector = vector - lastGrapplingHookVector;
			myBody.AddForce(changeInVector * grapplingHookDampening);
			lastGrapplingHookVector = vector;
		}
	}

	private Vector3 lastGrapplingHookVector;

	private void TurnOffDepositResourceIndicator() {
		depositResourcesIndicator.gameObject.SetActive(false);
	}

	private void TryToPutResourceInReceptical(Resourceable resourceable) {
		if (resourceable == null) {
			closestReceptical = null;
			depositResourcesIndicator.gameObject.SetActive(false);
			return;
		}

		GameObject[] nearbyRecepticals =
				QueryManager.GetNearbyResourceRecepticals(resourceable.transform.position, resourceableDepositDistance);

		if (nearbyRecepticals.Length == 0) {
			closestReceptical = null;
			depositResourcesIndicator.gameObject.SetActive(false);
			return;
		}

		closestReceptical = nearbyRecepticals[0];
		targetRetical.position = closestReceptical.transform.position;
		targetRetical.gameObject.SetActive(true);
		depositResourcesIndicator.gameObject.SetActive(true);
	}

	private void DepositResourceableInReceptical(Resourceable resourceable, ResourceReceptical receptical) {
		receptical.ProcessResourceable(resourceable);
	}

	/// <summary>
	/// Takes a [0,1] value and returns the corresponding [0,1] sigmoid value.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	private float SigmoidCurve(float value) {
		// Uses tanh
		float exp = -2f * (4f*value-2f);
		float dem = 1 + Mathf.Exp(exp);
		return (2f/dem - 1) *0.5f + 0.5f;
	}

	//private void SetCurrentTarget( Blackboard newTarget ) {
	//	currentTarget = newTarget;
	//}

	private void UpdatePress() {
		doingSpin = false;
		if (Input.GetMouseButton(1)) {
			if (currentGrabbedBody != null ) {
				var throwable = currentGrabbedBody.GetComponent<Throwable>();
				if ( throwable!=null ) {
					doingSpin = true;
				}
			}
		}
		if ( Input.GetMouseButtonUp(1)) {
			
		}

		// Short presses are simple, short clicks
		// If the player holds the button down for a specific amount of time they turn into Long presses
		if (Input.GetMouseButtonDown(0)) {
			StartPress();
		}
		if (doingPress) {
			if (Input.GetMouseButtonUp(0)) {
				DoShortPress();
			} else
			// wait for the player to press the mouse button then decide what kind of press it is
			if (longPressTimer > 0f) {
				longPressTimer -= Time.deltaTime;
				if (longPressTimer <= 0f) {
					DoLongPress();
				}
			}
		} else {
			if (Input.GetMouseButtonUp(0) ) {
				if (currentGrabbedTarget != null) {
					ReleaseGrabbedUnit();
				}
				if ( currentGrabbedBody!=null) {
					ReleaseGrabbedBody();
				}
				StopGrapplingHookMaybe();
			}
		}
	}

	private void StartPress() {
		longPressTimer = longPressPause;
		doingPress = true;
	}

	private void DoShortPress() {
		doingPress = false;
		foreach (var target in currentTargets) {
			myDamage.amount = damage;
			myDamage.pushBack = pushBackForce * (target.transform.position - transform.position).normalized;
			target.ApplyDamage(myDamage);
		}
	}

	private void DoLongPress() {
		doingPress = false;
		currentGrabbedTarget = GetClosestTarget();
		if ( currentGrabbedTarget!=null) {
			Events.Fire(PlayerWhipEvent.StartGrabbing);
			currentGrabbedTarget.Incapacitate();
		} else {
			if (GetClosestTree() != null) {
				GrabClosestTree();
			} else {
				StartGrapplingHookMaybe();
			}
		}

	}
	
	private void ReleaseGrabbedUnit() {
		currentGrabbedTarget.Capacitate();

		Resourceable resourceable = currentGrabbedTarget.GetComponent<Resourceable>();
		
		if (resourceable!=null && closestReceptical!=null) {
			var receptical = closestReceptical.GetComponent<ResourceReceptical>();
			DepositResourceableInReceptical(resourceable, receptical);
		}

		Events.Fire(PlayerWhipEvent.StopGrabbing);
		currentGrabbedTarget = null;
	}

	private void ReleaseGrabbedBody() {
		Resourceable resourceable = currentGrabbedBody.GetComponent<Resourceable>();

		if (resourceable != null && closestReceptical != null) {
			var receptical = closestReceptical.GetComponent<ResourceReceptical>();
			DepositResourceableInReceptical(resourceable, receptical);
		} else 
		if( doingSpin && crossHairs.HittingSomething) {
			var throwable = currentGrabbedBody.GetComponent<Throwable>();
			var vector = crossHairs.aimRayHit.point - currentGrabbedBody.transform.position;
			throwable.Throw(vector);
			doingSpin = false;
		}

		Events.Fire(PlayerWhipEvent.StopGrabbing);
		currentGrabbedBody = null;
	}

	private void StartGrapplingHookMaybe() {
		if ( crossHairs.HittingSomething ) {
			currentGrapplingBody = crossHairs.aimRayHit.collider.GetComponent<Rigidbody>();
			if (currentGrapplingBody == null) {
				grapplingHookPoint = crossHairs.aimRayHit.point;
				lastGrapplingHookVector = grapplingHookPoint - transform.position;
			}
			else {
				currentGrapplingPointOffset = currentGrapplingBody.transform.InverseTransformPoint(crossHairs.aimRayHit.point);
				lastGrapplingHookVector = currentGrapplingBody.transform.TransformPoint(currentGrapplingPointOffset) - transform.position;
				
			}
			doingGrapplingHook = true;
			Events.Fire(PlayerWhipEvent.StartGrabbing);
		}
	}

	private void StopGrapplingHookMaybe() {
		if (doingGrapplingHook) {
			if (currentGrapplingBody != null) {
				currentGrapplingBody = null;
			}
			doingGrapplingHook = false;
			Events.Fire(PlayerWhipEvent.StopGrabbing);
		}
		
	}

	private UnitEssence GetClosestTarget() {
		if (crossHairs.HittingSomething) {
			var unit = crossHairs.aimRayHit.collider.gameObject.GetComponent<UnitEssence>();
			if (unit == null) {
				unit = crossHairs.aimRayHit.collider.gameObject.GetComponentInParent<UnitEssence>();
			}
			if (unit != null) {
				return unit;
			}
		}
		if (currentTargets.Count == 0) {
			return null;
		}
		UnitEssence closest = currentTargets[0];
		float closestDistance = -1;
		foreach (var target in currentTargets) {
			float distance = (target.transform.position - transform.position).sqrMagnitude;
			if (closestDistance==-1 || distance < closestDistance ) {
				closest = target;
				closestDistance = distance;
			}
		}
		return closest;
	}

	private Grabbable GetClosestTree() {
		if (crossHairs.HittingSomething ) {
			var tree = crossHairs.aimRayHit.collider.gameObject.GetComponent<Grabbable>();
			if ( tree==null) {
				tree = crossHairs.aimRayHit.collider.gameObject.GetComponentInParent<Grabbable>();
			}
			if ( tree!=null) {
				return tree;
			}
		}

		return null;
	}

	private void SetTargetToClosestTree() {
		targetRetical.gameObject.SetActive(true);

		var closest = GetClosestTree();

		if (closest!=null) {
			targetRetical.position = crossHairs.aimRayHit.point;
		}
	}

	private void GrabClosestTree() {
		var closest = GetClosestTree();
		if (closest != null) {
			closest.StartGrab();
			currentGrabbedBody = closest.GetComponent<Rigidbody>();
			currentGrabbedPointOffset = closest.transform.InverseTransformPoint(crossHairs.aimRayHit.point);
			Events.Fire(PlayerWhipEvent.StartGrabbing);
		}
	}

	

	private void UpdateWhipEnd() {
		if ( currentGrabbedTarget != null ) {
			whipTarget = currentGrabbedTarget.transform.position;
		} else
		if (currentGrabbedBody != null) {
			whipTarget = currentGrabbedBody.transform.TransformPoint(currentGrabbedPointOffset);
		} else
		if (doingGrapplingHook) {
			if (currentGrapplingBody != null) {
				whipTarget = currentGrapplingBody.transform.TransformPoint(currentGrapplingPointOffset);
			} else {
				whipTarget = grapplingHookPoint;
			}
		}
		else {
			whipTarget = whipHandlePosition.position;
		}

		whipEnd = Vector3.Lerp(whipEnd, whipTarget, 0.9f);
		whipEndObject.transform.position = whipEnd;
	}
}
public enum PlayerWhipEvent {
	StartGrabbing,
	StopGrabbing,

}