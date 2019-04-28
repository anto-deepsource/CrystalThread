using HexMap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Porter {

	/// <summary>
	/// Tracks and publishes events about other units this npc is aware of.
	/// 
	/// -Npcs track enemies based on states:
	//	-Unaware of
	//	-Alerted to

	// the npc will 'notice' an enemy and upgrade its status of that enemy to 'alerted to'
	//	-if the enemy enters into the npcs view plus line of sight
	//	-if the enemy uses almost any ability, attack, or jumps while within a certain ditance
	//	-if a nearby ally engages the enemy and sends an alert
	//-during 'alerted to' the npc tracks the last know position of the enemy, not that enemies' real time position
	//	-the npc could project a possible path and try to anticipate the enemy
	//	-the enemy somehow goes invisible
	//	-the npc is blinded
	//	-the enemy leaves line of sight
	//	-the enemy gets a certain distance away
	//-the status can go back from 'alerted to' to removed from the npc's awareness if:
	//	-the enemy gets a certain distance away
	/// </summary>
	[RequireComponent(typeof(RadiusIndicator))]
	public class Awareness : MonoBehaviour {

		RadiusIndicator radiusIndicator;
		UnitEssence essence;
		public PolyShape shape;

		/// <summary>
		/// The distance that, no matter whether in front or in back, this unit will become aware of other units.
		/// </summary>
		public float immediateDistance = 4;

		[Tooltip("The furthest distance away this unit will remember another unit.")]
		public float furthestDistance = 40;

		[Tooltip("The number of seconds that this unit will take before forgeting a unit that they haven't seen in that long.")]
		public float forgetTime = 15f;

		/// <summary>
		/// For debugging
		/// </summary>
		//public List<RaycastHit> sightCasts = new List<RaycastHit>();

		// TODO: make this a dictionary of unit essences
		public Dictionary<GameObject, AwarenessState> units = new Dictionary<GameObject, AwarenessState>();

		private List<UnitEssence> engagingEnemies = new List<UnitEssence>();

		public bool EnemiesNearby {
			get {
				return engagingEnemies.Count > 0;
			}
		}
		
		/// <summary>
		/// The focused enemy, if there is one. Updated internally and calculated using various factors
		/// </summary>
		public GameObject MainEnemyTarget { get; private set; }

		public Vector3 MainEnemyPosition {
			get {
				return GetLastKnownPosition(MainEnemyTarget);
			}
		}

		// Use this for initialization
		void Start() {
			radiusIndicator = GetComponent<RadiusIndicator>();
			essence = gameObject.GetUnitEssence();
			shape = Utilities.PolyShape(gameObject);

			essence.Events.Add(this, EssenceEventCallback);
		}

		private void Update() {
			UpdateNearbyUnits();
			UpdateKnownUnits();
			UpdateMainTarget();
		}

		private void OnGUI() {
			
		}

		private void EssenceEventCallback( int code, object data ) {
			switch ((BlackboardEventType)code) {
				case BlackboardEventType.Staggered:
					break;
				case BlackboardEventType.StaggeredEnd:
					break;
				case BlackboardEventType.Damaged:
					DamageSource source = (DamageSource)data;
					var sourceUnit = source.sourceObject.GetUnitEssence();
					if ( sourceUnit!=null && sourceUnit!=essence) {
						UpdateKnowledgeAboutUnit(sourceUnit);
					}
					
					break;
				case BlackboardEventType.ResourcesAdded:
					break;
				case BlackboardEventType.ResourcesRemoved:
					break;
				case BlackboardEventType.Death:
					break;
				case BlackboardEventType.Incapacitated:
					break;
			}
		}

		private void UpdateNearbyUnits() {
			//sightCasts.Clear();

			// query any nearby enemy units
			GameObject[] nearbyUnits =
				QueryManager.GetNearbyUnits(transform.position, radiusIndicator.radius);

			// check each one to see if they are in our line of sight
			foreach (var unit in nearbyUnits) {
				PolyShape unitShape = Utilities.PolyShape(unit);
				var unitEssence = unit.GetUnitEssence();
				//var theirBlackboard = unit.GetComponent<Blackboard>();
				
				if (CommonUtils.DistanceSquared(unit.transform.position, transform.position) <
						immediateDistance * immediateDistance ||
						ColDet.PolyShapeAndPolyShape(unitShape, shape)
						) {
					// do a raycast 
					RaycastHit sightHit;
					Vector3 start = essence.transform.position + essence.height * 0.5f * Vector3.up;
					// try to get a collider and use that as the end point
					Collider targetCollider = unit.GetComponent<Collider>();
					Vector3 end = targetCollider ? targetCollider.bounds.center : unit.transform.position;
					Vector3 direction = (end - start).normalized;
					//Debug.DrawRay( start, direction * radiusIndicator.radius, Color.white, 0.5f, true);
					if (Physics.Raycast(start, direction, out sightHit, radiusIndicator.radius,
							HexNavMeshManager.SteerLayer)) {
						// if we didn't hit anything or the thing we hit wasn't the unit in question then we can't see this unit
						if (sightHit.collider.gameObject == unit) {

							UpdateKnowledgeAboutUnit(unitEssence);
							
						}
						//sightCasts.Add(sightHit);
					}

					
				}
			}
		}

		private void UpdateKnowledgeAboutUnit(UnitEssence unit) {

			if (unit.IsDead) {
				// TODO: investigate dead body
				SetUnitStatusAlertedTo(unit.gameObject);
			} else {
				// the unit is alive

				// add this unit to our map and store its last known position
				if (FactionUtils.IsNotA(unit.faction, essence.faction)) {
					SetUnitStatusEngaging(unit.gameObject);

				}
				else {
					SetUnitStatusAlertedTo(unit.gameObject);
				}
			}

			
		}

		/// <summary>
		/// Reevaluates already known units:
		/// -Forgets any that have died
		/// -Forgets any that have become a certain distance away
		/// -Forgets any that have a last known position nearby but have not been seen in a while
		/// </summary>
		private void UpdateKnownUnits() {
			var removable = new List<GameObject>();

			foreach (var pair in units ) {
				var unit = pair.Key;
				var status = pair.Value;
				if ( status.destroyed ) {
					removable.Add(unit);
					continue;
				}
				//var theirBlackboard = unit.GetComponent<Blackboard>();
				var unitEssence = unit.GetUnitEssence();
				if (unitEssence.IsDead ) {
					//removable.Add(unit);
					SetUnitStatusAlertedTo(unit);
				} else
				if( Vector3.SqrMagnitude( unit.transform.position - transform.position )
						> furthestDistance * furthestDistance ) {
					SetUnitStatusUnawareOf(unit);
				} else {
					var state = pair.Value;
					bool nearLastKnownPosition = Vector3.SqrMagnitude(state.lastKnownPosition - transform.position)
														< immediateDistance * immediateDistance;
					if (nearLastKnownPosition && state.TimeSinceLastSeen() > forgetTime ) {
						SetUnitStatusUnawareOf(unit);
					}
				}
			}

			foreach( var unit in removable ) {
				units.Remove(unit);
			}
		}
		
		private void UpdateMainTarget( ) {
			if (!EnemiesNearby) {
				MainEnemyTarget = null;
				return;
			}

			// Choose the main target based on distance to self
			float minDistance = 2f * furthestDistance * furthestDistance;
			foreach (var unit in engagingEnemies) {
				float distance = Vector3.SqrMagnitude(unit.transform.position - transform.position);

				if ( distance < minDistance ) {
					minDistance = distance;
					MainEnemyTarget = unit.gameObject;
				}
			}
		}

		/// <summary>
		/// Returns any enemies that are currently marked as 'Engaging"
		/// </summary>
		/// <returns></returns>
		//public IEnumerable<GameObject> GetAnyEngaging() {
		//	List<GameObject> results = (
		//	from awarenesState in units.Values
		//	where awarenesState.status == AwarenessStatus.Engaging
		//	select awarenesState.unit
		//	   ).ToList<GameObject>();

		//	return results.ToArray();

		//	foreach (var unit in units.Keys) {
		//		//if (units[unit] == AwarenessStatus.Engaging) {
		//		yield return unit;
		//		//}
		//	}
		//}

		public Vector3 GetLastKnownPosition(GameObject unit) {
			AwarenessState awarenessState;
			if (units.TryGetValue(unit, out awarenessState)) {
				return awarenessState.lastKnownPosition;
			}
			throw new System.Exception(string.Format("{0} was not in known units.", unit));
		}

		/// <summary>
		/// Returns any enemies that are currently marked as 'Engaging"
		/// </summary>
		/// <returns></returns>
		public UnitEssence[] GetAnyNearbyFriendlies() {
			List<UnitEssence> results = (
			from awarenesState in units.Values
			where awarenesState.status == AwarenessStatus.AlertedTo
				&& FactionUtils.IsA(awarenesState.unit.faction, essence.faction)
			select awarenesState.unit
			   ).ToList<UnitEssence>();

			return results.ToArray();
			
		}

		/// <summary>
		/// Sets the status of the given unit to 'Alerted To'.
		/// Updates that unit's last know position.
		/// If the status of the given unit was previously 'Unaware of'
		/// fires an event.
		/// </summary>
		private void SetUnitStatusAlertedTo(GameObject unit ) {
			var unitEssence = unit.GetUnitEssence();
			AwarenessState currentState;
			if ( units.TryGetValue(unit, out currentState)) {
				if (currentState.status == AwarenessStatus.Engaging) {
					engagingEnemies.Remove(currentState.unit);
				}
				currentState.UpdateLastKnownPosition( unit.transform.position );
			} else {
				currentState = units[unit] = new AwarenessState(unitEssence, unit.transform.position);
			}
			currentState.status = AwarenessStatus.AlertedTo;

		}
		
		private void SetUnitStatusUnawareOf(GameObject unit) {
			AwarenessState currentState;
			if (units.TryGetValue(unit, out currentState)) {
				if ( currentState.status == AwarenessStatus.Engaging) {
					engagingEnemies.Remove(currentState.unit);
				}
				currentState.status = AwarenessStatus.UnawareOf;
			}
		}

		private void SetUnitStatusEngaging(GameObject unit) {
			var unitEssence = unit.GetUnitEssence();
			AwarenessState currentState;
			if (units.TryGetValue(unit, out currentState)) {
				currentState.UpdateLastKnownPosition(unit.transform.position);
			}
			else {
				currentState = units[unit] = new AwarenessState(unitEssence, unit.transform.position);
			}

			if (currentState.status != AwarenessStatus.Engaging) {
				currentState.SetStatusAtLeast(AwarenessStatus.Engaging);
				engagingEnemies.Add(currentState.unit);
			}
		}

		

		//public void AddToStateRepresentation(WorldState<EntityType, BasicEntityState> state) {

		//	foreach ( var unitPair in units) {
		//		var awarenessState = unitPair.Value;
		//		var unitObject = unitPair.Key;
		//		var entityState = BasicEntityState.New(0, unitObject.transform.position);
		//		//var entityKey = EntityType.Unit("");
		//	}
		//}
	}

	public enum AwarenessStatus {
		UnawareOf,
		AlertedTo,
		Engaging,
	}

	// data about what unit 
	public class AwarenessState {

		public UnitEssence unit;

		public Vector3 lastKnownPosition;

		public float lastSeenTime;

		public AwarenessStatus status;

		public bool destroyed = false;

		public AwarenessState( UnitEssence unit, Vector3 lastKnownPosition) {
			this.unit = unit;
			this.unit.Events.Add(this, OtherEssenceEventCallback);
			UpdateLastKnownPosition( lastKnownPosition);
		}
		
		public void UpdateLastKnownPosition( Vector3 position ) {
			lastKnownPosition = position;
			lastSeenTime = Time.time;
			SetStatusAtLeast( AwarenessStatus.AlertedTo);
		}

		public float TimeSinceLastSeen() {
			return Time.time - lastSeenTime;
		}

		public void SetStatusAtLeast( AwarenessStatus newStatus ) {
			status = (AwarenessStatus) Mathf.Max((int)status, (int)newStatus);
		}

		public void SetStatusAtMost(AwarenessStatus newStatus) {
			status = (AwarenessStatus)Mathf.Min((int)status, (int)newStatus);
		}

		private void OtherEssenceEventCallback(int eventCode, object data) {
			switch ((BlackboardEventType)eventCode) {
				case BlackboardEventType.GameobjectDestroyed:
					destroyed = true;
					this.unit.Events.RemoveKey(this);
					break;
			}
		}
	}
}