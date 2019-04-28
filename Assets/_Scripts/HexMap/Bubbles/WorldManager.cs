using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour {

	public GameObject joblinPrefab;

	public GameObject bullyPrefab;

	public GameObject dollPrefab;

	private QuickListener listener;

	void Start() {
		listener = new QuickListener(BubbleCallbacks);
		BubbleManager.Events.Add(listener);
	}

	private void BubbleCallbacks(int eventCode, object data) {
		switch ((BubbleEvent)eventCode) {
			case BubbleEvent.NewBubble:
				Bubble newBubble = data as Bubble;

				CreateNewWorld(newBubble.center, newBubble.radius);

				break;
		}
	}

	private void CreateNewWorld(Vector2 center, float radius) {
		//CreateNewDollSomewhere(center, radius);

		for(int i = 0; i < 4; i ++ ) {
			CreateNewJoblinSomewhere(center, radius);
		}
		//for(float r = 0; r<radius; r += 10 ) {
		//	float x = r * Mathf.Cos(r);
		//	float z = r * Mathf.Sin(r);
		//	CreateNewRock(center.x+x, center.y+z);
		//}
	}

	private void CreateNewDollSomewhere(Vector2 center, float radius ) {
		GameObject newDoll = Instantiate(dollPrefab, transform);
		newDoll.SetActive(true);
		var xzPosition = center + Random.insideUnitCircle * radius;
		var position = HexNavMeshManager.WorldPositionPlusMapHeight(xzPosition.FromXZ());
		newDoll.transform.position = position;
	}

	private void CreateNewJoblinSomewhere(Vector2 center, float radius) {
		GameObject newJoblin = Instantiate(joblinPrefab, transform);
		newJoblin.SetActive(true);
		//var xzPosition = center + Random.insideUnitCircle * radius;
		//var position = HexNavMeshManager.WorldPositionPlusMapHeight(xzPosition.FromXZ());
		var position = HexNavMeshManager.GetAnyPointInArea(center, radius);
		position = HexNavMeshManager.WorldPosToWorldPosWithGround(position);
		newJoblin.transform.position = position;
	}
}
