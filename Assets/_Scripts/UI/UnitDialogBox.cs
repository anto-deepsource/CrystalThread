using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;
using UnityEngine.UI;

public class UnitDialogBox : AnimationHandler {

	public GameObject messageBubblePrefab;

	public Transform startPoint;

	public float bubbleLifetimePerCharacter = .2f;

	private List<BubbleInfo> messageBubbles = new List<BubbleInfo>();

	//private List<GameObject> childBubbles = new List<GameObject>();

	private List<Coroutine> verticalTranslationCoroutines = new List<Coroutine>();

	TextGenerator textGenerator;

	private Image panelBackground;
	private float panelBackgroundMaxAlpha;
	private Color panelBackgroundColor;
	private bool panelBackgroundShowing = false;
	private Coroutine panelBackgroundCoroutine;

	private void Awake() {
		textGenerator = new TextGenerator();
		panelBackground = GetComponent<Image>();
		panelBackgroundMaxAlpha = panelBackground.color.a;
		panelBackgroundColor = panelBackground.color;
		SetBackgroundAlpha(0);
	}

	private void Update() {
		if (messageBubbles.Count == 0 && panelBackgroundShowing && panelBackgroundCoroutine==null) {
			panelBackgroundCoroutine = StartCoroutine(FadeOutBackgroundAfterPause(1,1));
		}
	}

	override public void Play(AnimationKey key, AnimationData data) {
		if ( key != AnimationKey.Speaks ) {
			return;
		}

		string message = data.Message;
		
		PushNewMessage(message);
	}

	private void PushNewMessage(string message) {
		var newBubble = Instantiate(messageBubblePrefab, transform);
		newBubble.SetActive(true);
		var textComponent = newBubble.GetComponentInChildren<Text>();
		textComponent.text = message;



		TextGenerationSettings generationSettings =
			textComponent.GetGenerationSettings(textComponent.rectTransform.rect.size);
		textComponent.rectTransform.ForceUpdateRectTransforms();
		float height = textGenerator.GetPreferredHeight(message, generationSettings);

		var backgroundImage = newBubble.GetComponentInChildren<Image>();
		float backgroundHeight = backgroundImage.rectTransform.sizeDelta.y;

		textComponent.transform.localPosition = startPoint.localPosition +
				Vector3.down * (height + backgroundHeight);

		var bubbleInfo = new BubbleInfo() {
			gameObject = newBubble,
			height = height + backgroundHeight,
			position = -(height + backgroundHeight),
		};

		messageBubbles.Add(bubbleInfo);

		ShiftAllBubblesUp(height + backgroundHeight);

		float bubbleLifetime = bubbleLifetimePerCharacter * message.Length;
		StartCoroutine(FadeOutAfterPause(bubbleInfo, bubbleLifetime, 1f));

		SetBackgroundAlpha(panelBackgroundMaxAlpha);
	}

	private void ShiftAllBubblesUp(float distance) {
		StopAnyVerticalTranslations();
		foreach( var bubble in messageBubbles) {
			bubble.position += distance;

			var coroutine = StartCoroutine(TweenVertically(bubble, bubble.position, 1f));
			verticalTranslationCoroutines.Add(coroutine);
		}
	}
	
	private void StopAnyVerticalTranslations() {
		foreach( var coroutine in verticalTranslationCoroutines) {
			StopCoroutine(coroutine);
		}
		verticalTranslationCoroutines.Clear();
	}
	
	private void SetBackgroundAlpha(float alpha) {
		if ( panelBackgroundCoroutine!=null) {
			StopCoroutine(panelBackgroundCoroutine);
			panelBackgroundCoroutine = null;
		}
		panelBackgroundColor.a = alpha;
		panelBackground.color = panelBackgroundColor;
		panelBackgroundShowing = alpha > 0;
	}

	private static IEnumerator TweenVertically(BubbleInfo target, float newVerticalPosition, float tweenTime) {

		var transform = target.gameObject.transform;

		Vector3 startPos = transform.localPosition;
		Vector3 endPos = new Vector3(startPos.x, newVerticalPosition, startPos.z);
		float t = 0;
		while (t < tweenTime && !target.destroyed) {
			t += Time.deltaTime;
			transform.localPosition = Ease.EaseOut(startPos, endPos, t / tweenTime);
			yield return null;
		}
		if ( !target.destroyed) {
			transform.localPosition = endPos;
		}
		
	}

	private IEnumerator FadeOutAfterPause(BubbleInfo target, float lifetime, float fadeOutTime) {

		yield return new WaitForSeconds(lifetime);

		Image image = target.gameObject.GetComponentInChildren<Image>();

		Color startColor = image.color;
		Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

		float t = 0;
		while (t < fadeOutTime) {
			t += Time.deltaTime;
			image.color = Color.Lerp(startColor, endColor, t / fadeOutTime);
			yield return null;
		}
		messageBubbles.Remove(target);
		target.destroyed = true;
		Destroy(target.gameObject);
	}

	public IEnumerator FadeOutBackgroundAfterPause( float lifetime, float fadeOutTime) {

		yield return new WaitForSeconds(lifetime);

		Color startColor = panelBackground.color;
		Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

		float t = 0;
		while (t < fadeOutTime) {
			t += Time.deltaTime;
			panelBackground.color = Color.Lerp(startColor, endColor, t / fadeOutTime);
			yield return null;
		}

		SetBackgroundAlpha(0);
	}

	private class BubbleInfo {
		public GameObject gameObject;
		public float height;
		public float position;
		public bool destroyed = false;
	}
}
