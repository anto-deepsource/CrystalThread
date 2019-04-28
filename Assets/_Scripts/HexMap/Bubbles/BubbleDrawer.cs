using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	public class BubbleDrawer : MonoBehaviour {

		public GameObject bubblePrefab;

		public float yOffset = 15f;

		private QuickListener listener;

		// Use this for initialization
		void Start() {
			listener = new QuickListener(BubbleCallbacks);
			BubbleManager.Events.Add(listener);
		}

		private void BubbleCallbacks( int eventCode, object data ) {
			switch( (BubbleEvent) eventCode ) {
				case BubbleEvent.NewBubble:
					Bubble newBubble = data as Bubble;
					GameObject newObject = Instantiate(bubblePrefab, transform);
					newObject.transform.position = newBubble.center.FromXZ() + Vector3.up * yOffset;
					RadiusIndicator radiusIndicator = newObject.GetComponent<RadiusIndicator>();
					radiusIndicator.radius = newBubble.radius;
					break;
			}
		}
	}
}