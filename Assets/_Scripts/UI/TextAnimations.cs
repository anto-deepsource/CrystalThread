using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TextAnimations {

	public static void FlashRed(Text text, Color defaultColor, float duration = 1.0f) {
		text.StopAllCoroutines();
		text.StartCoroutine(_FlashRed(text,defaultColor,duration));
	}

	private static IEnumerator _FlashRed( Text text, Color defaultColor, float duration = 1.0f ) {
		text.color = Color.red;
		for ( float t= 0; t < duration; t += Time.deltaTime ) {
			yield return null;
		}
		text.color = defaultColor;
	}
}
