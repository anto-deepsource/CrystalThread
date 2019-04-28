
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPlayerController : MonoBehaviour {


	public UnitEssence myEssence;

	public float jumpForce = 15.0f;
	public float jumpBoostTime = 0.5f;
	
	//public bool controlUnitAnimator = true;
	public bool makeUnitLookForward = true;

	public ExtraEvent<ThirdPersonControllerEvent> Events {
		get {
			if (_events == null) {
				_events = new ExtraEvent<ThirdPersonControllerEvent>(this);
			}
			return _events;
		}
	}
	ExtraEvent<ThirdPersonControllerEvent> _events;

	#region Private Variables


	private float jumpBoostTimer = 0.0f;
	
	private bool _jump = false;
	
	//private bool controllerEnabled = true;

	/// <summary>
	/// The state of the current input, whether we're using the mouse/keyboard or gamepad.
	/// </summary>
	private ControllerState controllerState = new ControllerState();

	#endregion

	void Start() {
		if (myEssence == null) {
			myEssence = gameObject.GetUnitEssence();
		}
		if (myEssence == null) {
			this.enabled = false;
		}
		myEssence.currentControllers.Push(this);
	}

	void Update() {
		if ( !myEssence.IsDead && !myEssence.IsIncapacitated ) {
			MoveUpdate();
		}
	
	}

	private void MoveUpdate() {
		if (!myEssence.ControllerIsInControl(this)) {
			return;
		}
		controllerState.Update();

		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		Transform camTransform = Camera.main.transform;

		if (myEssence.IsSwimming) {
			myEssence.MoveVector = camTransform.forward * v + camTransform.right * h;
		} else {
			myEssence.MoveVector = ((camTransform.forward * v + camTransform.right * h).DropY()).normalized;
		}
		
		if (!_jump && myEssence.groundedComponent.IsGrounded && Input.GetButtonDown("Jump")) {
			_jump = true;
			Events.Fire(ThirdPersonControllerEvent.Jump, null);
		}
	}
	

	private void FixedUpdate() {
		if (!myEssence.ControllerIsInControl(this)) {
			return;
		}

		if (jumpBoostTimer > 0.0f) {
			jumpBoostTimer -= Time.deltaTime;
		}
		if (_jump) {
			myEssence.SetVerticalVelocity(jumpForce);
			jumpBoostTimer = jumpBoostTime;
			_jump = false;
		}
		else
		if (jumpBoostTimer > 0.0f && Input.GetButton("Jump")) {
			float v = jumpForce * Mathf.Sin(Mathf.PI * 0.5f * (jumpBoostTimer / jumpBoostTime));
			myEssence.SetVerticalVelocity(v);
		}
		else {
			jumpBoostTimer = 0.0f;
		}
		
		if (makeUnitLookForward) {
			myEssence.TurnVector = Camera.main.transform.forward.JustXZ();
			//myEssence.TurnToDirection(Camera.main.transform.forward.JustXZ());
		}
		else
		if (myEssence.MoveVector.sqrMagnitude > 0.01f) {
			myEssence.TurnVector = myEssence.MoveVector.JustXZ();
			//myEssence.TurnToDirection(moveVector.JustXZ());
		}
	}
	
}
