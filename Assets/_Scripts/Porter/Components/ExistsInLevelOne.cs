using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistsInLevelOne : MonoBehaviour {

	private LevelOfDetailComponent levelOfDetail;

	private void Start() {
		levelOfDetail = GetComponentInParent<LevelOfDetailComponent>();
		levelOfDetail.Events.Add(this, LevelCallback);
		//UseLevelOfDetail(levelOfDetail.levelOfDetail);

	}

	private void UseLevelOfDetail(int lod ) {
		if (lod == 1) {
			gameObject.SetActive(true);
		}
		else {
			gameObject.SetActive(false);
		}
	}

	private void LevelCallback(System.Object sender, LevelOfDetailEvent eventCode, System.Object data ) {
		switch (eventCode) {
			case LevelOfDetailEvent.LevelChange:
				int newLevelOfDetail = (int)data;
				UseLevelOfDetail(newLevelOfDetail);
				break;
		}
	}
}
