using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Data structure for saving path data for reuse, avoiding having to process it again.
	/// If a path from one point to another has not been found yet, it is processed, then
	/// the result it saved here as a table where every possible point is a column and
	/// every possible point is a row.
	/// The cells represent a relationship between the corresponding points,
	/// containing the next, adjacent point to travel to in order to get from
	/// the one point to the other.
	/// If the terrain changes, then all paths that depend on that point are invalidated.
	/// </summary>
	public class PointToPointTable {

		/// <summary>
		/// Main data table: dictionary of dictionaries where axis 0 is the start/current position, axis 1 is the target/end position, and the value
		/// is the next adjacent position in order to traverse from start to target.
		/// </summary>
		private Dictionary<Vector2Int, Dictionary<Vector2Int, Vector2Int>> pointTable = new Dictionary<Vector2Int, Dictionary<Vector2Int, Vector2Int>>();


	}
}
