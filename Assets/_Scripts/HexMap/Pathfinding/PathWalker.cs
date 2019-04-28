using Poly2Tri;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {
	public class PathWalker<T> {

		public float stopDistance = 1;

		private PathJob<T> pathJob;

		private List<Vector3> currentFullPath;

		private int currentPathIndex = -1;

		private NodeWork<T> completeWork;

		public List<Vector3> Path { get { return path; } }
		private List<Vector3> path = new List<Vector3>();

		public PathWalker(PathJob<T> job, float stopDistance) {
			this.stopDistance = stopDistance;

			pathJob = job;
			pathJob.OnComplete += CompletePathSuccess;
			pathJob.OnClear = ClearPath;
		}

		public bool Valid { get; private set; }

		private Vector3 _destination;
		public Vector3 Destination {
			get { return _destination; }
			set {
				Vector3 newDes = value.DropY();
				if (!_destination.ApproxEquals(newDes, 0.1f)) {
					pathJob.SetEnd(newDes);
				}
				_destination = newDes;
			}
		}

		/// <summary>
		/// A magnitude-1 vector that represents the direction the agent
		/// currently needs to move on the x-z plane.
		/// </summary>
		public Vector3 MoveVector( Vector3 currentPosition ) {
			if (HasRemainingWayPoints()) {
				Vector3 pos = currentFullPath[currentPathIndex];
				Vector3 vector = pos - currentPosition.DropY();
				if (vector.magnitude < stopDistance) {
					QueueNextWayPoint();
					// rerun the function on the next way point
					return MoveVector(currentPosition);
				} else {
					return vector.normalized;
				}
			} else {
				// we can simply move straight towards the destination point
				Vector3 pos = Destination;
				Vector3 vector = pos - currentPosition.DropY();
				//if (vector.magnitude < stopDistance) {
				//	// stop
				//	return Vector3.zero;
				//} else {
				if ( vector.sqrMagnitude > 1f ) {
					return vector.normalized;
				} else {
					return vector;
				}
				
				//}
			}
		}

		public void SetStart( Vector3 start ) {
			pathJob.SetStart(start);
		}

		public bool NeedsRestart { get { return pathJob.NeedsRestart;  } }

		public IEnumerator ProcessJobCoroutine() {
			return pathJob.ProcessJobCoroutine();
		}

		public void StartJob() {
			pathJob.StartJob();
		}

		public bool UpdateJob() {
			bool value = pathJob.UpdateJob();
			return value;
		}

		public bool HasRemainingWayPoints() {
			if (currentFullPath == null ||
					currentFullPath.Count == 0 ||
					currentPathIndex == -1 ||
					currentPathIndex >= currentFullPath.Count) {
				return false;
			}
			return true;
		}

		public bool PathContainsNode(AbstractPathNode<T> node) {
			if ( completeWork == null) {
				return false;
			}
			return completeWork.Contains(node);
		}

		public Vector3 GetCurrentWayPoint() {
			if (currentPathIndex < currentFullPath.Count) {
				return currentFullPath[currentPathIndex];
			} else {
				return Destination;
			}
		}

		/// <summary>
		/// Returns the point AFTER the current waypoint.
		/// </summary>
		public Vector3 GetNextWayPoint() {
			if (currentPathIndex + 1 < currentFullPath.Count) {
				return currentFullPath[currentPathIndex + 1];
			} else {
				return Destination;
			}
		}

		/// <summary>
		/// Called when we've reached the current waypoint and want to move to the next one.
		/// </summary>
		public void QueueNextWayPoint() {
			currentPathIndex++;
			ValidatePath();

			//if ( completeWork!=null && completeWork.Parent!=null) {
			//	completeWork = completeWork.Parent;
			//}
		}

		private void ClearPath() {
			path.Clear();
			currentFullPath?.Clear();
			completeWork = null;
			Valid = false;
		}

		public void ForceClearPath() {
			pathJob.ClearWork();
			ClearPath();
		}

		//public void ResetDebugFlags() {
		//	pathJob.debugFlags3 = pathJob.debugFlags2;
		//	pathJob.debugFlags2 = new HashSet<string>( pathJob.debugFlags );
		//	pathJob.debugFlags.Clear();
		//}

		//public void Flag(string flag ) {
		//	pathJob.debugFlags.Add(flag);
		//}

		private void CompletePathSuccess(NodeWork<T> work) {

			// we need to do a few things to the path in order for the agent to then use it
			currentFullPath = new List<Vector3>();
			NodeWork<T> currentWork = work;
			Vector3 lastPoint = currentWork.Node.WorldPos.FromXZ();
			while (currentWork != null) {
				// add them to the list at the beginning because we're moving through the path from end to start
				var point = currentWork.Node.WorldPos.FromXZ();
				var vector = point - lastPoint;
				currentFullPath.Insert(0, point);
				//currentFullPath.Insert(0, lastPoint + vector * 0.51f);
				currentWork = currentWork.Parent;
				lastPoint = point;
			}

			completeWork = work;
			currentPathIndex = 0;
			ValidatePath();
		}

		private void ValidatePath() {
			path.Clear();
			for (int i = currentPathIndex; i < currentFullPath.Count; i++) {
				path.Add(currentFullPath[i]);
			}
			path.Add(Destination);
			Valid = true;
		}

		//public bool DebugThatFuckingBug() {
		//	if (pathJob.Status == PathStatus.Processing && pathJob.openList.Count > 0) {
		//		NodeWork<T> work = pathJob.openList.PeekBest();
		//		return pathJob.closedList.ContainsKey(work.Node);
		//	}
		//	return false;
		//}
	}
}