
using HexMap;
using HexMap.Pathfinding;
using Porter.ProblemSolving;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {

	public class BuildSomethingState : IState {

		public HumanColony colony;

		public GameObject buildablePrefab;

		//public static readonly string Key = "BuildSomething";

		//public override string UniqueKey() { return Key; }

		MoveToTargetTask moveToTargetTask;
		Sequence sequence;
		
		GameObject currentBuildable;

		PrimitiveAction currentAction;

		HarvestableTree currentCarriedTree = null;

		private Blackboard blackboard;

		private void Start() {
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			SetupSequence();

		}

		private void SetupSequence() {

			if (sequence == null) {
				moveToTargetTask = new MoveToTargetTask(blackboard,6f);

				sequence = new Sequence(true);
				sequence.Add(new Gig(ChooseNewBuilding));
				sequence.Add(new Gig(ChooseNextAction));
				sequence.Add(new Gig(MoveToTargetGig));
				sequence.Add(new Gig(PerformAction));

			}
		}
		
		private bool ChooseNewBuilding( ) {

			if(currentBuildable==null) {
				Vector3 position = HexNavMeshManager.GetAnyPointInArea(transform.position, 10);
				currentBuildable = Instantiate(buildablePrefab);
				currentBuildable.transform.position = HexNavMeshManager.WorldPosToWorldPosWithGround(position);
				currentBuildable.SetActive(true);
				currentBuildable.GetComponent<Buildable>().FinishSetting();

			}

			return true;
		}

		private bool ChooseNextAction( ) {

			CollectResources problem = new CollectResources();

			var state = new WorldState<Entity, ResourcesEntityState>();

			var agent = new Entity() {
				uniqueName = "Agent",
				type = EntityType.Unit,

			};
			var agentState = ResourcesEntityState.New( 0, transform.position);

			state.SetAgent(agent, agentState);

			var harvestables = QueryManager.GetNearbyHarvestables(transform.position, 100);

			foreach ( var harvestable in harvestables ) {
				var treeComponent = harvestable.GetComponent<HarvestableTree>();

				var key = Entity.NewHarvestable(treeComponent.name, harvestable);
				var entityState = ResourcesEntityState.New(treeComponent.value, harvestable.transform.position);
				state.entities.Add(key, entityState);

				if ( treeComponent == currentCarriedTree ) {
					agentState.entityCurrentlyHauling = key;
				}
			}

			// add the target building
			var targetStructure = Entity.NewUnbuiltStructure("Target", currentBuildable);
			{
				var buildable = currentBuildable.GetComponent<Buildable>();
				float buildCost = buildable.buildCost;
				var entityState = ResourcesEntityState.New(buildCost, currentBuildable.transform.position);
				entityState.built = buildable.Built;
				state.entities.Add(targetStructure, entityState);
			}

			currentAction = problem.GetBestNextAction(targetStructure, agent, state);
			Debug.Log(currentAction.AsString());
			return true;
		}

		private bool MoveToTargetGig( ) {
			moveToTargetTask.TargetTransform = currentAction.target.gameObject.transform;
			return moveToTargetTask.UpdateTask();
		}

		private bool PerformAction( ) {
			// TODO: perform checks that we are able to do the aciton
			switch (currentAction.type) {
				case ActionType.Harvest:
					currentCarriedTree = currentAction.target.gameObject.GetComponent<HarvestableTree>();
					currentCarriedTree.SetBeingCarried(gameObject.GetUnitEssence());

					blackboard.Play(AnimationKey.Speaks, new AnimationData() { data = "Come here you bastard!" });
					break;
				case ActionType.Deposit: {
						currentCarriedTree.SetNotBeingCarried();
						var buildable = currentBuildable.GetComponent<Buildable>();
						buildable.buildCost -= currentCarriedTree.value;
						Destroy(currentCarriedTree.gameObject);
					}
					break;
				case ActionType.Build: {
						var buildable = currentBuildable.GetComponent<Buildable>();
						buildable.Build();
						currentBuildable = null;
					}
					break;
				case ActionType.None:
					break;
			}
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