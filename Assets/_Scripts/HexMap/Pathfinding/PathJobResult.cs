
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	/// <summary>
	/// Wraps a completed PathJob NodeWork and allows for
	/// other classes to quickly obtain the desired info about the path.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PathJobResult<T> {

		/// <summary>
		/// True if the job found a path.
		/// </summary>
		public bool Exists { get; private set; }

		public int Count {
			get {
				if ( !Exists ) {
					return 0;
				}
				GetPath(); // creates the actualPath if needed
				return actualPath.Count;
			}
		}

		public AbstractPathNode<T> StartNode {
			get {
				if ( !Exists ) { return null; }

				if (_startNode==null) {
					NodeWork<T> currentWork = work;
					// The node we want is going to be the very-most parent
					while (currentWork != null) {
						_startNode = currentWork.Node;
						currentWork = currentWork.Parent;
					}
				}
				return _startNode;
			}
		}
		private AbstractPathNode<T> _startNode;
		
		public AbstractPathNode<T> EndNode {
			get {
				if (!Exists) { return null; }

				if (_endNode == null) {
					_endNode = work.Node;
				}
				return _endNode;
			}
		}
		private AbstractPathNode<T> _endNode;

		private NodeWork<T> work;

		private bool processedActualPath = false;
		private List<Vector3> actualPath;

		private bool processedMidpointPath = false;
		private List<Vector3> midpointPath;

		private bool processedNodeSet = false;
		private HashSet<AbstractPathNode<T>> usedNodeSet;

		private bool processedPositionsSet = false;
		private HashSet<T> usedPositions;

		public PathJobResult(NodeWork<T> work) {
			if (work == null) {
				Exists = false;
				return;

			}
			else {
				Exists = true;
				this.work = work;
			}

		}

		public static PathJobResult<T> None() {
			return new PathJobResult<T>(null);
		}

		/// <summary>
		/// Uses the result path to calculate a list of points.
		/// </summary>
		/// <returns></returns>
		public List<Vector3> GetPath() {
			if ( !Exists ) { return null; }

			if (!processedActualPath) {
				actualPath = new List<Vector3>();
				NodeWork<T> currentWork = work;
				Vector3 lastPoint = currentWork.Node.WorldPos.FromXZ();
				while (currentWork != null) {
					// add them to the list at the beginning because we're moving through the path from end to start
					var point = currentWork.Node.WorldPos.FromXZ();
					//var vector = point - lastPoint;
					actualPath.Insert(0, point);
					//actualPath.Insert(0, lastPoint + vector * 0.51f);
					currentWork = currentWork.Parent;
					lastPoint = point;
				}

				processedActualPath = true;
			}
			return actualPath;
		}

		/// <summary>
		/// Uses the result path to calculate a list of points,
		/// where each returned point is the midpoint of two actual waypoints.
		/// Used for the global path to get bridge points.
		/// </summary>
		/// <returns></returns>
		public List<Vector3> GetMidpointPath() {
			if (!Exists) { return null; }

			if (!processedMidpointPath) {
				midpointPath = new List<Vector3>();
				NodeWork<T> currentWork = work;
				Vector3 lastPoint = currentWork.Node.WorldPos.FromXZ();
				currentWork = currentWork.Parent;
				while (currentWork != null) {
					// add them to the list at the beginning because we're moving through the path from end to start
					var point = currentWork.Node.WorldPos.FromXZ();
					var vector = point - lastPoint;
					//currentFullPath.Insert(0, point);
					midpointPath.Insert(0, lastPoint + vector * 0.51f);
					currentWork = currentWork.Parent;
					lastPoint = point;
				}

				processedMidpointPath = true;
			}
			return midpointPath;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool UsesNode(AbstractPathNode<T> node) {
			if (!Exists) { return false; }

			if ( !processedNodeSet ) {
				usedNodeSet = new HashSet<AbstractPathNode<T>>();
				NodeWork<T> currentWork = work;
				while (currentWork != null) {
					usedNodeSet.Add(currentWork.Node);
					currentWork = currentWork.Parent;
				}

				processedNodeSet = true;
			}
			return usedNodeSet.Contains(node);
		}

		public bool UsesPosition(T position) {
			if (!Exists) { return false; }

			if (!processedPositionsSet) {
				usedPositions = new HashSet<T>();
				NodeWork<T> currentWork = work;
				while (currentWork != null) {
					usedPositions.Add(currentWork.Node.Position);
					currentWork = currentWork.Parent;
				}

				processedPositionsSet = true;
			}
			return usedPositions.Contains(position);
		}
	}
}