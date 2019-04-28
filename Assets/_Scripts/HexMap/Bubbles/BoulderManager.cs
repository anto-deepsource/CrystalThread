using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderManager : MonoBehaviour {

	

	public GameObject boulderPrefab;

	private QuickListener listener;
	
	void Start() {
		listener = new QuickListener(BubbleCallbacks);
		BubbleManager.Events.Add(listener);
	}

	private void BubbleCallbacks(int eventCode, object data) {
		switch ((BubbleEvent)eventCode) {
			case BubbleEvent.NewBubble:
				Bubble newBubble = data as Bubble;

				CreateSpiralOfRocks(newBubble.center, newBubble.radius);
				
				break;
		}
	}

	private void CreateSpiralOfRocks( Vector2 center, float radius ) {

		for( float r = 0; r < radius; r += 10 ) {
			float x = r * Mathf.Cos(r);
			float z = r * Mathf.Sin(r);
			CreateNewRock(center.x+x, center.y+z);
		}
	}

	private void CreateNewRock(float x, float z ) {
		GameObject newBoulder = Instantiate(boulderPrefab, transform);
		newBoulder.transform.position = HexNavMeshManager.WorldPositionPlusMapHeight(new Vector3(x,0,z) );

	}

	private void CreateNewRock(Vector2 center) {
		CreateNewRock(center.x, center.y);
	}
}
