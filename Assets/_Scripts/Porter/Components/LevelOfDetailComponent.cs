using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOfDetailComponent : MonoBehaviour {

	public int levelOfDetail = -1;

	private LevelOfDetailManager manager;
	
	public ExtraEvent<LevelOfDetailEvent> Events {
		get {
			if ( _events == null ) {
				_events = new ExtraEvent<LevelOfDetailEvent>(this);
			}
			return _events;
		}
	}
	private ExtraEvent<LevelOfDetailEvent> _events;

	private void Start() {
		manager = LevelOfDetailManager.GetObject();
	}

	private void Update() {
		float distanceToCow = Vector3.SqrMagnitude(transform.position - manager.CowPosition());

		float levelOneDistance = manager.levelOneDistance;
		float levelTwoDistance = manager.levelTwoDistance;

		int newLOD = 0;

		if (distanceToCow < levelOneDistance * levelOneDistance) {
			newLOD = 1;
		//} else 
		//if (distanceToCow < levelTwoDistance * levelTwoDistance) {
		//	newLOD = 2;
		}

		if ( levelOfDetail != newLOD ) {
			Events.Fire(LevelOfDetailEvent.LevelChange, newLOD);
		}
		levelOfDetail = newLOD;
	}
}
public enum LevelOfDetailEvent {
	LevelChange, // data passed is the new level about to be changed to

}