using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestInputSelect : MonoBehaviour, ISelectHandler, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, ISubmitHandler {
	public void OnPointerDown(PointerEventData eventData) {
		Debug.Log("OnPointerDown");
		EventSystem.current.SetSelectedGameObject(gameObject);
	}

	public void OnPointerEnter(PointerEventData eventData) {
		Debug.Log("OnPointerEnter");
	}

	public void OnPointerUp(PointerEventData eventData) {
		Debug.Log("OnPointerUp");
	}

	public void OnSelect(BaseEventData eventData) {
		Debug.Log("OnSelect");
	}

	public void OnSubmit(BaseEventData eventData) {
		Debug.Log("OnSubmit");
	}
}
