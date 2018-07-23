using Poly2Tri;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {

	public abstract class AbstractPathNode<T> {
		
		public T Position { get; set; }

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
	}

	public class NodeWork<T> : IComparable<NodeWork<T>> {
		public AbstractPathNode<T> Node { get; private set; }

		/// <summary>
		/// Total cost of all the edges taken so.
		/// </summary>
		public float Cost { get; set; }

		public float Heuristic { get; set; }

		public float TotalEstimatedCost { get { return Cost + Heuristic; } }

		public int Steps { get; set; }

		public NodeWork<T> Parent { get; private set; }

		public List<NodeWork<T>> Children { get; private set; }

		public NodeWork( AbstractPathNode<T> node, NodeWork<T> cameFrom = null ) {
			this.Node = node;
			Parent = cameFrom;
			Children = new List<NodeWork<T>>();
			cameFrom?.Children.Add(this);

			Steps = cameFrom != null ? cameFrom.Steps + 1 : 0;
		}
		
		public int CompareTo(NodeWork<T> other) {
			return Mathf.RoundToInt(TotalEstimatedCost - other.TotalEstimatedCost);
		}

		/// <summary>
		/// Removes this work from being a child of its parent
		/// </summary>
		public void Emancipate() {
			Parent = null;
		}

		public bool Contains( AbstractPathNode<T> node) {
			if ( Node == node ) {
				return true;
			}
			if ( Parent == null ) {
				return false;
			}
			return Parent.Contains(node);
		}
	}

	public enum PathStatus {
		None = 0,
		Processing = 2,
		Failed = 4,
		Succeeded = 8,
		Partial = 16,
	}

	/// <summary>
	/// I needed a data structure that auto sorted new entries based on their estimated cost and also hashed them by the nodes they used.
	/// There's probably a more efficient way but I did a dual-list setup where a BST has all the elements sorted and a Dictionary
	/// hashes them by the nodes.
	/// </summary>
	public class WorkSet<T> {
		private BinarySearchTree<NodeWork<T>> sorted = new BinarySearchTree<NodeWork<T>>();
		private Dictionary<AbstractPathNode<T>, NodeWork<T>> _set = new Dictionary<AbstractPathNode<T>, NodeWork<T>>();

		public int Count { get { return _set.Count;  } }

		public void Add( NodeWork<T> work ) {
			sorted.Add(work);
			_set.Add(work.Node, work);
		}

		public void Remove( NodeWork<T> work ) {
			sorted.Remove(work);
			_set.Remove(work.Node);
		}

		/// <summary>
		/// Returns the next nodeWork with the LOWEST estimated cost
		/// </summary>
		/// <returns></returns>
		public NodeWork<T> PopBest() {
			// The 'best' path is the one with the lowest estimated cost
			NodeWork<T> best = sorted.Min;
			Remove(best);
			return best;
		}

		public bool TryGet( AbstractPathNode<T> node, out NodeWork<T> work ) {
			return _set.TryGetValue(node, out work);
		}

		public void Clear() {
			sorted.Clear();
			_set.Clear();
		}
	}

	/// <summary>
	/// Used to traverse the HexMap by tiles. Represents one side between two touching tiles that connects to each other side of each tile.
	/// </summary>
	public class GlobalNode : AbstractPathNode<Vector2Int> {

		//public EdgeCoords coords;
		
		public List<GlobalEdge> edges = new List<GlobalEdge>();

		public GlobalNode( Vector2Int position ) {
			Position = position;
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

		public GlobalEdge(Vector2Int start, Vector2Int end) : base(start, end) { }

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
	/// Used to traverse the HexMap by tiles. Represents one side between two touching tiles that connects to each other side of each tile.
	/// </summary>
	public class LocalNode : AbstractPathNode<DelaunayTriangle> {
		
		public List<LocalEdge> edges;

		public LocalNode(DelaunayTriangle position) {
			Position = position;
		}

		public Vector3 WorldPos {
			get {
				var point = Position.Centroid();
				return new Vector3(point.Xf, 0, point.Yf);
			}
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

		private void MakeEdges() {
			edges = new List<LocalEdge>();
			for (int i = 0; i < 3; i++) {
				if (!Position.EdgeIsConstrained[i]) {
					LocalEdge newEdge = new LocalEdge(Position, Position.Neighbors[i]);
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

		public LocalEdge(DelaunayTriangle start, DelaunayTriangle end) : base(start, end) { }

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


	/// <summary>
	/// Provides global pathfinding across hex tiles.
	/// Serves up 'Jobs' that can take a start and end position and asyncroniously return a step-by-step path or a 'No Path'.
	/// A Job's start and end positions can be altered and updated after beginning and the pathfinding compensates.
	/// </summary>
	public class HexStar {

	}
}
