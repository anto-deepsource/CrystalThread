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
	public class PathJob<T> {

		public PathStatus Status { get; private set; }

		private Vector3 startPosition;
		private Vector3 endPosition;

		private AbstractPathNode<T> startNode;
		private AbstractPathNode<T> endNode;

		public PriorityQueue<AbstractPathNode<T>, NodeWork<T>> openList = 
			new PriorityQueue<AbstractPathNode<T>, NodeWork<T>>();

		public Dictionary<AbstractPathNode<T>, NodeWork<T>> closedList = 
			new Dictionary<AbstractPathNode<T>, NodeWork<T>>();

		public NodeWork<T> completedWork = null;
		
		private bool needsRestart = false;

		private bool hasStart = false;
		private bool hasEnd = false;
		
		public bool NeedsRestart { get { return needsRestart && hasStart && hasEnd; } }

		public delegate void CompleteDelegate(NodeWork<T> work);
		
		public event CompleteDelegate OnComplete;
		public Action OnClear;
		public Action OnFailure;

		public Func< Vector3, AbstractPathNode<T> > GetNodeFromPosition;

		public Func<T, AbstractPathNode<T> > GetNodeFromArea;

		public Func<T, Vector3, float> Heuristic;
		public Func<T, Vector3, float> CostFunction;

		public QuickEvent Events { get { return _events; } }
		private QuickEvent _events = new QuickEvent();

		//public HashSet<string> debugFlags = new HashSet<string>();
		//public HashSet<string> debugFlags2;
		//public HashSet<string> debugFlags3;

		public PathJob() {
			Status = PathStatus.None;
		}

		public void SetStart(T newStart) {
			AbstractPathNode<T> newStartNode = GetNodeFromArea(newStart);
			startPosition = newStartNode.WorldPos;
			SetStart(newStartNode);
		}

		public void SetStart(Vector3 newStart) {
			AbstractPathNode<T> newStartNode = GetNodeFromPosition(newStart);
			startPosition = newStart;
			SetStart(newStartNode);
		}

		/// <summary>
		/// Accepts an initial or an updated start point and flags whether the path needs to be restarted.
		/// </summary>
		/// <param name="newStart"></param>
		public void SetStart(AbstractPathNode<T> newStartNode) {

			// If this is the first we're ever seeing a start point then we obviously know we need to restart
			if ( startNode == null ) {
				ClearWork();
				//debugFlags.Add("There was no previous start node");
			} else

			// If the new start nodes is not the same as the old start node -> we need to make a few checks
			if (newStartNode != startNode) {
				//debugFlags.Add("The last start node and the new start node were NOT the same");
				// if we've already found a path and the new start node is just one of the nodes on the path then all goo
				if (Status == PathStatus.Succeeded && PathContainsNode(newStartNode)) {
					// do nothing
					//debugFlags.Add("The previous path contained the new start node");
				} else
				////// if we've already processed the new start node then
				////// that's not a save all but it at least means
				////// that the new node is on the way and we may have processed some of it already
				////NodeWork<T> closedWork;
				////if (closedList.TryGet(newStartNode, out closedWork)) {
				////	// set this work to be the new end/root
				////	closedWork.Emancipate();
				////	// we can go through the children of this work and start our path search from there
				////	// first clear the path
				////	ClearWork();
				////	closedWork.IterateThroughChildrenAndAddToOpenList(ref openList);

				////	// if somehow we still have nothing in the openlist then we failed to salvage anything
				////	if (openList.Count == 0) {
				////		needsRestart = true;
				////	}

				////	// if we have anything in the open list then we at least have the one start node
				////	// queued up ready to process, possibly a few things already processed
				////	// we don't need/want to restart the process
				////	//Debug.Log("New start was on the way");
				////}
				{
					ClearWork();
				}
			} else {
				// else If they ARE the same we don't have to restart anything
				//debugFlags.Add("The last start node and the new start node WERE the same");
			}


			//debugFlags.Add("Got new start");
			// save the new position and node
			startNode = newStartNode;
			
			hasStart = true;
		}

		public void SetEnd(T newEnd) {
			AbstractPathNode<T> newEndNode = GetNodeFromArea(newEnd);
			endPosition = newEndNode.WorldPos;
			SetEnd(newEndNode);
		}

		public void SetEnd(Vector3 newEnd) {
			AbstractPathNode<T> newEndNode = GetNodeFromPosition(newEnd);
			endPosition = newEnd;
			SetEnd(newEndNode);
		}

		/// <summary>
		/// Accepts an initial or an updated end point and flags whether the path needs to be restarted.
		/// </summary>
		public void SetEnd(AbstractPathNode<T> newEndNode) {
			
			// If this is the first we're ever seeing a start point then we obviously know we need to restart
			if (endNode == null) {
				ClearWork();
			} else
			// if the new end node is not the same as the previous one
			if ( newEndNode != endNode ) {
				// There are two scenarios that we can salvage:
				//	1. we may have already found a path to that node (ie: if it has moved toward us)
				//NodeWork<T> closedWork;
				//if (closedList.TryGet(newEndNode, out closedWork)) {
				//	// We don't need to restart and the current process will fall out the bottom correctly
				//	// after we call CompletePathSuccess()
				//	CompletePathSuccess(closedWork);
				//	Debug.Log("New dest was on the way");
				//} else
				//// 2. if the new node is adjacent to the old one
				//if (newEndNode.IsAdjacentTo(endNode)) {
				//	// Then we can probably continue with the work we were doing and not worry too much

				//} else
				// can't think of any other ways to salvage
				{
					ClearWork();
				}
			}

			//debugFlags.Add("Got new end");
			// save the new position and node
			endNode = newEndNode;
			hasEnd = true ;
		}

		public void ClearWork() {
			Status = PathStatus.Restarting;
			openList.Clear();
			closedList.Clear();
			completedWork = null;
			OnClear?.Invoke();
			Events.Fire(JobEvent.Clear, this);
			//debugFlags.Add("Cleared work");
			needsRestart = true;
		}
		
		public bool PathContainsNode( AbstractPathNode<T> node ) {
			if ( Status!= PathStatus.Succeeded || completedWork == null ) {
				return false;
			}
			return completedWork.Contains(node);
		}

		public void StartJob() {
			Status = PathStatus.Processing;

			needsRestart = false;
			//debugFlags.Add("Start job");
			// The lists should already be cleared by here

			// if the start node or end node is null then there's not much we can do
			if (startNode == null || endNode == null) {
				CompletePathFailure();
			} else

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
						} else {
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
						} else {
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
			if ( openList.Count == 0) {
				CompletePathFailure();
				return false;
			}

			return true;
		}

		public IEnumerator ProcessJobCoroutine() {
			// Initialize the process
			Status = PathStatus.Processing;

			needsRestart = false;
			//debugFlags.Add("Start Coroutine");
			// The lists should already be cleared by here

			// if the start node or end node is null then there's not much we can do
			if ( startNode==null || endNode ==null) {
				CompletePathFailure();
			} else

			// Start by adding the starting tile
			{
				var newWork = new NodeWork<T>(startNode, null);
				//newWork.Cost = (startNode.WorldPos - startPosition.JustXZ()).magnitude;
				newWork.Cost = CostFunction(startNode.Position, startPosition);
				newWork.Heuristic = Heuristic(startNode.Position, endPosition);
				openList.Add(startNode, newWork);
			}
			
			float startTime = Time.time;
			// As long as we're still Processing and we have at least 1 node process:
			while (Status == PathStatus.Processing && openList.Count > 0) {

				// Pop the most promising next node work
				NodeWork<T> work = openList.PopTop();

				//if ( closedList.Contains(work.Node) ) {
				//	string s = "Brian";
				//}

				// If the tile we end up in after this is the destination tile
				// complete success
				if (work.Node == endNode) {
					CompletePathSuccess(work);
					continue;
				}

				// Process each of the edges for this node
				foreach (var edge in work.Node.Edges()) {
					// each edge should have the start as this node
					// get the node that corresponds to the end
					AbstractPathNode<T> endNode = GetNodeFromArea(edge.End);
					// We'll have to calculate the work so do that now
					NodeWork<T> newWork = NewNodeWork(endNode, work, edge);

					// check whether the end node for this edge is already in our open list
					NodeWork<T> openWork;
					if (openList.TryGet(endNode, out openWork)) {
						// this node is already on the open list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < openWork.TotalEstimatedCost) {
							// lets remove the one that's already there and replace it with this one
							openList.Remove(endNode);
							openList.Add(endNode, newWork);
						}
						// Either way skip to the next edge
						continue;
					} // end of check whether in our open list

					// check if this node is already on our closed list
					NodeWork<T> closedWork;
					if (closedList.TryGetValue(endNode, out closedWork)) {
						// this node is already on the closed list, but lets check to see if this route is better
						if (newWork.TotalEstimatedCost < closedWork.TotalEstimatedCost) {
							// lets remove the one that's already there and put the new one back on the open list
							closedList.Remove(endNode);
							openList.Add(endNode, newWork);
						}
						// Either way skip to the next edge
						continue;
					} // end of check if this node is already on our closed list

					// if we got here then just add it to the open list
					openList.Add(endNode, newWork);
				}

				//SetBestBathSoFar(work);

				try {
					closedList.Add(work.Node, work);
				}catch (Exception e ) {
					throw e;
				}

				//if (Time.time - startTime > HexNavMeshManager.MaxProcessTime ) {
				yield return null;
				//	startTime = Time.time;
				//}

			}

			// If we haven't successfully found a path yet then there isn't one
			// This will only happen if the agent is surrounded by walls relative to the destination
			if (Status == PathStatus.Processing) {
				CompletePathFailure();
			}
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
			completedWork = work;
			OnComplete?.Invoke(work);
			Events.Fire(JobEvent.CompleteSuccess, this);
			//debugFlags.Add("Success Complete");
		}

		private void CompletePathFailure() {
			//Debug.Log("Failed to find path");
			Status = PathStatus.Failed;
			OnFailure?.Invoke();
			Events.Fire(JobEvent.CompleteFailure, this);
			//debugFlags.Add("Failure Complete");
		}
		
		//private float Heuristic(Vector3 start, Vector3 end) {

		//	//// X-Z manhatten distance from the start position to the end.
		//	//return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.z - end.z);

		//	//// Euclidean distance (way the crow flys)
		//	//return (end - start).magnitude;

		//	// Move along the legs of a 90/45/45 triangle between start and end.
		//	// Sort of if manhattan distance rotated to be the worst case from any point to any other
		//	// hyp = euclidean distance, a = b, h^2 = b^2 + a^2
		//	// 2a^2 = h^2
		//	// a^2 = h^2/2
		//	// a = sqrt( h^2/2 )
		//	float h2 = (end.DropY() - start.DropY()).sqrMagnitude;
		//	float a = Mathf.Sqrt(h2 * 0.5f);
		//	return a * 2;
		//}
	}

	public enum JobEvent {
		Start,
		CompleteSuccess,
		CompleteFailure,
		Clear,
	}
}