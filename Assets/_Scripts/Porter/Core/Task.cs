using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Task : ScriptableObject {
	[SerializeField, Multiline]
	public string description = "Empty Task";

	public Task(string description) {
		this.description = description;
	}

	//virtual public bool IsFinished { get; set; }

	//virtual public bool Succeeded { get; set; }

	virtual public void Enter(Blackboard blackboard) {  }

	// the bool returned is 'IsFinished'
	virtual public bool TaskUpdate(Blackboard blackboard) {
		return true;
	}

	// the bool returned is 'IsFinished'
	virtual public bool TaskFixedUpdate(Blackboard blackboard) { return true; }

	// the bool returned is 'Succeeded'
	virtual public bool Exit(Blackboard blackboard) { return true; }

	virtual public void Interrupt(Blackboard blackboard) { }



}
