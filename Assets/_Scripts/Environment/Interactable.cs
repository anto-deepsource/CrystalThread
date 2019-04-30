using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

	public InteractionSystem interactionSystem;

	public InteractionObject interactionObject;

	void Update () {
		if (Input.GetButtonDown("Interact")) {
			interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, interactionObject, true);
		}
	}
}
