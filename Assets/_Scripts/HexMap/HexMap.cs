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

		public HexTileset tileset;

		public int generateDistance = 15;
		public int destroyDistance = 8;

		//public bool keepPlayerAbove = true;

		//public int spawnWallSteps = 8;

		public GameObject targetObject;

		public GameObject TargetObject { get { return targetObject;  } }

		//public HexTable<TileStatus> tileStatuses = new HexTable<TileStatus>();

		/// <summary>
		/// A volatile list of tile coords that are either waiting to be created or waiting to be destroyed.
		/// When the coord is popped, the actual operation to do is found within the tileStatuses.
		/// </summary>
		public List<Vector2Int> pendingTileModifications = new List<Vector2Int>();

		private HexTable<HexTile> tileTable = new HexTable<HexTile>();

		private HexagonMaker maker;

		//private HexMaze hexMaze;
		
		[SerializeField]
		private HexWallTableBool isWallTable = new HexWallTableBool();

		[SerializeField]
		private HexTable<int> elevationTable = new HexTable<int>();

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

		//private int steps = 0;

		//private bool isBeingDestroyed = false;

		public HexMetrics Metrics {
			get {
				maker = GetComponent<HexagonMaker>();
				return maker.metrics;
			}
		}

		
		public QuickEvent Events { get { return _events; } }
		private QuickEvent _events = new QuickEvent();

		// Use this for initialization
		void Start() {
			//hexMaze = GetComponent<HexMaze>();
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

				foreach( var point in shape.GetWorldPoints()) {
					Vector2Int coords = WorldPositionToAxialCoords(point);

					HexTile tile = tileTable.Get(coords);
					tile?.AddStaticObstacle(shape);
				}
			}
		}

		//public void GenerateStartArea() {
		//	hexMaze = GetComponent<HexMaze>();
		//	maker = GetComponent<HexagonMaker>();
		//	maker.ClearAllTiles();

		//	isWallTable = hexMaze.GetWallTable();

		//	maker.GenerateTilesAt(0,0,Metrics.radius);
		//}

		public void RemakeTiles() {
			//hexMaze = GetComponent<HexMaze>();
			maker = GetComponent<HexagonMaker>();
			maker.ClearAllTiles();

			elevationTable.Clear();

			maker.GenerateTilesAt(0, 0, Metrics.radius);
		}

		//public void GenerateMaze(int column, int row, int radius) {

		//	isWallTable.BeginChangeCheck();
		//	hexMaze.GenerateMaze(isWallTable, column, row, radius);
		//	Events.Fire(MapEvent.WallTableChanged, null);

		//	foreach( var pos in isWallTable.EndChangeCheck() ) {
				
		//		HexTile currentTile;
		//		if ( tileTable.TryGet(pos, out currentTile ) ) {
		//			Destroy(currentTile.gameObject);

		//			HexTile hexTile = maker.GenerateTile(pos.x, pos.y);
		//			hexTile.staticObstacles = currentTile.staticObstacles;

		//			tileTable.Set(pos, hexTile);
		//		}
				
		//		globalNodeTable.Remove(pos);
		//	}
			
		//}

		// Update is called once per frame
		void Update() {

			if (targetObject == null) {
				GameObject player;
				if (!QueryManager.GetPlayer(out player)) {
					return;
				}
				targetObject = player;
			}

			//if (keepPlayerAbove ) {
			//	HexNavMeshManager.EnsureAboveMap(targetObject.transform);
			//}

			playerCoords = maker.WorldPositionToAxialCoords(targetObject.transform.position);

			//Debug.Log(playerCoords);
			
			if (HexUtils.Distance(lastPlayerCoords, playerCoords) > 0) {
				//int x = playerCoords.x - lastPlayerCoords.x;
				//int y = playerCoords.y - lastPlayerCoords.y;
				//// TODO: if the player teleports this function doesn't know what to do
				//HexDirection direction = HexUtils.VectorToDirection(x, y);

				int z = HexUtils.Z(playerCoords.x, playerCoords.y);
				//switch (direction) {
				//	case HexDirection.E:
				RemoveMinColumns();
				AddMaxColumns();

				RemoveMaxZs(z);
				AddMinZs(z);

				//		break;
				//	case HexDirection.NE:
				RemoveMinRows();
				AddMaxRows();

				//		RemoveMaxZs(z);
				//		AddMinZs(z);
				//		break;
				//	case HexDirection.NW:
				//		RemoveMinRows();
				//		AddMaxRows();

				RemoveMaxColumns();
				AddMinColumns();

				//		break;
				//	case HexDirection.W:
				//		RemoveMaxColumns();
				//		AddMinColumns();

				RemoveMinZs(z);
				AddMaxZs(z);

				//		break;
				//	case HexDirection.SW:
				RemoveMaxRows();
				AddMinRows();

				//		RemoveMinZs(z);
				//		AddMaxZs(z);
				//		break;
				//	case HexDirection.SE:
				//		RemoveMinColumns();
				//		AddMaxColumns();
				//		RemoveMaxRows();
				//		AddMinRows();
				//		break;
				//}





			}

			//if (pendingTileModifications.Count > 0) {
			//	Vector2Int coords = pendingTileModifications[0];
			//	pendingTileModifications.RemoveAt(0);
			//	NewTileMaybe(coords.x, coords.y);
			//}

			//// spawn maze
			//steps++;
			//if (steps >= 200) {
			//	steps = 0;
			//	hexMaze.GenerateMaze(isWallTable, 0, 0, 3);
			//	Events.Fire(MapEvent.WallTableChanged, null);

			//	maker.Setup();
			//}

			lastPlayerCoords = playerCoords;
		}

		private void AddRow(int row ) {
			for( int c = minColumn - 1; c <= maxColumn+1; c ++ ) {
				NewTileMaybe(c, row);
				//pendingTileModifications.Add(new Vector2Int(c, row));
			}
		}

		private void AddColumn(int column) {
			for (int r = bottomRow-1; r <= topRow + 1; r++) {
				NewTileMaybe(column, r);
				//pendingTileModifications.Add(new Vector2Int(column, r));
			}
		}

		private void AddZ(int z) {
			for (int r = bottomRow - 1; r <= topRow + 1; r++) {
				NewTileMaybe(-r - z, r);
				//pendingTileModifications.Add(new Vector2Int(-r - z, r));
			}
		}

		private void NewTileMaybe(int column, int row) {
			if ( !tileTable.Contains(column, row) && HexUtils.Distance(column, row, playerCoords.x, playerCoords.y) <= generateDistance + 1) {
				HexTile hexTile = maker.GenerateTile( column, row );
				tileTable.Set(column, row, hexTile);
			}
		}

		private void RemoveMinColumns() {
			while (playerCoords.x - minColumn > destroyDistance) {
				foreach( var tile in tileTable.RemoveColumn(minColumn) ) {
					maker.ReturnTileToPool(tile);
					Destroy(tile.gameObject);
				}
				minColumn++;
			}
		}

		private void RemoveMaxColumns() {
			while (maxColumn - playerCoords.x > destroyDistance) {
				foreach (var tile in tileTable.RemoveColumn(maxColumn)) {
					maker.ReturnTileToPool(tile);
					Destroy(tile.gameObject);
				}
				maxColumn--;
			}
		}

		private void RemoveMinRows() {
			while (playerCoords.y - bottomRow > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(bottomRow)) {
					maker.ReturnTileToPool(tile);
					Destroy(tile.gameObject);
				}
				bottomRow++;
			}
		}

		private void RemoveMaxRows() {
			while (topRow - playerCoords.y > destroyDistance) {
				foreach (var tile in tileTable.RemoveRow(topRow)) {
					maker.ReturnTileToPool(tile);
					Destroy(tile.gameObject);
				}
				topRow--;
			}
		}

		private void RemoveMinZs(int z) {
			while (z - minZ > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(minZ)) {
					maker.ReturnTileToPool(tile);
					Destroy(tile.gameObject);
				}
				minZ++;
			}
		}

		private void RemoveMaxZs(int z) {
			while (maxZ - z > destroyDistance) {
				foreach (var tile in tileTable.RemoveZ(maxZ)) {
					maker.ReturnTileToPool(tile);
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
			int elevationA = GetElevationAt(column, row);

			int elevationB = GetElevationAt(HexUtils.MoveFrom(column, row, direction));

			int difference = Mathf.Abs(elevationB - elevationA);

			return difference > 2;

			//if (isWallTable == null) {
			//	return false;
			//}
			//return isWallTable.Get(column, row, direction);
		}

		public bool IsWallAt(Vector2Int pos, HexDirection direction) {
			return (IsWallAt(pos.x, pos.y, direction));
		}

		public int GetElevationAt(Vector2Int pos) {
			return GetElevationAt(pos.x, pos.y);
		}

		public int GetElevationAt( int column, int row ) {
			int elevation;
			if (elevationTable.TryGet(column,row, out elevation ) ) {
				return elevation;
			}
			var point = HexUtils.PositionFromCoordinates(column, row, 1f);
			float noise = HexUtils.GetHeightOnNoiseMap(Metrics.elevationNoiseMap,
				point.JustXZ() * Metrics.elevationNoiseMapResolution );
			elevation = Mathf.FloorToInt( noise * Metrics.maxElevation );
			//elevation = UnityEngine.Random.Range(0, Metrics.maxElevation );
			elevationTable.Set(column, row, elevation);
			return elevation;
		}

		public Vector3 AxialCoordsToWorldPosition(Vector2Int pos) {
			return HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
		}

		public Vector3 AxialCoordsToWorldPositionWithHeight(Vector2Int pos) {
			Vector3 result = HexUtils.PositionFromCoordinates(pos.x, pos.y, maker.metrics.tileSize);
			return result + Vector3.up * maker.metrics.XZPositionToHeight(result,true);
		}

		public float XZPositionToHeight(Vector3 position, bool scaleByMapHeight = false) {
			return maker.metrics.XZPositionToHeight(position, scaleByMapHeight);
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
			return HexUtils.WorldPositionToAxialCoords(position, Metrics);
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
		
		public GlobalNode GetGlobalNodeFromWorldPosition(Vector3 pos) {
			Vector2Int coords = WorldPositionToAxialCoords(pos);
			return GetGlobalNode(coords);
		}

		/// <summary>
		/// Caches and calculates the node information
		/// </summary>
		public GlobalNode GetGlobalNode( Vector2Int pos ) {
			GlobalNode result;
			if (!globalNodeTable.TryGet(pos, out result)) {
				result = CreateNewGlobalNode(pos);
				globalNodeTable.Set(pos, result);
			}
			return result;
		}
		
		private GlobalNode CreateNewGlobalNode( Vector2Int pos ) {
			GlobalNode newNode = new GlobalNode(pos, AxialCoordsToWorldPosition(pos) );
			// Check each other edge and create unidirectional connections between all applicable
			foreach ( var dir in HexDirectionUtils.All() ) {
				// don't connect to any walls
				if (IsWallAt(pos, dir)) {
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
				//throw new SystemException("TODO");
				return null;
			}
			
			// if this is the first time we've done local pathing on this tile then its
			// nav mesh hasn't been built yet
			if (!tile.navChunk.NavMeshBuilt) {
				tile.BuildNavMesh();
			}

			// if the tile has no triangles then there's not much we can do
			if (tile.navChunk.triangles == null || tile.navChunk.triangles.Count == 0) {
				//throw new SystemException("TODO");
				return null;
			}

			float min = -1;
			DelaunayTriangle closestTriangle = null;

			// get the triangle on that tile that contains the given point
			foreach (var triangle in tile.navChunk.triangles) {
				var point = new Poly2Tri.TriangulationPoint(pos.x, pos.z).AsVector2();
				if (ColDet.PointInTriangle(triangle, point)) {
					return GetLocalNodeByTriangle(triangle);
				}
				else {
					// Next best option is to come back with the triangle closest to the given point
					// we can do this by finding the closest point on the triangle to the given point
					float distance = ColDet.DistanceToClosestPointOnTriangle(triangle, point);

					if (closestTriangle == null || distance < min) {
						closestTriangle = triangle;
						min = distance;
					}
				}
			}

			if (closestTriangle == null) {
				throw new SystemException("Closest Triangle was null");
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

		public void NotifyOfStaticObstacleAdd(GameObject newObstacle) {
			//var worldXZPosition = newObstacle.transform.position;
			//// Get the tile that contains that point
			//Vector2Int coords = WorldPositionToAxialCoords(worldXZPosition);
			//HexTile tile = tileTable.Get(coords);
			//// if the tile is null or not found then there's not much we can do
			//if (tile == null) {
			//	//throw new SystemException("TODO");
			//	return;
			//}
			//tile.NavMeshBuilt = false;

			PolyShape shape = newObstacle.GetComponentInChildren<PolyShape>();

			foreach (var point in shape.GetWorldPoints()) {
				Vector2Int coords = WorldPositionToAxialCoords(point);

				HexTile tile = tileTable.Get(coords);
				tile?.AddStaticObstacle(shape);
			}
		}

		public void NotifyOfStaticObstacleRemove(GameObject newObstacle) {
			//var worldXZPosition = newObstacle.transform.position;
			//// Get the tile that contains that point
			//Vector2Int coords = WorldPositionToAxialCoords(worldXZPosition);
			//HexTile tile = tileTable.Get(coords);
			//// if the tile is null or not found then there's not much we can do
			//if (tile == null) {
			//	//throw new SystemException("TODO");
			//	return;
			//}
			//tile.NavMeshBuilt = false;

			PolyShape shape = newObstacle.GetComponentInChildren<PolyShape>();

			foreach (var point in shape.GetWorldPoints()) {
				Vector2Int coords = WorldPositionToAxialCoords(point);

				HexTile tile = tileTable.Get(coords);
				tile?.RemoveStaticObstacle(shape);
			}
		}
	}

	public enum MapEvent {
		WallTableChanged = 2,
	}

	public enum TileStatus {
		DoesNotExist,
		Exists,
		WaitingToBeCreated,
		WaitingToBeDestroyed
	}
}