
using Poly2Tri;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {

	/// <summary>
	/// Takes a start and a destination and attempts to navigate a path through the local nav mesh.
	/// Can be processed immediately or by a coroutine, but probably not both.
	/// Can continuously take new start and/or end positions and handles recalculations.
	/// Does not listen to the nav mesh for map changes but opens up data about the path so that
	/// an agent or system can listen themselves and check against this path.
	/// </summary>
	public class OneShotPathJob<T> {

		public PathStatus Status { get; private set; }

		public PathJobResult<T> resultPath;

		private Vector3 startPosition;
		private Vector3 endPosition;

		private AbstractPathNode<T> startNode;
		private AbstractPathNode<T> endNode;

		public PriorityQueue<AbstractPathNode<T>, NodeWork<T>> openList =
			new PriorityQueue<AbstractPathNode<T>, NodeWork<T>>();

		public Dictionary<AbstractPathNode<T>, NodeWork<T>> closedList =
			new Dictionary<AbstractPathNode<T>, NodeWork<T>>();

		//public NodeWork<T> completedWork = null;

		//private bool needsRestart = false;

		//private bool hasStart = false;
		//private bool hasEnd = false;

		//public bool NeedsRestart { get { return needsRestart && hasStart && hasEnd; } }

		public delegate void CompleteDelegate(PathJobResult<T> work);

		public event CompleteDelegate OnComplete;
		//public Action OnClear;
		//public Action OnFailure;

		public Func<Vector3, AbstractPathNode<T>> GetNodeFromPosition;

		public Func<T, AbstractPathNode<T>> GetNodeFromArea;

		public Func<T, Vector3, float> Heuristic;
		public Func<T, Vector3, float> CostFunction;
		
		public OneShotPathJob(T start, T end,
				Func<Vector3, AbstractPathNode<T>> GetNodeFromPosition, 
				Func<T, AbstractPathNode<T>> GetNodeFromArea,
				Func<T, Vector3, float> Heuristic,
				Func<T, Vector3, float> CostFunction) {
			Status = PathStatus.None;
			this.GetNodeFromPosition = GetNodeFromPosition;
			this.GetNodeFromArea = GetNodeFromArea;
			this.Heuristic = Heuristic;
			this.CostFunction = CostFunction;
			SetStart(start);
			SetEnd(end);
			StartJob();
		}

		private void SetStart(T newStart) {
			AbstractPathNode<T> newStartNode = GetNodeFromArea(newStart);
			startPosition = newStartNode.WorldPos;
			startNode = newStartNode;
		}

		private void SetStart(Vector3 newStart) {
			AbstractPathNode<T> newStartNode = GetNodeFromPosition(newStart);
			startPosition = newStart;
			startNode = newStartNode;
		}

		private void SetEnd(T newEnd) {
			AbstractPathNode<T> newEndNode = GetNodeFromArea(newEnd);
			endPosition = newEndNode.WorldPos;
			endNode = newEndNode;
		}

		private void SetEnd(Vector3 newEnd) {
			AbstractPathNode<T> newEndNode = GetNodeFromPosition(newEnd);
			endPosition = newEnd;
			endNode = newEndNode;
		}
		
		private void StartJob() {
			Status = PathStatus.Processing;

			openList.Clear();
			closedList.Clear();

			// if the start node or end node is null then there's not much we can do
			if (startNode == null || endNode == null) {
				CompletePathFailure();
			}
			else

			// Start by adding the starting tile
			{
				var newWork = new NodeWork<T>(startNode, null);
				//newWork.Cost = (startNode.WorldPos - startPosition.JustXZ()).magnitude;
				newWork.Cost = CostFunction(startNode.Position, startPosition);
				newWork.Heuristic = Heuristic(startNode.Position, endPosition);
				openList.Add(startNode, newWork);
			}

			float startTime = Time.time;
		}

		/// <summary>
		/// Returns true if the job is still updating, false if the job is done.
		/// </summary>
		public bool UpdateJob() {
			// As long as we're still Processing and we have at least 1 node process:
			if (Status == PathStatus.Processing && openList.Count > 0) {

				// Pop the most promising next node work
				NodeWork<T> work = openList.PopTop();

				// If the tile we end up in after this is the destination tile
				// complete success
				if (work.Node == endNode) {
					CompletePathSuccess(work);
					return false;
				}

				// Process each of the edges for this node
				foreach (var edge in work.Node.Edges()) {
					// each edge should have the start as this node
					// get the node that corresponds to the end
					AbstractPathNode<T> newNode = GetNodeFromArea(edge.End);
					// We'll have to calculate the work so do that now
					NodeWork<T> newWork = NewNodeWork(newNode, work, edge);

					// check whether the end node for this edge is already in our open list
					NodeWork<T> openWork;
					if (openList.TryGet(newNode, out openWork)) {
						// this node is already on the open list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < openWork.TotalEstimatedCost) {
							// lets remove the one that's already there and replace it with this one
							openList.Remove(newNode);
							openList.Add(newNode, newWork);
							//string message = string.Format("Add {0}, remove {1} from open list, during {2}",
							//	newWork.Node.Position, openWork.Node.Position,work.Node.Position);
							//debugFlags.Add(message);
						}
						else {
							//string message = string.Format("Don't add {0}, leave {1} in open list, during {2}",
							//	newWork.Node.Position, openWork.Node.Position, work.Node.Position);
							//debugFlags.Add(message);
						}
						// Either way skip to the next edge
						continue;
					} // end of check whether in our open list

					// check if this node is already on our closed list
					NodeWork<T> closedWork;
					if (closedList.TryGetValue(newNode, out closedWork)) {
						// this node is already on the closed list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < closedWork.TotalEstimatedCost) {
							// lets remove the one that's already there and put the new one back on the open list
							closedList.Remove(newNode);
							openList.Add(newNode, newWork);
							//string message = string.Format("Add {0}, remove {1} from closed list, during {2}",
							//		newWork.Node.Position, closedWork.Node.Position, work.Node.Position);
							//debugFlags.Add(message);
						}
						else {
							//string message = string.Format("Don't add {0}, leave {1} in closed list, during {2}",
							//	newWork.Node.Position, closedWork.Node.Position, work.Node.Position);
							//debugFlags.Add(message);
						}
						// Either way skip to the next edge
						continue;
					} // end of check if this node is already on our closed list

					// if we got here then just add it to the open list
					openList.Add(newNode, newWork);
					//debugFlags.Add("Add new work to open list: " + newWork.Node.Position.ToString() );
				}

				//SetBestBathSoFar(work);

				//try {
				closedList.Add(work.Node, work);
				//} catch (Exception e) {
				//	throw e;
				//}

				//if (Status == PathStatus.Processing && openList.Count > 0) {
				//if ( closedList.ContainsKey(openList.PeekBest().Node) ) {
				//	debugFlags.Add("The next best work is already in the closed list" );
				//}
				//}

			}

			// If we haven't successfully found a path yet then there isn't one
			// This will only happen if the agent is surrounded by walls relative to the destination
			if (openList.Count == 0) {
				CompletePathFailure();
				return false;
			}

			return true;
		}
		
		private NodeWork<T> NewNodeWork(AbstractPathNode<T> node,
				NodeWork<T> cameFrom, Edge<T> edge) {
			NodeWork<T> newWork = new NodeWork<T>(node, cameFrom);

			newWork.Cost = cameFrom.Cost + edge.Cost();
			newWork.Heuristic = Heuristic(node.Position, endPosition);
			return newWork;

		}

		private void CompletePathSuccess(NodeWork<T> work) {
			Status = PathStatus.Succeeded;
			//completedWork = work;
			resultPath = new PathJobResult<T>(work);
			OnComplete?.Invoke(resultPath);
		}

		private void CompletePathFailure() {
			Status = PathStatus.Failed;
			// TODO: notify listeners of failure
		}
		
	}

}