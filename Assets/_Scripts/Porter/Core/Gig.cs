using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gig : ITask {

	public delegate bool UpdateDelegate(Blackboard blackboard);

	private UpdateDelegate _update;

	public Gig( UpdateDelegate update ) {
		_update = update;
	}

	public bool Update(Blackboard blackboard) {
		return _update(blackboard);
	}
}
