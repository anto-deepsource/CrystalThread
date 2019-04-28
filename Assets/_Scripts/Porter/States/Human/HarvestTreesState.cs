
using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
	
	public class HarvestTreesState : IState {

		public HumanColony colony;

		//public static readonly string Key = "HarvestTrees";

		//public override string UniqueKey() { return Key; }

		MoveToTargetTask moveToTargetTask;
		Sequence sequence;

		GameObject currentTargetObject = null;
		HarvestableTree currentCarriedTree = null;

		private Blackboard blackboard;

		private void Start() {
			SetupSequence();
		}

		private void SetupSequence() {
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}

			if (sequence == null) {
				sequence = new Sequence(true);
				sequence.Add(new Gig(PickTarget));
				moveToTargetTask = new MoveToTargetTask(blackboard,6f);
				sequence.Add(moveToTargetTask);
				sequence.Add(new Gig(PickUpTree));
				sequence.Add(new Gig(ChooseReturnPoint));
				sequence.Add(new Gig(TakeTreeBackToProcessingPoint));
				sequence.Add(new Gig(DropTreeAtProcessingPoint));
			}
		}

		private bool stillPickingTarget = false;

		private bool PickTarget( ) {

			var center = colony.transform.position;
			var radius = colony.radius;

			var nearbyHarvestables = QueryManager.GetNearbyHarvestables(center, radius);

			//foreach( var harvestable in nearbyHarvestables) {

			//}
			currentTargetObject = nearbyHarvestables[0];
			moveToTargetTask.TargetTransform = currentTargetObject.transform;
			return true;
		}
		
		private bool PickUpTree( ) {

			// TODO: check whether we are in an appropriate position to carry the tree
			currentCarriedTree = currentTargetObject.GetComponent<HarvestableTree>();
			currentCarriedTree.SetBeingCarried(gameObject.GetUnitEssence());
			blackboard.Play(AnimationKey.Speaks, new AnimationData() { data = "Come here you bastard!" });
			return true;
		}

		private bool ChooseReturnPoint( ) {
			var center = transform.position;
			var nearbyProcessingPoints = 
				QueryManager.GetNearbyFriendlyProcessingPoint(center, colony.faction);
			// TODO: choose the one that is nearest considering pathing, not just how the crow flys
			currentTargetObject = nearbyProcessingPoints[0];
			moveToTargetTask.TargetTransform = currentTargetObject.transform;
			return true;
		}

		private bool TakeTreeBackToProcessingPoint( ) {
			return moveToTargetTask.UpdateTask();
		}

		private bool DropTreeAtProcessingPoint( ) {

			currentCarriedTree.SetNotBeingCarried();
			var processingPoint = currentTargetObject.GetComponent<MaterialProcessingPoint>();
			processingPoint.ProcessTree(currentCarriedTree);
			return true;
		}

		public override void Begin() {
			base.Begin();
			SetupSequence();
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