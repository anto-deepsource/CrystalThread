using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

public class PlayerToNpcDialog : MonoBehaviour {
	public HitWorldCrossHairs crossHairs;

	public GameObject currentInteractableIndicator;

	private UnitEssence myEssence;

	private void Awake() {
		myEssence = gameObject.GetUnitEssence();
	}

	private void Update() {
		bool deactivateIndicator = true;
		if (crossHairs.HasCloseInteractable) {
			GameObject targetObject = crossHairs.closestInteractable;

			UnitEssence essence = targetObject.GetUnitEssence();

			if (essence != null && !essence.IsDead && FactionUtils.IsA(myEssence.faction, essence.faction)) {
				deactivateIndicator = false;
				currentInteractableIndicator.SetActive(true);
				currentInteractableIndicator.transform.position = essence.transform.position + Vector3.up * essence.height;

				if (Input.GetButtonDown("PositiveSocial")) {
					myEssence.Play(AnimationKey.Speaks, AnimationData.NewMessage("How's it going?"));
					essence.Inform(BlackboardEventType.PositiveUnitInteraction, myEssence);
				}
				if (Input.GetButtonDown("EqualSocial")) {
					myEssence.Play(AnimationKey.Speaks, AnimationData.NewMessage("Some kind of weather we're having."));
					essence.Inform(BlackboardEventType.NeutralUnitInteraction, myEssence);
				}
				if (Input.GetButtonDown("NegativeSocial")) {
					myEssence.Play(AnimationKey.Speaks, AnimationData.NewMessage("You want a piece of this?"));
					essence.Inform(BlackboardEventType.NegativeUnitInteraction, myEssence);
				}
			}
		}
		if (currentInteractableIndicator.activeSelf && deactivateIndicator) {
			currentInteractableIndicator.SetActive(false);
		}

	}
}
