
using HexMap;
using HexMap.Pathfinding;
using Porter.ProblemSolving;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {

	public class WanderState : IState {


		public HumanColony colony;

		public float waitLength = 12;

		//public static readonly string Key = "Wander";

		//public override string UniqueKey() { return Key; }

		MoveToTargetTask moveToTargetTask;
		Sequence sequence;

		private float standAroundTime = 0;

		private bool initialized = false;

		private Blackboard blackboard;
		
		private void Start() {
			Initialize();
		}

		private void Initialize() {
			if (initialized) {
				return;
			}
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			moveToTargetTask = new MoveToTargetTask(blackboard, 6f);
			moveToTargetTask.speedModifier = .5f;
			sequence = new Sequence(true);
			sequence.Add(new Gig(PickTarget));
			sequence.Add(new Gig(MoveToPoint));
			sequence.Add(new Gig(StandThereForAWhile));
			initialized = true;
		}

		private bool PickTarget( ) {

			var center = colony.transform.position;
			var radius = colony.radius;

			var nearbyFriends = QueryManager.GetNearbyUnitsInFaction(center, radius, colony.faction );

			if (nearbyFriends.Length == 0) {
				PointToAnyPlaceInTheColony();

			} else {

				var closestFriend = nearbyFriends[0];

				moveToTargetTask.TargetTransform = closestFriend.transform;

				//moveToTargetTask.TargetPoint = 
				//	HexNavMeshManager.GetAnyPointInArea(closestFriend.transform.position, 6);


			}


			standAroundTime = waitLength;

			return true;
		}

		private void PointToAnyPlaceInTheColony() {
			var center = colony.transform.position;
			var radius = colony.radius;

			moveToTargetTask.TargetPoint = HexNavMeshManager.GetAnyPointInArea(center, radius);
		}

		private bool MoveToPoint( ) {
			return moveToTargetTask.UpdateTask();
		}

		private bool StandThereForAWhile() {
			standAroundTime -= Time.deltaTime;
			return standAroundTime<=0f;
		}

		public override void Begin() {
			base.Begin();
			Initialize();
			sequence.RestartSequence();
		}

		private void Update() {
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