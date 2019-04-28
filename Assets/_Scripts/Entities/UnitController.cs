using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class UnitController : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;
	public LayerMask cursorLayer;

	public float jumpForce = 25.0f;
	public float jumpBoostTime = 0.2f;
	private float jumpBoostTimer = 0.0f;

	public float flyForce = 10.0f;
	public float moveForce = 60;
	public float moveSpeed = 40;
	public float sprintSpeed = 12.0f;

	[Tooltip("Whether the character should use and control the main camera.")]
	public bool useCamera = true;
	[Tooltip("Whether or not the camera should interact with objects in the environment, moving closer to avoid obstructions.")]
	public bool useCameraBoomStick = true;
	[Tooltip("Affects the point in space relative to the character that the camera focuses on. " +
		"Higher values cause the camera to focus on the top of or above the character, " +
		"negative values focus the camera below.")]
	public float cameraOffsetY = 2.0f;
	[Tooltip("Affects the point in space relative to the character that the camera focuses on. " +
		"Positive values cause the camera to focus to the right the character (over the shoulder), " +
		"negative values focus the camera on the left.")]
	public float cameraOffsetX = 3.0f;
	[Tooltip("Limits how far the camera can rotate vertically down. Values below -90 will cause bad spinning.")]
	public float cameraMinAngleY = -50.0f;
	[Tooltip("Limits how far the camera can rotate vertically up. Values above 90 will cause bad spinning.")]
	public float cameraMaxAngleY = 50.0f;
	[Tooltip("Limits how far the camera can zoom in. Must be greater than 0.")]
	public float cameraMinZ = 5.0f;
	[Tooltip("Limits how far the camera can zoom out.")]
	public float cameraMaxZ = 60.0f;
	[Tooltip("The initial distance of the camera.")]
	public float cameraDistance = 13;
	[Tooltip("The sensitivity of the camera as it moves around the X-axis (left and right). Negative values with invert the controls.")]
	public float camSmoothX = 4.0f;
	[Tooltip("The sensitivity of the camera as it moves around the Y-axis (up and down). Negative values with invert the controls.")]
	public float camSmoothY = -1.2f;
	[Tooltip("The sensitivity of the camera as it zooms in and out. Negative values with invert the controls.")]
	public float camSmoothZ = -10.0f;

	[Tooltip("While true, pressing 'Exit' (Default: 'Esc' key) while playing in the editor will quit stop the play test.")]
	public bool useQuickExit = true;
	[Tooltip("While true, pressing 'ToggleLockedCursor' (Default: 'Tab' key) while playing in the editor will hide/unhide the mouse cursor.")]
	public bool useQuickCursorToggle = true;

	public bool controlUnitAnimator = true;
	public bool makeUnitLookForward = true;

	public bool useFlight = false;

	public GameObject crossHairs;
	public GameObject buildMenu;

	public bool brake = true;
	public float brakeRate = 1.0f;
	public float extraGravity = -100.0f;
	//bool groundedChecked = false;
	bool IsGrounded = false;
	private bool _jump = false;
	//RaycastHit groundHit;

	Camera cam;
	Transform camTransform;
	float cameraX;
	float cameraY;
	Vector3 cameraOffset;
	

	Rigidbody myBody;
	Vector3 movement;

	UnitAnimator animator;

	private bool controllerEnabled = true;

	private bool placingBuilding = false;
	private GameObject placingObject;
	public float placeRotateSpeed = 3;

	private float height;

	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	private ControllerState controllerState = new ControllerState();

	private ObjectQuery query;

	public void Awake() {
		query = new ObjectQuery() {
			gameObject = gameObject,
			hasBlackboard = false,
			isPlayer = true,
			isUnit = true,
			faction = Faction.Player,

		};
		QueryManager.RegisterObject(query);
	}

	void Start() {
		myBody = GetComponent<Rigidbody>();
		// Grab a handle to the camera
		cam = Camera.main;
		camTransform = cam.transform;
		// Lock and hide the cursor
		Cursor.lockState = CursorLockMode.Locked;

		animator = GetComponentInChildren<UnitAnimator>();
		if (controlUnitAnimator && animator == null)
			controlUnitAnimator = false;

		height = GetComponent<CapsuleCollider>().height;

		HideBuildMenu();
	}

	// Update is called once per frame
	void Update () {

		controllerState.Update();
		if ( controllerEnabled ) {
			CameraUpdate();
			HandleCursor();
		}

		MoveUpdate();
		AnimatorUpdate();
		Hotkeys();
		Menus();
		
	}

	void FixedUpdate() {
		CameraFixedUpdate();
		BodyFixedUpdate();
	}

	void CameraUpdate() {
		if (useCamera) {
			cameraX += controllerState.currentX * camSmoothX;
			cameraY += controllerState.currentY * camSmoothY;

			//Debug.Log(string.Format("{0},{1}", cameraX, cameraY));

			cameraY = Mathf.Clamp(cameraY, cameraMinAngleY, cameraMaxAngleY);

			cameraDistance += Input.GetAxis("Mouse ScrollWheel") * camSmoothZ;
			cameraDistance = Mathf.Clamp(cameraDistance, cameraMinZ, cameraMaxZ);
		}
	}

	void CameraFixedUpdate() {
		if (useCamera) {
			// go out from this character a certain distance by a certain angle
			Vector3 dir = new Vector3(0, 0, -cameraDistance);
			Quaternion rotation = Quaternion.Euler(cameraY, cameraX, 0);
			cameraOffset.Set(cameraOffsetX, cameraOffsetY, 0.0f);

			//// perform a ray cast out from the character
			//// if we hit something we can put our camera there in order to stay in front of things that'll obstruct our view
			//camRay.origin = transform.position + cameraOffset;
			//camRay.direction = rotation * -Vector3.forward;
			//if (useCameraBoomStick && Physics.Raycast(camRay, out camRayhit, cameraDistance, groundLayer)) {
			//	camTransform.position = Vector3.Lerp(camTransform.position, camRayhit.point, 0.8f);
			//} else {
			camTransform.position = Vector3.Lerp(camTransform.position, transform.position + rotation * (dir + cameraOffset), 0.8f);
			//}

			Vector3 focusPoint = transform.position + rotation * (cameraOffset);
			camTransform.LookAt(focusPoint);
			Vector3 forward = DropY(camTransform.forward);

			if (makeUnitLookForward)
				transform.rotation = Quaternion.LookRotation(forward);
			else if(DropY(myBody.velocity).magnitude>0.01f) {
				transform.rotation = Quaternion.LookRotation(DropY(myBody.velocity));
			}

			
		}
	}

	public bool GetCursorRaycast( out RaycastHit rayhit ) {

		Ray camRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

		Vector3 camThisVector = transform.position - camRay.origin;
		Vector3 projection = CommonUtils.Projection(camThisVector, camRay.direction);
		camRay.origin += projection;

		return Physics.Raycast(camRay, out rayhit, 100, cursorLayer);
	}

	void Hotkeys() {
		if (useQuickExit && Application.isPlaying && Input.GetButtonDown("Exit")) {
			Debug.Log("Used Quick Exit");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		
		if (useQuickCursorToggle && Application.isPlaying && Input.GetButtonDown("ToggleLockedCursor")) {
			// Lock and hide the cursor
			if (Cursor.lockState == CursorLockMode.Locked) {
				Cursor.lockState = CursorLockMode.None;
			} else {
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}

	void HandleCursor() {
		RaycastHit cursorRayHit;

		//if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out cursorRayHit, 100, cursorLayer )) {
		Ray camRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

		Vector3 camThisVector = transform.position - camRay.origin;
		Vector3 projection = CommonUtils.Projection(camThisVector, camRay.direction);
		camRay.origin += projection;

		if ( Physics.Raycast(camRay, out cursorRayHit, 100, cursorLayer) ) {
			crossHairs.transform.position = cursorRayHit.point;
			crossHairs.transform.rotation = Quaternion.LookRotation(cursorRayHit.normal);

			if (placingBuilding) {
				if (Input.GetButtonDown("Interact")) {
					PlaceBuilding();
				} else if (Input.GetButton("SpinCW")) {
					placingObject.transform.Rotate(new Vector3(0, placeRotateSpeed, 0));
				} else if (Input.GetButton("SpinCCW")) {
					placingObject.transform.Rotate(new Vector3(0, -placeRotateSpeed, 0));
				}
			} else {

				// test the colliding object for various components and handle looking at them and interacting with them
				GameObject targetObject = cursorRayHit.collider.gameObject;

				Buildable buildable = targetObject.GetComponentInParent<Buildable>();

				if (buildable != null) {
					buildable.OnPlayerLookAt(gameObject);

					if (Input.GetButtonDown("Interact")) {
						buildable.OnPlayerInteract(gameObject);
					}
				}
			}
		}
	}

	void MoveUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		movement = camTransform.forward * v + camTransform.right * h;
		movement.y = 0.0f;

		if (myBody.velocity.magnitude < moveSpeed)
			movement = movement.normalized * moveForce;
		else
			movement = Vector3.zero;

		// Different way of doing jump/ground check
		//IsGrounded = false;
		//RaycastHit groundRayHit;
		//Ray groundRay = new Ray(transform.position, Vector3.down);
		//if ( Physics.Raycast(groundRay, out groundRayHit, height*0.6f, groundLayer) ) {
		//	IsGrounded = true;
		//}
		
		if (brake && movement.magnitude < 0.001f && IsGrounded ) {
			movement = -myBody.velocity* brakeRate;
		}
		if (!IsGrounded)
			movement.y += extraGravity;

		if ( !_jump && IsGrounded && Input.GetButtonDown("Jump") ) {
			_jump = true;
		}

	}

	void Menus() {
		if ( Input.GetButtonDown("BuildMenu" ) ) {
			if ( controllerEnabled ) {
				ClearSettingBuilding();
				DisableController();
				ShowBuildMenu();
			} else {
				EnableController();
				HideBuildMenu();
			}
			
		}
	}

	void BodyFixedUpdate() {
		if (IsGrounded)
			myBody.angularVelocity = Vector3.zero;

		if (!useFlight) {
			if (jumpBoostTimer > 0.0f ) {
				jumpBoostTimer -= Time.deltaTime;
			}
			if (_jump) {
				myBody.velocity = new Vector3(myBody.velocity.x, jumpForce, myBody.velocity.z);
				jumpBoostTimer = jumpBoostTime;
				_jump = false;
			} else
			if (jumpBoostTimer > 0.0f && Input.GetButton("Jump") ) {
				float v = jumpForce * Mathf.Sin(Mathf.PI*0.5f*(jumpBoostTimer / jumpBoostTime));
				myBody.velocity = new Vector3(myBody.velocity.x, v, myBody.velocity.z);
			} else {
				jumpBoostTimer = 0.0f;
			}
		} else {
			if (Input.GetButtonDown("Jump")) {
				myBody.velocity = new Vector3(myBody.velocity.x, flyForce, myBody.velocity.z);
			} else
			// if we are still holding the jump button down
			if (Input.GetButton("Jump")) {
				// slow the fall rate when the unit starts to fall
				if (myBody.velocity.y < 0.0f) {
					myBody.velocity = new Vector3(myBody.velocity.x, 0, myBody.velocity.z);
				}
			}
		}

		myBody.AddForce(movement * Time.deltaTime, ForceMode.Impulse);
	}

	void AnimatorUpdate() {
		if (!controlUnitAnimator) {
			return;
		}

		animator.IsInAir = !IsGrounded;
		animator.IsRunning = movement.magnitude > 0.01f;

	}

	static Vector3 _v = Vector3.zero;
	public static Vector3 DropY(Vector3 v) {
		_v.Set(v.x, 0.0f, v.z);
		return _v;
	}

	/// <summary>
	/// Used in tandem with the FootContact to keep track of jump/grounded
	/// </summary>
	/// <param name="collider"></param>
	public void OnTriggerStay(Collider collider) {
		if ( CommonUtils.IsOnLayer(collider.gameObject, groundLayer ) ) {
			IsGrounded = true;
		}
		
	}

	public void OnTriggerExit(Collider collider) {
		if (CommonUtils.IsOnLayer(collider.gameObject, groundLayer)) {
			IsGrounded = false;
		}
	}

	/// <summary>
	/// Used when menus or dialogs come up to disable the unit controller and reenable the cursor and clicking
	/// </summary>
	public void DisableController() {
		controllerEnabled = false;
		Cursor.lockState = CursorLockMode.None;
	}

	public void EnableController() {
		controllerEnabled = true;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void ShowBuildMenu() {
		buildMenu.SetActive(true);
	}

	public void HideBuildMenu() {
		if (buildMenu != null) {
			buildMenu.SetActive(false);
		}
	}

	public void StartSettingBuilding( StructureData data ) {
		placingObject = GameObject.Instantiate(data.gameObject);
		//placingObject.transform.SetParent(crossHairs.transform);
		//placingObject.transform.localPosition = Vector3.zero;
		placingObject.GetComponent<Buildable>().StartSetting(crossHairs.transform);

		placingBuilding = true;
		crossHairs.transform.Find("SpinIndicators").gameObject.SetActive(true);
		EnableController();
		HideBuildMenu();
	}

	public void ClearSettingBuilding() {
		if ( placingObject!=null ) {
			Destroy(placingObject);
			placingObject = null;
		}
		placingBuilding = false;
		crossHairs.transform.Find("SpinIndicators").gameObject.SetActive(false);
	}

	public void PlaceBuilding() {
		placingObject.transform.SetParent(null);
		placingObject.GetComponent<Buildable>().FinishSetting();
		placingBuilding = false;
		placingObject = null;
		crossHairs.transform.Find("SpinIndicators").gameObject.SetActive(false);
	}
}
