using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter.ProblemSolving {

	public struct PrimitiveAction {
		public ActionType type;
		public Entity target;
		public float expectedValue;

		public string AsString() {
			return string.Format("{0} {1}", type.ToString(), target.uniqueName);
		}
	}

	public enum ActionType {
		Harvest,
		Deposit,
		Build,
		None,
	}

	public enum EntityType {
		Harvestable = 1 << 1,
		Structure = 1 << 2,
		Unit = 1 << 3,
	}

	public class Entity {
		public string uniqueName;
		public EntityType type;
		public GameObject gameObject;
		//public float harvestSpeed;
		//public Vector3 position;

		public static Entity NewHarvestable(string name) {
			return new Entity() {
				uniqueName = name,
				type = EntityType.Harvestable,
			};
		}
		
		public static Entity NewUnbuiltStructure(string name) {
			return new Entity() {
				uniqueName = name,
				type = EntityType.Structure,
			};
		}

		public static Entity NewHarvestable(string name, GameObject gameObject) {
			return new Entity() {
				uniqueName = name,
				type = EntityType.Harvestable,
				gameObject = gameObject,
			};
		}
		
		public static Entity NewUnbuiltStructure(string name, GameObject gameObject) {
			return new Entity() {
				uniqueName = name,
				type = EntityType.Structure,
				gameObject = gameObject,
			};
		}
	}

	public class ResourcesEntityState : BasicEntityState {

		public Entity entityCurrentlyHauling;

		public bool built = false;

		public override BasicEntityState Copy() {
			var copy = new ResourcesEntityState {
				position = position,
				userValue = userValue,
				entityCurrentlyHauling = entityCurrentlyHauling,
				built = built
			};
			return copy;
		}
		new public static ResourcesEntityState WithValue(float value) {
			return new ResourcesEntityState() {
				userValue = value,
			};
		}
		new public static ResourcesEntityState New(float value, Vector3 position) {
			return new ResourcesEntityState() {
				userValue = value,
				position = position
			};
		}
	}

	public class CollectResources {
		
		public PrimitiveAction GetBestNextAction(Entity targetUnbuiltStructure,
				Entity agent, WorldState<Entity, ResourcesEntityState> state) {

			if (TerminalTest(targetUnbuiltStructure, agent, state) ) {
				float utility = UtilityTest(targetUnbuiltStructure, agent, state);
				return new PrimitiveAction() {
					type = ActionType.None,
					expectedValue = utility
				};
			}

			// set the bestAction to do nothing and get nothing
			PrimitiveAction bestAction = new PrimitiveAction() {
				type = ActionType.None,
				expectedValue = -Mathf.Infinity
			};

			foreach( var action in GetPossibleActions(agent, state) ) {
				WorldState<Entity, ResourcesEntityState> proceedingState = GetProceedingState(action, agent, state);

				float utility = UtilityTest(targetUnbuiltStructure, agent, proceedingState);
				float cost = Cost(action, agent, state);
				float expectedValue = utility - cost;
				if ( bestAction.expectedValue < expectedValue) {
					bestAction = action;
					bestAction.expectedValue = expectedValue;
				}
			}

			return bestAction;
		}

		public bool TerminalTest( Entity targetUnbuiltStructure, Entity agent, WorldState<Entity, ResourcesEntityState> state) {
			var entityState = state.entities[targetUnbuiltStructure];
			return entityState.built;
		}

		public float UtilityTest(Entity targetUnbuiltStructure, Entity agent, WorldState<Entity, ResourcesEntityState> state) {
			var entityState = state.entities[targetUnbuiltStructure];
			if (entityState.built) {
				return Mathf.Infinity;
			}
			float remainingCost = entityState.userValue;
			if ( remainingCost <= 0 ) {
				return 100;
			}
			var haulingEntity = state.agentState.entityCurrentlyHauling;
			if ( haulingEntity != null ) {
				float resourceValue = state.entities[haulingEntity].userValue;
				return resourceValue- remainingCost;
			}
			return -remainingCost;
			
		}
		
		public float Cost(PrimitiveAction action, Entity agent, WorldState<Entity, ResourcesEntityState> state) {
			var entityState = state.entities[action.target];
			Vector3 targetPosition = entityState.position;
			Vector3 currentPosition = state.agentState.position;
			return Vector3.SqrMagnitude(targetPosition - currentPosition);
		}

		public IEnumerable<PrimitiveAction> GetPossibleActions(Entity agent, WorldState<Entity, ResourcesEntityState> state) {
			float currentResources = state.agentState.userValue;
			Entity haulingEntity = state.agentState.entityCurrentlyHauling;
			foreach (var entityKey in state.entities.Keys) {
				var value = state.entities[entityKey];
				switch (entityKey.type) {
					case EntityType.Harvestable:
						if ( haulingEntity == null ) {
							yield return new PrimitiveAction() {
								target = entityKey,
								type = ActionType.Harvest,
							};
						}
						break;
					case EntityType.Structure:
						var cost = state.entities[entityKey].userValue;

						if ( cost <= 0 ) {
							yield return new PrimitiveAction() {
								target = entityKey,
								type = ActionType.Build,
							};
						} else
						if (haulingEntity != null) {
							yield return new PrimitiveAction() {
								target = entityKey,
								type = ActionType.Deposit,
							};
						}

						
						break;
					case EntityType.Unit:
						break;
				}
				
			}
			//foreach (var key in state.unbuiltStructures.Keys) {
			//	float currentResources = state.units[agent];
			//	var cost = state.unbuiltStructures[key];
			//	if (currentResources > 0 ) {
			//		
			//	}

			//}
			//foreach (var key in units.Keys) {
			//	var value = units[key];
			//	copy.units.Add(key, value);
			//}
		}

		public WorldState<Entity, ResourcesEntityState> GetProceedingState(PrimitiveAction action, Entity agent,
				WorldState<Entity, ResourcesEntityState> state) {

			var proceedingState = state.Copy();

			switch (action.type) {
				case ActionType.Harvest: {
						//var entityState = state.entities[action.target];
						//float value = entityState.userValue;

						//float afterHarvestedValue = entityState.userValue - value;
						//if (afterHarvestedValue <= 0) {
						//	proceedingState.entities.Remove(action.target);
						//}
						//else {
						//	entityState.userValue = afterHarvestedValue;
						//	proceedingState.entities[action.target] = entityState;
						//}
						//proceedingState.agentState.userValue += value;

						proceedingState.agentState.entityCurrentlyHauling = action.target;
					}
					break;
				case ActionType.Deposit: {
						Entity hauledEntity = state.agentState.entityCurrentlyHauling;
						float value = proceedingState.entities[hauledEntity].userValue;
						proceedingState.entities.Remove(hauledEntity);

						proceedingState.entities[action.target].userValue -= value;

						proceedingState.agentState.entityCurrentlyHauling = null;
					}
					break;
				case ActionType.Build: {
						//var entityState = state.entities[action.target];
						//float currentAgentResources = state.agentState.userValue;
						//float remainingCost = entityState.userValue;
						//float cost = Mathf.Min(currentAgentResources, remainingCost);
						//float afterBuildValue = remainingCost - cost;
						//entityState.userValue = afterBuildValue;
						proceedingState.entities[action.target].built = true;
						//proceedingState.agentState.userValue -= cost;
					}
					break;
				case ActionType.None:
					break;
			}

			return proceedingState;
		}
	}
}