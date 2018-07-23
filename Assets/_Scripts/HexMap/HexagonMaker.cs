//using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HexMap {
	/// <summary>
	/// Creates a single pointy-topped hex tile.
	/// https://www.redblobgames.com/grids/hexagons/
	/// 
	/// </summary>
	public class HexagonMaker : MonoBehaviour {

		public HexMetrics metrics;

		public Material material;

		public List<PolyShape> obstacles = new List<PolyShape>();

		private HexTable<HexTile> tileTable = new HexTable<HexTile>();
		
		private HexMap hexMap;

		private MeshBuilder meshBuilder;

		private GameObject tilesFolder;

		private void Start() {
			meshBuilder = new MeshBuilder();
			hexMap = GetComponent<HexMap>();
			tilesFolder = ObjectFactory.Folder("Tiles", transform);
		}
		public void Clear() {
			
			CommonUtils.DestroyChildren(transform);
			tileTable.Clear();
			//hexWallTable.Clear();
			//isWallTable.Clear();
		}

		public void Setup() {
			hexMap = GetComponent<HexMap>();
			meshBuilder = new MeshBuilder();

			tilesFolder = ObjectFactory.Folder("Tiles", transform);
			foreach ( var pos in HexUtils.ForEachTileWithin(0,0,metrics.radius ) ) {
				HexTile hexTile = NewHexTile(pos.x, pos.y, tileTable);
			}
			
		}
		
		public HexTile NewHexTile(int column, int row, HexTable<HexTile> tileTable) {

			Vector3 pos = HexUtils.PositionFromCoordinates(column, row, metrics.tileSize);

			Mesh hexMesh;

			hexMesh = meshBuilder.WallsHexagon( metrics, hexMap, column, row);

			//if ( IsWallAt( column, row ) ) {
			//	hexMesh = MountainHexagon(tileSize, column, row );
			//} else {
			//	hexMesh = Hexagon(tileSize, pos);
			//}
			
			GameObject tile = NewMeshedObject("Tile", tilesFolder.transform, hexMesh, material);
			tile.layer = gameObject.layer;

			HexTile hexTile = tile.AddComponent<HexTile>();
			hexTile.column = column;
			hexTile.row = row;
			hexTile.metrics = metrics;
			hexTile.map = hexMap;
			
			tile.transform.localPosition = pos;

			hexTile.staticObstacles.AddRange(obstacles);

			hexTile.BuildNavMesh();

			tileTable.Set(column, row, hexTile);

			return hexTile;
		}
		

		/// <summary>
		/// Takes the world position and returns the tile's axial coordinates that contains this position
		/// relative to the map's tile size and scale.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public Vector2Int WorldPositionToAxialCoords(Vector3 position) {
			position = transform.InverseTransformPoint(position);

			float q = ((float)Mathf.Sqrt(3)/3f * position.x - 1f/3f * position.z) / metrics.tileSize;
			float r = (2f/3f * position.z) / metrics.tileSize;

			int rx = Mathf.RoundToInt(q);
			int ry = Mathf.RoundToInt(r);

			float xDiff = Mathf.Abs(q - rx);
			float yDiff = Mathf.Abs(r - ry);

			if ( xDiff > yDiff ) {
				rx = -ry - Mathf.RoundToInt( -q -r );
			} else {
				ry = -rx - Mathf.RoundToInt( -q - r);
			}

			return new Vector2Int(rx,ry);
		}

		public Vector3 AxialCoordsToWorldPosition(Vector2Int pos ) {
			return HexUtils.PositionFromCoordinates(pos.x, pos.y, metrics.tileSize);
		}
		
		private GameObject NewMeshedObject(string name, Transform parent, Mesh mesh, Material material = null) {
			GameObject obj = new GameObject(name);
			obj.transform.SetParent(parent);

			MeshFilter filter = obj.AddComponent<MeshFilter>();
			filter.mesh = mesh;
			MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
			if (material != null) {
				renderer.material = material;
			}
			MeshCollider collider = obj.AddComponent<MeshCollider>();
			collider.sharedMesh = mesh;

			return obj;
		}
	}
}