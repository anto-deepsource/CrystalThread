using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	[CreateAssetMenu(fileName = "HexMapData", menuName = "Hex Map Data", order = 1)]
	public class MapData : ScriptableObject {

		[SerializeField] public HexWallTable<bool> isWallTable = new HexWallTable<bool>();
	}
}