using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	public class GlobalPathManager : Singleton<GlobalPathManager> {
		private GlobalPathManager() { }

		private PathSystem pathSystem = new PathSystem();

		public static void RequestPath(Vector2Int start, Vector2Int end,
				OneShotPathJob<Vector2Int>.CompleteDelegate callback) {
			Instance.pathSystem.RequestPath(start, end, callback);
		}

		private void Update() {
			pathSystem.Update();
		}
	}
}