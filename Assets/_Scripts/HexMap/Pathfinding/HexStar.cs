using Poly2Tri;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {

	public abstract class AbstractPathNode<T> {
		
		public T Position { get; set; }

		/// <summary>
		/// The x-z plane position in the world. Used for cost and heuristics.
		/// </summary>
		public Vector2 WorldPos { get; set; }

		public abstract IEnumerable<Edge<T>> Edges();
		
	}

	public abstract class Edge<T> {
		public T Start { get; private set; }
		public T End { get; private set; }

		public Edge(T start, T end) {
			this.Start = start;
			this.End = end;
		}

		public abstract float Cost();

		public float Width { get; set; }
	}
	
	public enum PathStatus {
		None = 0,
		Processing = 2,
		Failed = 4,
		Succeeded = 8,
		Restarting = 16,
	}
	
	/// <summary>
	/// Used to traverse the HexMap by tiles. Represents one side between two touching tiles that connects to each other side of each tile.
	/// </summary>
	public class GlobalNode : AbstractPathNode<Vector2Int> {

		//public EdgeCoords coords;
		
		public List<GlobalEdge> edges = new List<GlobalEdge>();

		public GlobalNode( Vector2Int position, Vector3 worldPosition ) {
			Position = position;
			WorldPos = worldPosition.JustXZ();
		}

		public override IEnumerable<Edge<Vector2Int>> Edges() {
			foreach( var edge in edges ) {
				yield return edge;
			}
		}
	}
	
	/// <summary>
	/// A unidirectional connection from one node to another
	/// </summary>
	public class GlobalEdge : Edge<Vector2Int> {


		private bool _costCalculated = false;
		public float _cost;

		public GlobalEdge(Vector2Int start, Vector2Int end) : base(start, end) {
			Width = 1;
		}

		public override float Cost() {
			if (!_costCalculated) {
				//Vector3 startPos = Start.UnitWorldPosition;
				//Vector3 endPos = End.UnitWorldPosition;
				//_cost = (endPos - startPos).magnitude;
				_cost = 2;
				_costCalculated = true;
			}
			return _cost;
		}


	}

	/// <summary>
	/// Used to traverse the HexMap by triangulated triangles.
	/// </summary>
	public class LocalNode : AbstractPathNode<DelaunayTriangle> {
		
		public List<LocalEdge> edges;

		public LocalNode(DelaunayTriangle position) {
			Position = position;
			WorldPos = Position.Centroid().AsVector3().JustXZ();
		}

		public override IEnumerable<Edge<DelaunayTriangle>> Edges() {
			// only make the edges when you need them, then cache the results for any time after that
			if ( edges==null) {
				MakeEdges();
			}
			foreach (var edge in edges) {
				yield return edge;
			}
		}

		/// <summary>
		/// Checks the edges and returns true if the given node is connected to this one.
		/// </summary>
		/// <param name="otherNode"></param>
		/// <returns></returns>
		public bool IsAdjacentTo( LocalNode otherNode ) {
			foreach( var edge in Edges() ) {
				if ( edge.End == otherNode.Position ) {
					return true;
				}
			}
			return false;
		}

		private void MakeEdges() {
			edges = new List<LocalEdge>();
			for (int i = 0; i < 3; i++) {
				if (!Position.EdgeIsConstrained[i]) {
					var pointA = Position.Points[i].AsVector3();
					var pointB = Position.Points[(i+1)%3].AsVector3();
					var width = (pointB - pointA).magnitude;
					LocalEdge newEdge = new LocalEdge(Position, Position.Neighbors[i], width);
					edges.Add(newEdge);
				}
			}

			//foreach (var triangle in Position.Neighbors) {
			//	if ( triangle!=null ) {
			//		LocalEdge newEdge = new LocalEdge(Position, triangle);
			//		edges.Add(newEdge);
			//	}
				
			//}
		}

	}
	
	/// <summary>
	/// A unidirectional connection from one node to another
	/// </summary>
	public class LocalEdge : Edge<DelaunayTriangle> {


		private bool _costCalculated = false;
		public float _cost;

		public LocalEdge(DelaunayTriangle start, DelaunayTriangle end, float width)
				: base(start, end) {
			Width = width;
		}

		public override float Cost() {
			if (!_costCalculated) {
				var start = Start.Centroid();
				var end = End.Centroid();
				float x = start.Xf - end.Xf;
				float y = start.Yf - end.Yf;
				_cost = Mathf.Sqrt( x*x + y*y);
				_costCalculated = true;
			}
			return _cost;
		}
	}

	

	//public struct EdgeCoords {
	//	public Vector2Int position;
	//	public HexDirection direction;

	//	public EdgeCoords(Vector2Int position, HexDirection direction) {
	//		this.position = position;
	//		this.direction = direction;
	//	}

	//	/// <summary>
	//	/// Returns the edge's center point's world position if the tile size is one. Not accounting for y or height.
	//	/// </summary>
	//	public Vector3 UnitWorldPosition {
	//		get {
	//			return HexUtils.PositionFromCoordinates(position, 1) + HexUtils.CenterOfSide(direction);
	//		}
	//	}
	//}
	
}
