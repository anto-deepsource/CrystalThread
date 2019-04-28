
using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class HiveAgent : MonoBehaviour {
		
		public PrimitiveTask rootTask;

		public List<GameObject> subunits = new List<GameObject>();

		#region Private Variables
		
		private bool running = false;
		
		#endregion
		
		void Start() {
			running = true;
			rootTask.Begin();
		}
		
	}
}