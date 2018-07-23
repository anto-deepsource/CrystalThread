using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	[RequireComponent(typeof(HexMap))]
	public class HexMaze : MonoBehaviour {

		public int column = 0;
		public int row = 1;
		public int radius = 3;

		public GameObject lightStonePrefab;

		private HexagonMaker hexagonMaker;
		private GameObject pickupsFolder;

		public void Awake() {
			hexagonMaker = GetComponent<HexagonMaker>();
		}

		private void ValidateHexagonMaker() {
			if (hexagonMaker == null) {

				hexagonMaker = GetComponent<HexagonMaker>();
			}
		}

		public HexWallTableBool GetWallTable() {
			ValidateHexagonMaker();

			pickupsFolder = ObjectFactory.Folder("Picksups", transform);

			HexWallTableBool wallTable = new HexWallTableBool();
			//MakeRing(wallTable, column, row, radius);
			WallOffCircle(wallTable, column, row, radius);
			//Vector2Int pos = HexUtils.RandomPointInArea(column, row, radius);
			//WallOffTile(wallTable, pos);
			GrowingTreeMaze(wallTable, column, row, radius);

			OpenOneWallOnRing(wallTable, column, row, radius);

			return wallTable;
		}

		private void MakeRing(HexWallTable<bool> wallTable, int column, int row, int radius) {
			if ( radius==0 ) {
				WallOffTile(wallTable, column, row);
				return;
			}
			// Pick some direction, and move out (radius) tiles
			HexDirection startDir = HexDirection.E;
			Vector2Int currentPos = HexUtils.MoveFrom(column, row, startDir, radius);
			// Turn and move perpendicular to the ring
			HexDirection moveDir = startDir.Next2();
			
			for( int i = 0; i < 6; i ++ ) {
				// Move CCW one tile for (radius) tiles, putting two walls
				for (int t = 0; t < radius - 1; t++) {
					currentPos = HexUtils.MoveFrom(currentPos, moveDir);
					wallTable.Set(currentPos, moveDir.Last(), true);
					wallTable.Set(currentPos, moveDir.Last2(), true);
				}

				// Move into the corner
				currentPos = HexUtils.MoveFrom(currentPos, moveDir);

				// Corner hexes will need three walls
				wallTable.Set(currentPos, moveDir, true);
				wallTable.Set(currentPos, moveDir.Last(), true);
				wallTable.Set(currentPos, moveDir.Last2(), true);

				// Turn CCW
				moveDir = moveDir.Next();
			}
		}

		/// <summary>
		/// Fills all walls on all tiles within the radius of the given coords.
		/// </summary>
		/// <param name="wallTable"></param>
		/// <param name="column"></param>
		/// <param name="row"></param>
		/// <param name="radius"></param>
		private void WallOffCircle(HexWallTable<bool> wallTable, int column, int row, int radius) {
			foreach ( var pos in HexUtils.ForEachTileWithin( column, row, radius ) ) {
				for (int d = 0; d < 6; d++) {
					wallTable.Set( pos.x, pos.y, (HexDirection)d, true);
				}
			}
		}

		private void WallOffTile(HexWallTable<bool> wallTable, Vector2Int pos) {
			WallOffTile(wallTable, pos.x, pos.y);
		}

		private void WallOffTile(HexWallTable<bool> wallTable, int column, int row) {
			foreach (var dir in HexDirectionUtils.All()) {
				wallTable.Set(column,row, (HexDirection)dir, true);
			}
		}

		private void OpenOneWallOnRing(HexWallTable<bool> wallTable, int column, int row, int radius) {
			if (radius == 0) {
				
				return;
			}
			// Pick some direction, and move out (radius) tiles
			HexDirection startDir = HexDirection.E;
			Vector2Int currentPos = HexUtils.MoveFrom(column, row, startDir, radius);
			// Turn and move perpendicular to the ring
			HexDirection moveDir = startDir.Next2();

			for (int i = 0; i < 6; i++) {
				// Move CCW one tile for (radius) tiles, putting two walls
				for (int t = 0; t < radius - 1; t++) {
					currentPos = HexUtils.MoveFrom(currentPos, moveDir);
					//wallTable.Set(currentPos, moveDir.Last(), true);
					//wallTable.Set(currentPos, moveDir.Last2(), true);
				}
				// TODO: make this better
				wallTable.Set(currentPos, moveDir.Last(), false);
				break;

				// Move into the corner
				currentPos = HexUtils.MoveFrom(currentPos, moveDir);

				//// Corner hexes will need three walls
				//wallTable.Set(currentPos, moveDir, true);
				//wallTable.Set(currentPos, moveDir.Last(), true);
				//wallTable.Set(currentPos, moveDir.Last2(), true);

				// Turn CCW
				moveDir = moveDir.Next();
			}
		}

		/// <summary>
		/// Chooses the most recently added element that is not closed
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		private Vector2Int ChooseFromList( List<Vector2Int> list, HexTable<int> statusTable) {
			for( int i = list.Count -1; i >= 0; i ++ ) {
				Vector2Int result = list[i];
				//if ( statusTable.Get(result) != 2 ) {
					return result;
				//}
			}

			throw new System.Exception("Shouldn't have hit this.");
		}

		[NonSerialized]
		private HexDirection[] _shuffledDirections;

		private IEnumerable<HexDirection> ShuffledDirections() {
			//if (_shuffledDirections==null) {
				_shuffledDirections = new HexDirection[6];
				for (int i = 0; i < 6; i++) {
					_shuffledDirections[i] = (HexDirection)i;
				}
			//}
			
			int n = 6;
			while (n > 1) {
				n--;
				int k = Mathf.FloorToInt( UnityEngine.Random.value * (n + 1) );
				HexDirection value = _shuffledDirections[k];
				_shuffledDirections[k] = _shuffledDirections[n];
				_shuffledDirections[n] = value;
			}

			for( int i = 0; i < 6; i ++ ) {
				yield return _shuffledDirections[i];
			}
		}

		private void GrowingTreeMaze(HexWallTable<bool> wallTable, int column, int row, int radius) {
			// We'll keep track of the status of each tile, for later. All tiles start at zero = open.
			HexTable<int> statusTable = new HexTable<int>();
			// Start an empty list, from which we'll 
			List<Vector2Int> openList = new List<Vector2Int>();

			// Add a random point to the open list
			Vector2Int pos = HexUtils.RandomPointInArea(column, row, radius);
			openList.Add(pos);
			// Set that space's status to 1 = used.
			statusTable.Set(pos, 1);

			while ( openList.Count > 0 ) {
				pos = ChooseFromList(openList, statusTable);

				int direction = -1;
				Vector2Int move = Vector2Int.zero;

				// Attempt each direction, randomly, taking the first one that qualifies
				foreach ( var d in ShuffledDirections() ) {
					// if the next space is open
					move = HexUtils.MoveFrom(pos, d);
					if ( statusTable.Get(move) != 0) {
						continue;
					}
					// if the next space is not outside of our boundaries
					if ( HexUtils.Distance(column, row, move.x, move.y) > radius ) {
						continue;
					}

					// use this direction
					direction = (int)d;
					break;
				}

				// If no new tiles were possible
				if ( direction==-1 ) {
					// declare this position closed and remove it from the list
					openList.Remove(pos);
					
					if ( statusTable.Get(pos) == 1 ) {
						// it is also a dead-end
						// spawn a light stone at this location
						//SpawnManager.NewLightStone();
						Vector3 lightStonePos = hexagonMaker.AxialCoordsToWorldPosition(pos);
						GameObject newStone = GameObject.Instantiate(lightStonePrefab, pickupsFolder.transform);
						float y = hexagonMaker.metrics.XZPositionToHeight(lightStonePos.x, lightStonePos.z, true);
						newStone.transform.position = lightStonePos;
					}
					
				}
				// This is a viable movement
				else {
					// remove the wall between the two spaces
					wallTable.Set(pos, (HexDirection)direction, false);
					// add the new space to the open list and update its status
					openList.Add(move);
					statusTable.Set(move, 1);

					// mark the current position as NOT a dead-end
					statusTable.Set(pos, 2);
				}
			}
		}
		
	}
}