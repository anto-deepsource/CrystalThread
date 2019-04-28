
using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	public class FlankTargetState : IState {

		public Awareness awareness;

		public float stopDistance = 1f;

		public float flankDistance = 10f;

		Sequence sequence;

		MoveToPointTask moveTask;

		//public static readonly string Key = "FlankTarget";

		//public override string UniqueKey() { return Key; }
		
		private void Start() {
			SetupSequence();
		}

		private void SetupSequence() {

			if (this.awareness == null) {
				awareness = GetComponentInChildren<Awareness>();
			}

			sequence = new Sequence(false);
			sequence.Add(new Gig(PickTarget));
			moveTask = new MoveToPointTask(stopDistance);
			sequence.Add(moveTask);
		}

		private bool PickTarget() {

			if (awareness.EnemiesNearby) {
				moveTask.target = HexNavMeshManager.GetAnyPointInArea(
					awareness.MainEnemyTarget.transform.position, flankDistance);
				moveTask.stopDistance = stopDistance;

				return true;
			}
			else {
				Successful = true;
				return false;
			}
		}
		
		public override void Begin() {
			base.Begin();
			SetupSequence();
			sequence.RestartSequence();
		}

		public void Update() {
			sequence.UpdateTask();
		}

		/// <summary>
		/// Called whenever the statemachine changes from this state.
		/// Make sure that the NavMeshAgent is disabled.
		/// </summary>
		public override void Exit(Blackboard blackboard) {
			base.Exit(blackboard);
			// since this state use navigation/move to task, we need to try and stop the nav agent
			Utilities.StopHexNavAgentMaybe(blackboard);
		}
	}
}