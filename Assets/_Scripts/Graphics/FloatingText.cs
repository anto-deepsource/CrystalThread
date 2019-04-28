using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour {

	public float disappearTime = 1.2f;
	
	private void StartFloatUp() {
		Vector3 moveVector = Vector3.up * 100 + Vector3.right * 100f * (Random.value *2f - 1f);
		StartCoroutine(Animations.TranslateLocal(transform, moveVector, disappearTime));
		StartCoroutine(WaitAndFadeOut());
		Destroy(gameObject, disappearTime * 1.1f);
	}

	private IEnumerator WaitAndFadeOut() {
		float t = 0;
		while ( t < disappearTime * 0.5f ) {
			yield return null;
			t += Time.deltaTime;
		}
		Text text = GetComponent<Text>();

		yield return Animations.AlphaOut(text, disappearTime * 0.5f);
	}

	public static FloatingText NewDamageText( GameObject targetObject, int amount ) {
		targetObject.SetActive(true);
		FloatingText newText = targetObject.AddComponent<FloatingText>();
		newText.StartFloatUp();

		Text text = targetObject.GetComponent<Text>();
		text.text = string.Format("{0}", amount);
		text.color = Color.red;

		return newText;
	}

	public static FloatingText NewHealText(GameObject targetObject, int amount) {
		targetObject.SetActive(true);
		FloatingText newText = targetObject.AddComponent<FloatingText>();
		newText.StartFloatUp();

		Text text = targetObject.GetComponent<Text>();
		text.text = string.Format("{0}", amount);
		text.color = Color.green;

		return newText;
	}
}
