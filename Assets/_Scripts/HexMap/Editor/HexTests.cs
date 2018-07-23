using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Pathfinding;

namespace HexMap {
	public class HexTests {

		private static HexTile NewTile(int column, int row) {
			HexTile tile = new HexTile();
			tile.column = column;
			tile.row = row;
			return tile;
		}

		[Test]
		public void HashTableTest1() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			HexTile tile = NewTile(0, 0);
			table.Set(tile.column, tile.row, tile);
			Assert.AreEqual(table.Count, 1);
		}

		[Test]
		public void HashTableTest2() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			int count = 0;

			for (int c = -10; c < 11; c++) {
				for (int r = -20; r < 40; r++) {
					HexTile tile = NewTile(c, r);
					table.Set(tile.column, tile.row, tile);
					count++;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsTrue(table.TryGet(c, r, out getTile));
					Assert.AreEqual(tile, getTile);
				}
			}

		}

		[Test]
		public void HashTableTest3() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			int count = 0;

			for (int c = -10; c < 11; c++) {
				for (int r = -20; r < 4; r++) {
					HexTile tile = NewTile(c, r);
					table.Set(tile.column, tile.row, tile);
					count++;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsTrue(table.TryGet(c, r, out getTile));
					Assert.AreEqual(tile, getTile);
				}
			}

			for (int c = -10; c < 11; c++) {
				for (int r = -20; r < 4; r++) {
					table.Remove(c, r);
					count--;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsFalse(table.TryGet(c, r, out getTile));
				}
			}

			Assert.AreEqual(table.Count, 0);
		}

		[Test]
		public void HashTableTest4() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			int count = 0;

			for (int c = -2; c < 2; c++) {
				for (int r = -2; r < 2; r++) {
					HexTile tile = NewTile(c, r);
					table.Set(tile.column, tile.row, tile);
					count++;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsTrue(table.TryGet(c, r, out getTile));
					Assert.AreEqual(tile, getTile);
				}
			}

			count = 0;

			foreach (var result in table.GetAllNeighbors(0, 0)) {
				count++;
			}

			Assert.AreEqual(count, 6);
		}

		[Test]
		public void HashTableTest5() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			int count = 0;

			int rows = 0;
			int columns = 0;
			int z = 0;

			int ROW_NUMBER = 3;
			int COLUMN_NUMBER = -1;
			int Z_NUMBER = 0;

			for (int c = -10; c < 11; c++) {
				for (int r = -20; r < 4; r++) {
					HexTile tile = NewTile(c, r);
					table.Set(tile.column, tile.row, tile);
					count++;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsTrue(table.TryGet(c, r, out getTile));
					Assert.AreEqual(tile, getTile);

					if ( r == ROW_NUMBER) {
						rows++;
					}
					if ( c == COLUMN_NUMBER) {
						columns++;
					}
					if ( HexUtils.Z(c,r) == Z_NUMBER) {
						z++;
					}
				}
			}

			List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInColumn(COLUMN_NUMBER));
			Assert.AreEqual(tiles.Count, columns);

			tiles = new List<HexTile>(table.GetAllTilesInRow(ROW_NUMBER));
			Assert.AreEqual(tiles.Count, rows);

			tiles = new List<HexTile>(table.GetAllTilesInZ(Z_NUMBER));
			Assert.AreEqual(tiles.Count, z);



			foreach( var tile in table.RemoveRow( ROW_NUMBER ) ) {
				rows--;
				if ( tile.column == COLUMN_NUMBER ) {
					columns--;
				}
				if (HexUtils.Z(tile.column, tile.row) == Z_NUMBER) {
					z--;
				}
			}

			Assert.AreEqual(rows, 0);

			tiles = new List<HexTile>(table.GetAllTilesInRow(ROW_NUMBER));
			Assert.AreEqual(tiles.Count, rows);
			Assert.AreEqual(tiles.Count, 0);


			foreach (var tile in table.RemoveColumn(COLUMN_NUMBER)) {
				columns--;
				if (HexUtils.Z(tile.column, tile.row) == Z_NUMBER) {
					z--;
				}
			}

			Assert.AreEqual(columns, 0);

			tiles = new List<HexTile>(table.GetAllTilesInColumn(COLUMN_NUMBER));
			Assert.AreEqual(tiles.Count, columns);
			Assert.AreEqual(tiles.Count, 0);


			foreach (var tile in table.RemoveZ(Z_NUMBER)) {
				z--;
			}

			Assert.AreEqual(z, 0);

			tiles = new List<HexTile>(table.GetAllTilesInZ(Z_NUMBER));
			Assert.AreEqual(tiles.Count, z);
			Assert.AreEqual(tiles.Count, 0);
		}

		[Test]
		public void HashTableTest6() {
			HexTable<HexTile> table = new HexTable<HexTile>();
			Assert.AreEqual(table.Count, 0);

			int count = 0;

			int min_c = -20;
			int max_c = 21;

			int[] columnCounts = new int[max_c - min_c];

			for (int c = min_c; c < max_c; c++) {
				for (int r = -20; r < 4; r++) {
					HexTile tile = NewTile(c, r);
					table.Set(tile.column, tile.row, tile);
					count++;
					Assert.AreEqual(table.Count, count);
					HexTile getTile;
					Assert.IsTrue(table.TryGet(c, r, out getTile));
					Assert.AreEqual(tile, getTile);
					columnCounts[c + max_c -1 ]++;
				}
			}

			for (int c = min_c; c < max_c; c++) {
				List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInColumn(c));
				Assert.AreEqual(columnCounts[c + max_c - 1], tiles.Count);
			}

			for (int z = -2; z < 3; z ++ ) {
				List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInZ(z));
				Assert.IsTrue(tiles.Count > 0);

				foreach (var tile in table.RemoveZ(z)) {
					columnCounts[tile.column + max_c - 1]--;
					Assert.AreEqual(HexUtils.Z(tile.column,tile.row), z);
					count--;
				}

				tiles = new List<HexTile>(table.GetAllTilesInZ(z));
				Assert.AreEqual(0, tiles.Count);
			}

			for (int c = min_c; c < max_c; c++) {
				List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInColumn(c));
				Assert.AreEqual(columnCounts[c + max_c - 1], tiles.Count);
			}

			Assert.AreEqual(table.Count, count);
		}


		//[Test]
		//public void HashTableTest7() {
		//	HexTable<HexTile> table = new HexTable<HexTile>();
		//	Assert.AreEqual(table.Count, 0);

		//	int count = 0;

		//	int min_c = -10;
		//	int max_c = 11;

		//	int[] columnCounts = new int[max_c - min_c];

		//	for (int c = min_c; c < max_c; c++) {
		//		for (int r = -20; r < 4; r++) {
		//			HexTile tile = NewTile(c, r);
		//			table.Add(tile.column, tile.row, tile);
		//			count++;
		//			Assert.AreEqual(table.Count, count);
		//			HexTile getTile;
		//			Assert.IsTrue(table.TryGet(c, r, out getTile));
		//			Assert.AreEqual(tile, getTile);
		//			columnCounts[c + max_c - 1]++;
		//		}
		//	}

		//	for (int c = min_c; c < max_c; c++) {
		//		List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInColumn(c));
		//		Assert.AreEqual(columnCounts[c + max_c - 1], tiles.Count);
		//	}

		//	for (int z = -2; z < 3; z++) {
		//		List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInZ(z));
		//		Assert.IsTrue(tiles.Count > 0);

		//		foreach (var tile in table.RemoveZ(z)) {
		//			columnCounts[tile.column + max_c - 1]--;
		//			count--;
		//		}

		//		tiles = new List<HexTile>(table.GetAllTilesInZ(z));
		//		Assert.AreEqual(0, tiles.Count);
		//	}

		//	for (int c = min_c; c < max_c; c++) {
		//		List<HexTile> tiles = new List<HexTile>(table.GetAllTilesInColumn(c));
		//		Assert.AreEqual(columnCounts[c + max_c - 1], tiles.Count);
		//	}

		//	Assert.AreEqual(table.Count, count);
		//}

		//[Test]
		//public void HexMesh1() {
		//	Mesh mesh = MeshBuilder.Hexagon(1, false, false);

		//	Vector3[] vertices = mesh.vertices;

		//	Assert.AreEqual(vertices.Length, 3 * 6);

		//	foreach (HexCorner corner in Enum.GetValues(typeof(HexCorner))) {
		//		// The first point of the first triangle added (SE)
		//		Vector3 point = vertices[(int)corner * 3 + 0];
		//		// should be the origin
		//		Assert.AreEqual(point, Vector3.zero);
		//		// next should be the next CCW corner
		//		point = vertices[(int)corner * 3 + 1];
		//		Assert.AreEqual(point, HexUtils.CornerPosition((int)corner + 1));
		//		// next should be the given corner
		//		point = vertices[(int)corner * 3 + 2];
		//		Assert.AreEqual(point, HexUtils.CornerPosition((int)corner));
		//	}


		//}

		[Test]
		public void HexCorner1() {
			Assert.AreEqual(HexCorner.SE.Next(), HexCorner.NE);
			Assert.AreEqual(HexCorner.NE.Next(), HexCorner.N);
			Assert.AreEqual(HexCorner.N.Next(), HexCorner.NW);
			Assert.AreEqual(HexCorner.NW.Next(), HexCorner.SW);
			Assert.AreEqual(HexCorner.SW.Next(), HexCorner.S);
			Assert.AreEqual(HexCorner.S.Next(), HexCorner.SE);
		}

		[Test]
		public void HexCorner2() {
			Assert.AreEqual(HexDirection.E.GetCorner(), HexCorner.SE);
			Assert.AreEqual(HexDirection.NE.GetCorner(), HexCorner.NE);
			Assert.AreEqual(HexDirection.NW.GetCorner(), HexCorner.N);
			Assert.AreEqual(HexDirection.W.GetCorner(), HexCorner.NW);
			Assert.AreEqual(HexDirection.SW.GetCorner(), HexCorner.SW);
			Assert.AreEqual(HexDirection.SE.GetCorner(), HexCorner.S);
		}

		[Test]
		public void HexDirection1() {
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.E), new Vector2Int(1, 0));
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.NE), new Vector2Int(0, 1));
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.NW), new Vector2Int(-1, 1));
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.W), new Vector2Int(-1, 0));
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.SW), new Vector2Int(0, -1));
			Assert.AreEqual(HexUtils.MoveFrom(0, 0, HexDirection.SE), new Vector2Int(1, -1));

		}

		[Test]
		public void HexDirection2() {
			Assert.AreEqual(HexUtils.VectorToDirection(1, 0), HexDirection.E);
			Assert.AreEqual(HexUtils.VectorToDirection(0, 1), HexDirection.NE);
			Assert.AreEqual(HexUtils.VectorToDirection(-1, 1), HexDirection.NW);
			Assert.AreEqual(HexUtils.VectorToDirection(-1, 0), HexDirection.W);
			Assert.AreEqual(HexUtils.VectorToDirection(0, -1), HexDirection.SW);
			Assert.AreEqual(HexUtils.VectorToDirection(1, -1), HexDirection.SE);

		}

		[Test]
		public void HexDirection3() {
			Assert.AreEqual(HexDirection.E.Last2(), HexDirection.SW);
			Assert.AreEqual(HexDirection.NE.Last2(), HexDirection.SE);
			Assert.AreEqual(HexDirection.NW.Last2(), HexDirection.E);
			Assert.AreEqual(HexDirection.W.Last2(), HexDirection.NE);
			Assert.AreEqual(HexDirection.SW.Last2(), HexDirection.NW);
			Assert.AreEqual(HexDirection.SE.Last2(), HexDirection.W);

		}

		[Test]
		public void HexDistance1() {
			Assert.AreEqual(1, HexUtils.Distance(0, 1));
			Assert.AreEqual(2, HexUtils.Distance(2, 0));
			Assert.AreEqual(5, HexUtils.Distance(1, 4));

		}

		[Test]
		public void HexDistance2() {
			Assert.AreEqual(1, HexUtils.Distance(0, 0, 0, 1));
			Assert.AreEqual(2, HexUtils.Distance(0, 0, 2, 0));
			Assert.AreEqual(5, HexUtils.Distance(0, 4, -3, 2));

		}

		[Test]
		public void HexWallTable1() {
			HexWallTable<bool> table = new HexWallTable<bool>();
			
			foreach ( var direction in HexDirectionUtils.All() ) {
				Assert.IsFalse(table.Get(0, 0, direction));
				table.Set(0, 0, direction, true);
				Assert.IsTrue(table.Get(0, 0, direction));
			}

			foreach(var direction in HexDirectionUtils.All()) {
				Assert.IsTrue(table.Get(0, 0, direction));
				table.Set(0, 0, direction, false);
				Assert.IsFalse(table.Get(0, 0, direction));
			}

			foreach (var direction in HexDirectionUtils.All()) {
				Vector2Int move = HexUtils.MoveFrom(0, 0, direction);
				Assert.IsFalse(table.Get(move, direction.Opposite()));
				table.Set(move, direction.Opposite(), true);
				Assert.IsTrue(table.Get(move, direction.Opposite()));
				Assert.IsTrue(table.Get(0, 0, direction));
			}
			
		}

		[Test]
		public void HexWallTable2() {
			HexWallTable<bool> table = new HexWallTable<bool>();

			for ( int r = -5; r < 6; r ++ ) {
				for (int c = -3; c < 4; c ++ ) {
					foreach (var direction in HexDirectionUtils.All()) {
						Assert.IsFalse(table.Get(c, r, direction));
						table.Set(c, r, direction, true);
						Assert.IsTrue(table.Get(c, r, direction));
					}

					foreach (var direction in HexDirectionUtils.All()) {
						Assert.IsTrue(table.Get(c, r, direction));
						table.Set(c, r, direction, false);
						Assert.IsFalse(table.Get(c, r, direction));
					}

					foreach (var direction in HexDirectionUtils.All()) {
						Vector2Int move = HexUtils.MoveFrom(c, r, direction);
						Assert.IsFalse(table.Get(move, direction.Opposite()));
						table.Set(move, direction.Opposite(), true);
						Assert.IsTrue(table.Get(move, direction.Opposite()));
						Assert.IsTrue(table.Get(c, r, direction));
						table.Set(move, direction.Opposite(), false);
						Assert.IsFalse(table.Get(move, direction.Opposite()));
					}
				}
			}
		}

		[Test]
		public void HexWallTable3() {
			HexWallTable<bool> table = new HexWallTable<bool>();

			table.Set(1, 0, HexDirection.NW, true);

			Vector2Int move = HexUtils.MoveFrom(0, 0, HexDirection.NE);
			Assert.IsTrue(table.Get(move, HexDirection.NE.Last2()));
			
		}

		//[Test]
		//public void WorkSet1() {
		//	WorkSet set = new WorkSet();
		//	Assert.AreEqual(0, set.Count);
		//	var node = new NoNode();
		//	NodeWork work = new NodeWork(node);
		//	work.Cost = 1;
		//	set.Add(work);

		//	Assert.AreEqual(1, set.Count);

		//	var node2 = new NoNode();
		//	NodeWork work2 = new NodeWork(node2);
		//	work2.Cost = 3;
		//	set.Add(work2);

		//	Assert.AreEqual(2, set.Count);

		//	Assert.AreEqual(work, set.PopBest());

		//	Assert.AreEqual(work2, set.PopBest());
		//}

		//[Test]
		//public void WorkSet2() {
		//	WorkSet set = new WorkSet();
		//	Assert.AreEqual(0, set.Count);

		//	int count = 20;

		//	List<int> costs = new List<int>();
		//	// create a sorted list of random costs
		//	int lastCost = (int)(UnityEngine.Random.value * 100f);
		//	for( int i = 0; i < count; i ++ ) {
		//		costs.Add(lastCost);
		//		lastCost += (int)(UnityEngine.Random.value * 100f);
		//	}
			
		//	foreach( var cost in CommonUtils.Shuffled<int>( costs) ) {
		//		var node = new NoNode();
		//		NodeWork work = new NodeWork(node);
		//		work.Cost = cost;
		//		set.Add(work);
		//		Debug.Log(cost);
		//	}
		//	Debug.Log("====");
		//	Assert.AreEqual(count, set.Count);

		//	for (int i = 0; i < count; i++) {
		//		int cost = costs[i];
		//		Assert.AreEqual(cost, set.PopBest().Cost);
		//		Debug.Log(cost);
		//	}

		//	Assert.AreEqual(0, set.Count);
		//}

		//[Test]
		//public void WorkSet3() {
		//	WorkSet set = new WorkSet();
		//	Assert.AreEqual(0, set.Count);

		//	int count = 20;

		//	List<int> costs = new List<int>();
		//	// create a sorted list of random costs
		//	int lastCost = (int)(UnityEngine.Random.value * 100f);
		//	for (int i = 0; i < count; i++) {
		//		costs.Add(lastCost);
		//		lastCost += (int)(UnityEngine.Random.value * 100f);
		//	}
		//	NodeWork noWork;
		//	foreach (var cost in CommonUtils.Shuffled<int>(costs)) {
		//		var node = new NoNode();
		//		NodeWork work = new NodeWork(node);
		//		work.Cost = cost;
				
		//		Assert.IsFalse(set.TryGet(node, out noWork));

		//		set.Add(work);

		//		Assert.IsTrue(set.TryGet(node, out noWork));
		//		Assert.AreEqual(noWork, work);
		//	}

		//	Assert.AreEqual(count, set.Count);

		//	for (int i = 0; i < count; i++) {
		//		int cost = costs[i];
		//		Assert.AreEqual(cost, set.PopBest().Cost);
		//	}

		//	Assert.AreEqual(0, set.Count);
		//}

		//[Test]
		//public void WorkSet4() {
		//	WorkSet set = new WorkSet();
		//	Assert.AreEqual(0, set.Count);

		//	var node = new NoNode();
		//	NodeWork work = new NodeWork(node);
		//	work.Cost = 99;
		//	set.Add(work);

		//	Assert.AreEqual(1, set.Count);

		//	var node2 = new NoNode();
		//	NodeWork work2 = new NodeWork(node2);
		//	work2.Cost = 99;
		//	set.Add(work2);

		//	Assert.AreEqual(2, set.Count);

		//	//Assert.AreEqual(work, set.PopBest());

		//	//Assert.AreEqual(work2, set.PopBest());
		//}
	}

	//public class NoNode : AbstractPathNode {
	//	//public IEnumerable<Edge<INode>> Edges() {
	//	//	throw new NotImplementedException();
	//	//}
	//}
}