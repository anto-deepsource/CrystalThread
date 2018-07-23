using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	public class HexNavMeshManager : Singleton<HexNavMeshManager> {
		private HexNavMeshManager() { }

		#region Private members

		private static HexagonMaker maker;

		private static HexMap map;

		#endregion

		#region Public Static Functions

		public static HexagonMaker GetHexMaker() {
			if ( maker == null ) {
				// TOOD: come up with something better than this
				maker = GameObject.Find("HexMap").GetComponent<HexagonMaker>();
			}
			return maker;
		}

		public static HexMap GetHexMap() {
			if (map == null) {
				// TOOD: come up with something better than this
				map = GameObject.Find("HexMap").GetComponent<HexMap>();
			}
			return map;
		}

		#endregion


	}
}