using HexMap.Pathfinding;
using Poly2Tri;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HexMap {
	[Serializable]
	public class HexWallTableBool : HexWallTable<bool> { }

	[RequireComponent(typeof(HexagonMaker))]
	public class HexMap : MonoBehaviour {

		public int generateDistance = 15;
		public int destroyDistance = 8;

		public bool keepPlayerAbove = true;

		private HexTable<HexTile> tileTable = new HexTable<HexTile>();

		private HexagonMaker maker;

		private HexMaze hexMaze;

		

		[SerializeField]
		private HexWallTableBool isWallTable = new HexWallTableBool();

		private HexTable<GlobalNode> globalNodeTable = new HexTable<GlobalNode>();

		private Dictionary<DelaunayTriangle, LocalNode> localNodeDict = new Dictionary<DelaunayTriangle, LocalNode>();

		private Vector2Int lastPlayerCoords;
		private Vector2Int playerCoords;

		private int topRow = 0;
		private int bottomRow = 0;
		private int minColumn = 0;
		private int maxColumn = 0;
		private int minZ = 0;
		private int maxZ = 0;

		public HexMetrics Metrics {
			get { return maker.metrics;  }
		}

		// Use this for initialization
		void Start() {
			maker = GetComponent<HexagonMaker>();
			GameObject player;
			if (QueryManager.GetPlayer(out player)) {
				lastPlayerCoords = maker.WorldPositionToAxialCoords(player.transform.position);
			}

			FillHexTileTable();
			ProcessAllObstacles();
			
		}

		public void FillHexTileTable() {
			tileTable.Clear();

			// Something about how it serializes doesn't keep the tileTable in the HexagonMaker stable between edit mode and play mode
			// So let's waste a compounding amount of effort recursivally rebuilding it on startup
			foreach (var tile in GetComponentsInChildren<HexTile>()) {
				tile.map = this;
				tileTable.Set(tile.column, tile.row, tile);
				topRow = topRow > tile.row ? topRow : tile.row;
				bottomRow = bottomRow < tile.row ? bottomRow : tile.row;
				maxColumn = maxColumn > tile.column ? maxColumn : tile.column;
				minColumn = minColumn < tile.column ? minColumn : tile.column;
				int z = HexUtils.Z(tile.column, tile.row);
				maxZ = maxZ > z ? maxZ : z;
				minZ = minZ < z ? minZ : z;
			}
		}

		public void ProcessAllObstacles() {
			GameObject[] obstacles = QueryManager.GetAllStaticObstacles();

			foreach (var obstacle in obstacles) {
				PolyShape shape = obstacle.GetComponentInChildren<PolyShape>();

				Vector2Int coords = WorldPositionToAxialCoords(obstacle.transform.position);

				HexTile tile = tileTable.Get(coords);
				tile?.AddStaticObstacle(shape);
			}
		}

		public void MakeMaze() {
			hexMaze = GetComponent<HexMaze>();
			maker = GetComponent<HexagonMaker>();
			maker.Clear();

			isWallTable = hexMaze.GetWallTable();

			
			//maker.SetMapTable(isWallTable);
			maker.Setup();
		}

		// Update is called once per frame
		void Update() {
			GameObject player;
			if ( !QueryManager.GetPlayer( out player ) ) {
				return;
			}

			if (keepPlayerAbove ) {
				float leastY = maker.metrics.XZPositionToHeight(player.transform.position) * maker.metrics.mapHeight;
				if ( player.transform.position.y < leastY - 10f ) {
					player.transform.position = new Vector3(player.transform.position.x, leastY + 2, player.transform.position.z);
					Rigidbody myBody = player.GetComponent<Rigidbody>();
					myBody.velocity = new Vector3(myBody.velocity.x, 0, myBody.velocity.z);
				}
			}

			playerCoords = maker.WorldPositionToAxialCoords(player.transform.position);

			//Debug.Log(playerCoords);

			

			if (HexUtils.Distance(lastPlayerCoords, playerCoords) > 0) {
				int x = playerCoords.x - lastPlayerCoords.x;
				int y = playerCoords.y - lastPlayerCoords.y;
				// TODO: if the player teleports this function doesn't know what to do
				HexDirection direction = HexUtils.VectorToDirection(x, y);

				int z = HexUtils.Z(playerCoords.x, playerCoords.y);
				switch (direction) {
					case HexDirection.E:
						RemoveMinColumns();
						AddMaxColumns();

						RemoveMaxZs(z);
						AddMinZs(z);

						break;
					case HexDirection.NE:
						RemoveMinRows();
						AddMaxRows();

						RemoveMaxZs(z);
						AddMinZs(z);
						break;
					case HexDirection.NW:
						RemoveMinRows();
						AddMaxRows();

						RemoveMaxColumns();
						AddMinColumns();

						break;
					case HexDirection.W:
						RemoveMaxColumns();
						AddMinColumns();

						RemoveMinZs(z);
						AddMaxZs(z);

						break;
					case HexDirection.SW:
						RemoveMaxRows();
						AddMinRows();

						RemoveMinZs(z);
						AddMaxZs(z);
						break;
					case HexDirection.SE:
						RemoveMinColumns();
						AddMaxColumns();
						RemoveMaxRows();
						AddMinRows();
						break;
				}



			}

			lastPlayerCoords = playerCoords;
		}

		private void AddRow(int row ) {
			for( int c = minColumn - 1; c <= maxColumn+1; c ++ ) {
				NewTileMaybe(c, row);
			}
		}

		private void AddColumn(int column) {
			for (int r = bottomRow-1; r <= topRow + 1; r++) {
				NewTileMaybe( column, r);
			}
		}

		private void AddZ(int z) {
			for (int r = bottomRow - 1; r <= topRow + 1; r++) {
				NewTileMaybe(-r-z, r);
			}
		}

		private void NewTileMaybe(int column, int row) {
			if ( !tileTable.Contains(column, row) && HexUtils.Distance(column, row, playerCoords.x, playerCoords.y) <= generateDistance + 1) {
				HexTile hexTile = maker.NewHexTile(column, row, tileTable);
				
			}
		}

		private void RemoveMinColumns() {
			while (playerCoords.x - minColumn > destroyDistance) {
				foreach( var tile in tileTable.RemoveColumn(minColumn) ) {
					Destroy(tile.gameObject);
				}
				minColumn++;
			}
		}

		private void RemoveMaxColumns() {
			while (maxColumn - playerCoords.x > destroyDistance) {
				foreach (var tile in tileTable.RemoveColumn(maxColumn)) {
					Destroy(tile.gameObject);
				}
				maxColumn--;
			}
		}

		private void RemoveMinRows() {
			while (playerCoords.y - bottomRow > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(bottomRow)) {
					Destroy(tile.gameObject);
				}
				bottomRow++;
			}
		}

		private void RemoveMaxRows() {
			while (topRow - playerCoords.y > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(topRow)) {
					Destroy(tile.gameObject);
				}
				topRow--;
			}
		}

		private void RemoveMinZs(int z) {
			while (z - minZ > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(minZ)) {
					Destroy(tile.gameObject);
				}
				minZ++;
			}
		}

		private void RemoveMaxZs(int z) {
			while (maxZ - z > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(maxZ)) {
					Destroy(tile.gameObject);
				}
				maxZ--;
			}
		}


		private void AddMinColumns() {
			while (playerCoords.x - minColumn < generateDistance) {
				AddColumn(--minColumn);
			}
		}

		private void AddMaxColumns() {
			while (maxColumn - playerCoords.x < generateDistance) {
				AddColumn(++maxColumn);
			}
		}

		private void AddMinRows() {
			while (playerCoords.y - bottomRow < generateDistance) {
				AddRow(--bottomRow);
			}
		}

		private void AddMaxRows() {
			while (topRow - playerCoords.y < generateDistance) {
				AddRow(++topRow);
			}
		}

		private void AddMinZs(int z) {
			while (z - minZ < generateDistance) {
				AddZ(--minZ);
			}
		}
		
		private void AddMaxZs(int z) {
			while (maxZ - z < generateDistance) {
				AddZ(++maxZ);
			}
		}


		#region Map Properties Methods

		public bool IsWallAt(int column, int row, HexDirection direction) {
			if (isWallTable == null) {
				return false;
			}
			return isWallTable.Get(column, row, direction);
		}

		public bool IsWallAt(Vector2Int pos, HexDirection direction) {
			return (IsWallAt(pos.x, pos.y, direction));
		}

		public Vector3 AxialCoordsToWorldPosition(Vector2Int pos) {
			return HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
		}

		public Vector3 AxialCoordsToWorldPositionWithHeight(Vector2Int pos) {
			Vector3 result = HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
			return result + Vector3.up * maker.metrics.XZPositionToHeight(result,true);
		}

		public HexTile GetTile( int column, int row ) {
			return tileTable.Get(column, row);
		}

		public HexTile GetTile( Vector2Int pos ) {
			return tileTable.Get(pos);
		}

		/// <summary>
		/// Takes the world position and returns the tile's axial coordinates that contains this position
		/// relative to the map's tile size and scale.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector2Int WorldPositionToAxialCoords(Vector3 position) {
			position = transform.InverseTransformPoint(position);

			float q = ((float)Mathf.Sqrt(3) / 3f * position.x - 1f / 3f * position.z) / maker.metrics.tileSize;
			float r = (2f / 3f * position.z) / maker.metrics.tileSize;

			int rx = Mathf.RoundToInt(q);
			int ry = Mathf.RoundToInt(r);

			float xDiff = Mathf.Abs(q - rx);
			float yDiff = Mathf.Abs(r - ry);

			if (xDiff > yDiff) {
				rx = -ry - Mathf.RoundToInt(-q - r);
			} else {
				ry = -rx - Mathf.RoundToInt(-q - r);
			}

			return new Vector2Int(rx, ry);
		}

		#endregion

		#region Global Pathfinding

		public struct NodeAndDirection {
			public GlobalNode node;
			public HexDirection direction;
			public NodeAndDirection( GlobalNode node, HexDirection direction ) {
				this.node = node;
				this.direction = direction;
			}
		}

		/// <summary>
		/// Caches and calculates the node information
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		public GlobalNode GetGlobalNode( Vector2Int pos ) {
			GlobalNode result;
			if (!globalNodeTable.TryGet(pos, out result)) {
				result = CreateNewGlobalNode(pos);
				globalNodeTable.Set(pos, result);
			}
			return result;
		}
		
		private GlobalNode CreateNewGlobalNode( Vector2Int pos ) {
			GlobalNode newNode = new GlobalNode(pos);
			// Check each other edge and create unidirectional connections between all applicable
			foreach ( var dir in HexDirectionUtils.All() ) {
				// don't connect to any walls
				if ( IsWallAt( pos, dir)) {
					continue;
				}
				// create a new edge
				Vector2Int move = HexUtils.MoveFrom(pos, dir);
				GlobalEdge newEdge = new GlobalEdge(newNode.Position, move);
				newNode.edges.Add(newEdge);
			}
			
			return newNode;
		}

		public LocalNode GetLocalNodeByPosition(Vector3 pos) {
			// Get the tile that contains that point
			Vector2Int coords = WorldPositionToAxialCoords(pos);
			HexTile tile = tileTable.Get(coords);
			// if the tile is null or not found then there's not much we can do
			if ( tile == null ) {
				throw new SystemException("TODO");
			}
			// if this is the first time we've done local pathing on this tile then its
			// nav mesh hasn't been built yet
			if ( !tile.NavMeshBuilt ) {
				tile.BuildNavMesh();
			}

			// if the tile has no triangles then there's not much we can do
			if ( tile.triangles == null || tile.triangles.Count==0) {
				throw new SystemException("TODO");
			}

			float min = -1;
			DelaunayTriangle closestTriangle = null;

			// get the triangle on that tile that contains the given point
			foreach( var triangle in tile.triangles ) {
				var point = new Poly2Tri.TriangulationPoint(pos.x, pos.z).AsVector2();
				if (ColDet.PointInTriangle( triangle, point) ) {
					return GetLocalNodeByTriangle(triangle);
				} else {
					// Next best option is to come back with the triangle closest to the given point
					// we can do this by finding the closest point on the triangle to the given point
					float distance = ColDet.DistanceToClosestPointOnTriangle(triangle, point);

					if (closestTriangle == null || distance < min) {
						closestTriangle = triangle;
						min = distance;
					}
				}
			}

			// we didn't outright find a triangle with our point in it but we can return the closest one
			return GetLocalNodeByTriangle(closestTriangle);
		}

		public LocalNode GetLocalNodeByTriangle(DelaunayTriangle triangle) {
			LocalNode node;
			if (!localNodeDict.TryGetValue(triangle, out node)) {
				node = new LocalNode(triangle);
				localNodeDict[triangle] = node;
				// the edges will be generated when they're needed
			}
			return node;
		}

		public void ResetLocalNodesEdgesMaybe(DelaunayTriangle triangle) {
			LocalNode node;
			if ( localNodeDict.TryGetValue(triangle, out node)) {
				node.edges = null;
				// the edges will be regenerated when they're needed
			}
		}

		#endregion
	}
}