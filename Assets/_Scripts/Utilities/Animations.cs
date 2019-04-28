using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Animations {

	public static IEnumerator TranslateLocal(Transform target, Vector3 translation, float tweenTime ) {

		Vector3 startPos = target.localPosition;
		Vector3 endPos = target.localPosition + translation;
		float t = 0;
		while( t < tweenTime ) {
			t += Time.deltaTime;
			target.localPosition = Ease.EaseOut(startPos, endPos, t / tweenTime);
			yield return null;
		}
		target.localPosition = endPos;
	}

	public static IEnumerator AlphaOut(Text target, float tweenTime) {

		float startPos = target.color.a;

		float t = 0;
		while (t < tweenTime) {
			t += Time.deltaTime;
			Color newColor = target.color;
			newColor.a = Ease.EaseOut(startPos, 0, t / tweenTime);
			target.color = newColor;
			yield return null;
		}
		target.color = Color.clear;
	}
}
