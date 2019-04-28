using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	/// <summary>
	/// I needed a data structure that auto sorted new entries based on their estimated cost and also hashed them by the nodes they used.
	/// There's probably a more efficient way but I did a dual-list setup where a BST has all the elements sorted and a Dictionary
	/// hashes them by the nodes.
	/// </summary>
	public class WorkSet<T> {
		private List<NodeWork<T>> _list = new List<NodeWork<T>>();
		private Dictionary<AbstractPathNode<T>, NodeWork<T>> set = new Dictionary<AbstractPathNode<T>, NodeWork<T>>();

		private bool sorted = false;

		public int Count { get { return _list.Count; } }

		public void Add(NodeWork<T> work) {
			_list.Add(work);
			set.Add(work.Node, work);
			sorted = false;
		}

		public void Remove(NodeWork<T> work) {
			if (!_list.Remove(work)) {
				string s = "Brian";
			}
			if (!set.Remove(work.Node)) {
				//foreach (var w in set.Values) {
				//	if (!sorted.Contains(w)) {
				string s = "Brian";
				//	}
				//}
			}
		}

		/// <summary>
		/// Returns the next nodeWork with the LOWEST estimated cost
		/// </summary>
		public NodeWork<T> PopBest() {
			// The 'best' path is the one with the lowest estimated cost
			NodeWork<T> best = PeekBest();

			Remove(best);
			return best;
		}

		public NodeWork<T> PeekBest() {
			//float min = 0;
			//// The 'best' path is the one with the lowest estimated cost
			//NodeWork<T> best = null;

			if (!sorted ) {
				_list.Sort();
				sorted = true;
			}
			
			return _list[0];

			//foreach (var work in _list) {
			//	if (best == null || work.TotalEstimatedCost < min) {
			//		best = work;
			//		min = work.TotalEstimatedCost;
			//	}
			//}
			//return best;
		}

		public bool Contains(AbstractPathNode<T> node) {
			return set.ContainsKey(node);
		}

		public bool TryGet(AbstractPathNode<T> node, out NodeWork<T> work) {
			return set.TryGetValue(node, out work);
		}

		public void Clear() {
			_list.Clear();
			set.Clear();
			sorted = false;
		}

	}
}