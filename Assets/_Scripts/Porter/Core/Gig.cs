using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gig : ITask {

	public delegate bool UpdateDelegate();

	private UpdateDelegate _update;

	public Gig( UpdateDelegate update ) {
		_update = update;
	}

	public bool UpdateTask() {
		return _update();
	}
}
