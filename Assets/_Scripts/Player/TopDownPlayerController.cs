using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

/*
 * Similiar to the UnitController but doesn't do the camera or jumping.
 * 
 **/
public class TopDownPlayerController : MonoBehaviour {

	[Tooltip("The layer used for checking whether the character is grounded and also for positioning the camera.")]
	public LayerMask groundLayer;
	
	public float moveForce = 60;
	public float moveSpeed = 40;
	//public float sprintSpeed = 12.0f;
	
	[Tooltip("While true, pressing 'Exit' (Default: 'Esc' key) while playing in the editor will quit stop the play test.")]
	public bool useQuickExit = true;
	[Tooltip("While true, pressing 'ToggleLockedCursor' (Default: 'Tab' key) while playing in the editor will hide/unhide the mouse cursor.")]
	public bool useQuickCursorToggle = true;

	public bool controlUnitAnimator = true;

	public GameObject crossHairs;

	public bool brake = true;
	public float brakeRate = 1.0f;
	public float extraGravity = -100.0f;
	bool IsGrounded = false;
	
	Rigidbody myBody;
	Vector3 movement;

	UnitAnimator animator;

	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	private ControllerState controllerState = new ControllerState();

	void Start() {
		myBody = GetComponent<Rigidbody>();

		// Lock and hide the cursor
		//Cursor.lockState = CursorLockMode.Locked;

		animator = GetComponentInChildren<UnitAnimator>();
		if (controlUnitAnimator && animator == null)
			controlUnitAnimator = false;
	}

	// Update is called once per frame
	void Update() {

		controllerState.Update();
		MoveUpdate();
		AnimatorUpdate();
		Hotkeys();

	}

	void FixedUpdate() {
		BodyFixedUpdate();
	}
	
	void Hotkeys() {
		if (useQuickExit && Application.isPlaying && Input.GetButtonDown("Exit")) {
			//Debug.Log("Used Quick Exit");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		//if (useQuickCursorToggle && Application.isPlaying && Input.GetButtonDown("ToggleLockedCursor")) {
		//	// Lock and hide the cursor
		//	if (Cursor.lockState == CursorLockMode.Locked) {
		//		Cursor.lockState = CursorLockMode.None;
		//	} else {
		//		Cursor.lockState = CursorLockMode.Locked;
		//	}
		//}
	}

	void MoveUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		Transform camTransform = Camera.main.transform;

		movement = camTransform.forward * v + camTransform.right * h;
		movement.y = 0.0f;

		if (myBody.velocity.magnitude < moveSpeed)
			movement = movement.normalized * moveForce;
		else
			movement = Vector3.zero;


		//if (!IsGrounded)
		//	Debug.Log("Not grounded");


		//Debug.Log(string.Format("movement: {0}, IsGrounded: {1}", movement, IsGrounded));
		if (brake && movement.magnitude < 0.001f && IsGrounded) {
			//Debug.Log("Counter Velocity");
			movement = -myBody.velocity * brakeRate;
		}
		if (!IsGrounded)
			movement.y = extraGravity;

		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);

		// line equation = camRay.origin + t * camRay.direction
		// we want the point where that line crosses the y plane that the player is on so
		// transform.position.y = camRay.origin.y + t * camRay.direction.y
		// t = (transform.position.y-camRay.origin.y)/camRay.direction.y
		Vector3 cursorPoint = camRay.origin + ((transform.position.y - camRay.origin.y) / camRay.direction.y) * camRay.direction;
		crossHairs.transform.position = cursorPoint;
		transform.rotation = Quaternion.LookRotation(cursorPoint - transform.position);
	}


	void BodyFixedUpdate() {
		if (IsGrounded)
			myBody.angularVelocity = Vector3.zero;

		myBody.AddForce(movement * Time.deltaTime, ForceMode.Impulse);
	}

	void AnimatorUpdate() {
		if (!controlUnitAnimator) {
			return;
		}

		animator.IsInAir = !IsGrounded;
		animator.IsRunning = movement.magnitude > 0.001f;

	}

	static Vector3 _v = Vector3.zero;
	public static Vector3 DropY(Vector3 v) {
		_v.Set(v.x, 0.0f, v.z);
		return _v;
	}

	public void OnTriggerStay(Collider collider) {
		//Debug.Log("OnTriggerStay");
		IsGrounded = true;
	}

	public void OnTriggerExit(Collider collider) {
		//Debug.Log("OnTriggerExit");
		IsGrounded = false;
	}

}
