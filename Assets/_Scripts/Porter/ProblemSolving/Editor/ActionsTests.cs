using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Porter.ProblemSolving;

public class ActionsTests {

	//[Test]
	//public void ActionsTestsSimplePasses() {
	//	// Use the Assert class to test conditions.

	//	CollectResources problem = new CollectResources();

	//	var agent = new Entity() {
	//		uniqueName = "Agent",
	//		type = EntityType.Unit
	//	};

	//	var targetStructure = Entity.NewUnbuiltStructure("House", 10);

	//	var state = new CollectResources.WorldState(targetStructure);

	//	var action = problem.GetBestNextAction(targetStructure, agent, state);
	//	Debug.Log(action.AsString());
	//}

	[Test]
	public void TrivialSuccessTest() {
		// Use the Assert class to test conditions.

		CollectResources problem = new CollectResources();
		
		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, new ResourcesEntityState() );

		var targetStructure = Entity.NewUnbuiltStructure("House" );

		
		state.entities.Add(targetStructure, new ResourcesEntityState());

		var action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		//Assert.AreEqual( 0, action.expectedValue);
	}

	[Test]
	public void TrivialNoSuccessTest() {
		// Use the Assert class to test conditions.

		CollectResources problem = new CollectResources();


		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0) );

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(999));

		var action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.None, action.type);
		Assert.AreEqual(-Mathf.Infinity, action.expectedValue);
	}

	[Test]
	public void ProceedingStateTest1() {
		// Use the Assert class to test conditions.

		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		var harvestable = Entity.NewHarvestable("Tree");
		state.entities.Add(harvestable, ResourcesEntityState.WithValue(10f));

		var action = new PrimitiveAction() {
			type = ActionType.Harvest,
			target = harvestable
		};

		var proceedingState = problem.GetProceedingState(action, agent, state);
		Assert.IsNotNull(proceedingState.agentState.entityCurrentlyHauling);
		Assert.AreEqual(harvestable, proceedingState.agentState.entityCurrentlyHauling);
		//Assert.AreEqual(0f, proceedingState.harvestables[harvestable]);
		//Assert.IsFalse(proceedingState.entities.ContainsKey(harvestable));

		Assert.AreNotEqual(state, proceedingState);
		Assert.IsNull(state.agentState.entityCurrentlyHauling);
		//Assert.AreEqual(0f, state.agentState.userValue);
		//Assert.AreEqual(10f, state.entities[harvestable].userValue);
	}

	[Test]
	public void TrivialUtilityTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		var harvestable = Entity.NewHarvestable("Tree");
		state.entities.Add(harvestable, ResourcesEntityState.WithValue(10f));

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(999));

		var action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Harvest, action.type);
		Assert.AreEqual(harvestable, action.target);
		//Assert.AreEqual(problem.Heuristic(10f, 999 ), action.expectedValue);
	}

	[Test]
	public void BuildStructureTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		var harvestable = Entity.NewHarvestable("Tree");
		state.entities.Add(harvestable, ResourcesEntityState.WithValue(10f));

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(10));

		var action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Harvest, action.type);
		Assert.AreEqual(harvestable, action.target);

		var proceedingState = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, proceedingState);
		Assert.AreEqual(ActionType.Deposit, action.type);
		Assert.AreEqual(targetStructure, action.target);

		proceedingState = problem.GetProceedingState(action, agent, proceedingState);
		action = problem.GetBestNextAction(targetStructure, agent, proceedingState);
		Assert.AreEqual(ActionType.Build, action.type);
		Assert.AreEqual(targetStructure, action.target);
	}

	[Test]
	public void TwoActionTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		var harvestable = Entity.NewHarvestable("Tree");
		state.entities.Add(harvestable, ResourcesEntityState.WithValue(10f));

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(10));

		var action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Harvest, action.type);
		Assert.AreEqual(harvestable, action.target);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Deposit, action.type);
		Assert.AreEqual(targetStructure, action.target);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		Assert.AreEqual(targetStructure, action.target);
	}

	[Test]
	public void ThreeActionTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		int count = 2;

		for( int i = 0; i < count; i ++ ) {
			var harvestable = Entity.NewHarvestable("Tree " + i);
			state.entities.Add(harvestable, ResourcesEntityState.WithValue(1f));
		}
		
		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(count));

		PrimitiveAction action;
		for( int i = 0; i < count; i ++ ) {
			action = problem.GetBestNextAction(targetStructure, agent, state);
			Assert.AreEqual(ActionType.Harvest, action.type);

			state = problem.GetProceedingState(action, agent, state);
			action = problem.GetBestNextAction(targetStructure, agent, state);
			Assert.AreEqual(ActionType.Deposit, action.type);
			Assert.AreEqual(targetStructure, action.target);

			state = problem.GetProceedingState(action, agent, state);
		}
		
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		//Assert.AreEqual(targetStructure, action.target);
		//Assert.AreEqual(problem.Heuristic(10f, 999), action.expectedValue);
	}

	[Test]
	public void NActionTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		int count = 20;

		for (int i = 0; i < count; i++) {
			var harvestable = Entity.NewHarvestable("Tree " + i);
			state.entities.Add(harvestable, ResourcesEntityState.WithValue(1f));
		}

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(count));

		PrimitiveAction action;
		for (int i = 0; i < count; i++) {
			action = problem.GetBestNextAction(targetStructure, agent, state);
			Assert.AreEqual(ActionType.Harvest, action.type);

			state = problem.GetProceedingState(action, agent, state);
			action = problem.GetBestNextAction(targetStructure, agent, state);
			Assert.AreEqual(ActionType.Deposit, action.type);
			Assert.AreEqual(targetStructure, action.target);

			state = problem.GetProceedingState(action, agent, state);
		}

		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		//Assert.AreEqual(targetStructure, action.target);
		//Assert.AreEqual(problem.Heuristic(10f, 999), action.expectedValue);
	}

	[Test]
	public void UnequalTreesTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		int count = 20;

		Entity biggestTree = Entity.NewHarvestable("Biggest Tree");
		state.entities.Add(biggestTree, ResourcesEntityState.WithValue(21));

		for (int i = 0; i < count; i++) {
			var harvestable = Entity.NewHarvestable("Little Tree " + i);
			state.entities.Add(harvestable, ResourcesEntityState.WithValue(1) );
		//	biggestTree = harvestable;
		}

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(20));

		PrimitiveAction action;
		//for (int i = 0; i < count; i++) {

		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Harvest, action.type);
		Assert.AreEqual(biggestTree, action.target);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Deposit, action.type);
		Assert.AreEqual(targetStructure, action.target);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		Assert.AreEqual(targetStructure, action.target);
		//Assert.AreEqual(problem.Heuristic(10f, 999), action.expectedValue);
	}

	//// A UnityTest behaves like a coroutine in PlayMode
	//// and allows you to yield null to skip a frame in EditMode
	//[UnityTest]
	//public IEnumerator ActionsTestsWithEnumeratorPasses() {
	//	// Use the Assert class to test conditions.
	//	// yield to skip a frame
	//	yield return null;
	//}

	[Test]
	public void DistanceCostTest() {
		CollectResources problem = new CollectResources();

		var state = new WorldState<Entity, ResourcesEntityState>();

		var agent = new Entity() {
			uniqueName = "Agent",
			type = EntityType.Unit,
			
		};
		state.SetAgent(agent, ResourcesEntityState.WithValue(0));

		{
			var harvestable = Entity.NewHarvestable("Tree Further 1");
			state.entities.Add(harvestable, ResourcesEntityState.New(10f, new Vector3(10, 10, 10)));
		}

		var closerPosition = new Vector3(1, 1, 1);
		var closerHarvestable = Entity.NewHarvestable("Tree Closer");
		state.entities.Add(closerHarvestable, ResourcesEntityState.New(10f, closerPosition));

		{
			var harvestable = Entity.NewHarvestable("Tree Further 2");
			state.entities.Add(harvestable, ResourcesEntityState.New(10f, new Vector3(10, 10, 10)));
		}

		var targetStructure = Entity.NewUnbuiltStructure("House");
		state.entities.Add(targetStructure, ResourcesEntityState.WithValue(10));

		PrimitiveAction action;
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Harvest, action.type);
		Assert.AreEqual(closerHarvestable.uniqueName, action.target.uniqueName);
		Assert.AreEqual(closerPosition, state.entities[action.target].position);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Deposit, action.type);
		Assert.AreEqual(targetStructure, action.target);

		state = problem.GetProceedingState(action, agent, state);
		action = problem.GetBestNextAction(targetStructure, agent, state);
		Assert.AreEqual(ActionType.Build, action.type);
		Assert.AreEqual(targetStructure, action.target);
	}
}
