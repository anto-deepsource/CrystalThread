using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	public class NodeWork<T> : IComparable {
		public AbstractPathNode<T> Node { get; private set; }

		/// <summary>
		/// Total cost of all the edges taken so.
		/// </summary>
		public float Cost { get; set; }

		public float Heuristic { get; set; }

		public float TotalEstimatedCost { get { return Cost + Heuristic; } }

		public int Steps { get; set; }

		public NodeWork<T> Parent { get; private set; }

		//public List<NodeWork<T>> Children { get; private set; }

		public NodeWork(AbstractPathNode<T> node, NodeWork<T> cameFrom = null) {
			this.Node = node;
			Parent = cameFrom;
			//Children = new List<NodeWork<T>>();
			//cameFrom?.Children.Add(this);

			Steps = cameFrom != null ? cameFrom.Steps + 1 : 0;
		}

		public int CompareTo(NodeWork<T> other) {
			return Mathf.RoundToInt(TotalEstimatedCost - other.TotalEstimatedCost);
		}

		public int CompareTo(object obj) {
			if ( obj is NodeWork<T> ) {
				return CompareTo(obj as NodeWork<T>);
			} else {
				return -1;
			}
		}

		/// <summary>
		/// Removes this work from being a child of its parent
		/// </summary>
		public void Emancipate() {
			Parent = null;
		}

		public bool Contains(AbstractPathNode<T> node) {
			if (Node == node) {
				return true;
			}
			if (Parent == null) {
				return false;
			}
			return Parent.Contains(node);
		}


		//public void IterateThroughChildrenAndAddToOpenList(ref WorkSet<T> openList) {
		//	// if it has no children then it hasn't been processed yet and can go back on the open list
		//	if (Children.Count == 0) {
		//		openList.Add(this);
		//	} else {
		//		// go through each of the children
		//		foreach (var child in Children) {
		//			child.IterateThroughChildrenAndAddToOpenList(ref openList);
		//		}
		//	}
		//}

		
	}
}