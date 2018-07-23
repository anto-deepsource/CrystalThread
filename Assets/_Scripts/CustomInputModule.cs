using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInputModule : StandaloneInputModule {
	protected override void ProcessMove(PointerEventData pointerEvent) {
		var targetGO = pointerEvent.pointerCurrentRaycast.gameObject;
		HandlePointerExitAndEnter(pointerEvent, targetGO);
	}
}