using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter.ProblemSolving {


	public class WorldState<KeyType,DataType> where DataType : BasicEntityState {

		public DataType agentState;

		public Dictionary<KeyType, DataType> entities = new Dictionary<KeyType, DataType>();

		
		public WorldState() {
			//agentCurrentPosition = Vector3.zero;
			//money = 0;
		}

		public void SetAgent(KeyType key, DataType entityState) {
			agentState = entityState;
			entities[key] = entityState;
		}

		public WorldState<KeyType, DataType> Copy() {
			WorldState<KeyType, DataType> copy = new WorldState<KeyType, DataType>();

			copy.agentState = (DataType) agentState.Copy();

			foreach( var entityPair in entities ) {
				copy.entities[entityPair.Key] = (DataType) entityPair.Value.Copy();
			}

			//copy.agentCurrentPosition = agentCurrentPosition;
			//copy.money = money;
			//foreach( var key in harvestables.Keys ) {
			//	var value = harvestables[key];
			//	copy.harvestables.Add(key, value);
			//}
			//foreach (var key in unbuiltStructures.Keys) {
			//	var value = unbuiltStructures[key];
			//	copy.unbuiltStructures.Add(key, value);
			//}
			//foreach (var key in units.Keys) {
			//	var value = units[key];
			//	copy.units.Add(key, value);
			//}
			return copy;
		}
	}

	public class BasicEntityState {
		public Vector3 position;
		public float userValue;

		public static BasicEntityState WithValue(float value) {
			return new BasicEntityState() {
				userValue = value,
			};
		}
		
		public static BasicEntityState New(float value, Vector3 position) {
			return new BasicEntityState() {
				userValue = value,
				position = position
			};
		}

		public virtual BasicEntityState Copy() {
			var copy = new BasicEntityState {
				position = position,
				userValue = userValue
			};
			return copy;
		}
	}
	
}