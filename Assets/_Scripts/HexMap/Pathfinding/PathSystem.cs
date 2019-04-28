using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap.Pathfinding {

	/// <summary>
	/// Represents a body of paths for a map.
	/// Agents can query the system for a path from one abstract node to another
	/// and the system will figure it out once, then cache it.
	/// Paths are processed concurrently with the game, so a query for a path
	/// isn't guarenteed to return immediately, and instead links up a request
	/// with a callback that will be invoked when the path is finished at some point
	/// in the future.
	/// </summary>
	public class PathSystem {

		#region Path Map Members and Methods

		/// <summary>
		/// A map of any paths we've processed and cached.
		/// Stored in a table of tables: where axis 0 is the start coords,
		/// and axis 1 is the end coords.
		/// </summary>
		private HexTable<HexTable<OneShotPathJob<Vector2Int>>> pathMap =
			new HexTable<HexTable<OneShotPathJob<Vector2Int>>>();

		private HexMap map;

		/// <summary>
		/// Checks the cached paths map and returns immediately whether a path
		/// exists and what it is.
		/// </summary>
		private bool TryGetPath(Vector2Int start, Vector2Int end, out OneShotPathJob<Vector2Int> resultPath) {
			HexTable<OneShotPathJob<Vector2Int>> endTable = GetEndTable(start);
			return endTable.TryGet(end, out resultPath);
		}

		/// <summary>
		/// Used to get into the second layer of the pathMap table.
		/// Given a start coord, gets the corresponding table, which
		/// is keyed by end/destination coords.
		/// </summary>
		/// <param name="start"></param>
		/// <returns></returns>
		private HexTable<OneShotPathJob<Vector2Int>> GetEndTable(Vector2Int start) {
			HexTable<OneShotPathJob<Vector2Int>> resultTable;
			if (!pathMap.TryGet(start, out resultTable)) {
				resultTable = new HexTable<OneShotPathJob<Vector2Int>>();
				pathMap.Set(start, resultTable);
			}
			return resultTable;
		}

		private void SetPath(Vector2Int start, Vector2Int end, OneShotPathJob<Vector2Int> path) {
			HexTable<OneShotPathJob<Vector2Int>> endTable = GetEndTable(start);
			endTable.Set(end, path);
		}

		#endregion

		/// <summary>
		/// A reference to any jobs that are still processing and need to be updated.
		/// </summary>
		List<OneShotPathJob<Vector2Int>> ongoingJobs = new List<OneShotPathJob<Vector2Int>>();
		List<OneShotPathJob<Vector2Int>> removableJobs = new List<OneShotPathJob<Vector2Int>>();

		public void RequestPath(Vector2Int start, Vector2Int end,
				OneShotPathJob<Vector2Int>.CompleteDelegate callback) {

			OneShotPathJob<Vector2Int> resultPath;
			// If a path for this particular pair of start and end coords hasn't been requested yet
			// lets create one
			if (TryGetPath(start, end, out resultPath)) {
				// If the path is already processed and completed just immediately call the callback action
				if (resultPath.Status == PathStatus.Succeeded) {
					callback(resultPath.resultPath);
				}
				else
				if (resultPath.Status == PathStatus.Failed) {
					Debug.LogWarning("Cached path had failed.");
				}
				else
				// If the path is otherwise processing 
				{
					resultPath.OnComplete += callback;
				}
			} else {
				var newJob = NewGlobalJob(start, end);
				newJob.OnComplete += callback;
				//newJob.OnComplete += PathJobOnCompleteListener;
				SetPath(start, end, newJob);
				//resultPath.SetStart(start);
				//resultPath.SetEnd(end);
				//resultPath.StartJob();
				ongoingJobs.Add(newJob);
				//ongoingJobs.Add(new Tuple<Vector2Int, Vector2Int>(start, end), 1);
			}
			
		}

		public OneShotPathJob<Vector2Int> NewGlobalJob(Vector2Int start, Vector2Int end) {
			if (map == null) {
				map = HexNavMeshManager.GetHexMap();
			}

			var job = new OneShotPathJob<Vector2Int>(start, end,
				map.GetGlobalNodeFromWorldPosition,
				map.GetGlobalNode,
				HexNavMeshManager.GlobalHeuristic,
				HexNavMeshManager.GlobalCostFunction);
			
			//job.GetNodeFromPosition = map.GetGlobalNodeFromWorldPosition;
			//job.GetNodeFromArea = map.GetGlobalNode;
			//job.Heuristic = ;
			//job.CostFunction = ;
			return job;
		}

		//private void PathJobOnCompleteListener(PathJobResult<Vector2Int> pathResult) {
		//	Vector2Int start = pathResult.StartNode.Position;
		//	Vector2Int end = pathResult.EndNode.Position;
		//	SetPath(start, end, pathResult);
		//}

		#region Path Job Update Methods

		public void Update() {
			// TODO: Do the same number of passes over ongoing jobs, doing added passes over ones with more subscribers
			// -If there are a lot of different jobs that only have one subscriber each -> each job gets processed a little
			// -If there are only a few jobs -> each job gets processed a lot
			// -If there are a lot of different jobs, some that have a few subscribers and the rest that have none or one
			//    -> the jobs with more subscribers gets processed a lot, the others get processed a little

			//List< Tuple < Vector2Int, Vector2Int >> removable = new List<Tuple<Vector2Int, Vector2Int>>();

			removableJobs.Clear();

			foreach ( var job in ongoingJobs ) {
				//var start = pair.Key.Item1;
				//var end = pair.Key.Item2;

				//PathJob<Vector2Int> pathJob;
				//if (!TryGetPath(start, end, out pathJob)) {
				//	Debug.LogWarning("Ongoing job was null.");
				//}
				bool stillProcessing = job.UpdateJob(); // true if still updating

				if ( !stillProcessing) {
					removableJobs.Add(job);
				}
			}

			foreach( var job in removableJobs) {
				ongoingJobs.Remove(job);
			}
		}

		#endregion
	}
}