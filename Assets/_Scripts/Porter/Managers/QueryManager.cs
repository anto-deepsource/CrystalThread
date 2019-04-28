using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class QueryManager : Singleton<QueryManager> {
	protected QueryManager() { } // guarantee this will be always a singleton only - can't use the constructor!

	public HashSet<ObjectQuery> objects = new HashSet<ObjectQuery>();

	public static void RegisterObject(ObjectQuery newObject ) {
		Instance.objects.Add(newObject);
	}

	public static void UnregisterObject(ObjectQuery newObject) {
		QueryManager manager = Instance;
		if (manager != null) {
			manager.objects.Remove(newObject);
		}
	}

	[Obsolete("Use GetPlayer( out player ) which returns true/false whether a single player object was found.")]
	public static GameObject GetPlayer() {
		List<GameObject> results = (
			from obj in Instance.objects
			   where obj.gameObject.CompareTag("Player")
			   select obj.gameObject
			   ).ToList<GameObject>();
		if ( results.Count != 1 ) {
			Debug.Log("Query Player had " + results.Count + " results");

		}
		return results[0];
	}

	public static bool GetPlayer( out GameObject player, bool failOnMoreThanOne = false ) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isPlayer
			select obj.gameObject
			   ).ToList<GameObject>();
		if (failOnMoreThanOne && results.Count > 1) {
			//Debug.Log("Query Player had " + results.Count + " results");
			player = null;
			return false;
		}
		if ( results.Count == 0) {
			//Debug.Log("Query Player had " + results.Count + " results");
			player = null;
			return false;
		}
		player = results[0];
		return true;
	}

	public static GameObject[] GetNearbyUnits( Vector3 position, float range ) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isUnit
				&& CommonUtils.DistanceSquared(position, obj.gameObject.transform.position) <= range * range
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyUnitsInFaction(Vector3 position, float range, Faction faction) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isUnit
				&& FactionUtils.IsA(obj.faction, faction)
				&& CommonUtils.DistanceSquared(position, obj.gameObject.transform.position) <= range * range
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}


	public static GameObject[] GetNearbyUnitsNotInFaction(Vector3 position, float range, Faction faction) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isUnit
				&& FactionUtils.IsNotA( obj.faction, faction )
				&& CommonUtils.DistanceSquared(position, obj.gameObject.transform.position) <= range * range
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyPickups(Vector3 position, float range) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isPickup
			orderby (obj.gameObject.transform.position - position ).sqrMagnitude ascending
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyHarvestables(Vector3 position, float range) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isHarvestable
			orderby (obj.gameObject.transform.position - position).sqrMagnitude ascending
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyResourceables(Vector3 position, float range) {
		float rangedSquared = range * range;
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isResourceable 
			&& (obj.gameObject.transform.position - position).sqrMagnitude <= rangedSquared
			orderby (obj.gameObject.transform.position - position).sqrMagnitude ascending
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyResourceRecepticals(Vector3 position, float range) {
		float rangedSquared = range * range;
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isResourceReceptical
			&& (obj.gameObject.transform.position - position).sqrMagnitude <= rangedSquared
			orderby (obj.gameObject.transform.position - position).sqrMagnitude ascending
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static GameObject[] GetNearbyFriendlyProcessingPoint
			(Vector3 position, Faction faction) {
		var results = (
		from obj in Instance.objects
			where obj.gameObject.CompareTag("Material Processing Point")
				&& FactionUtils.IsA(obj.faction, faction)

			orderby (obj.gameObject.transform.position - position).sqrMagnitude ascending

			select obj.gameObject);

		return results.ToArray();
	}

	public static GameObject[] GetAllStaticObstacles( ) {
		List<GameObject> results = (
			from obj in Instance.objects
			where obj.isStaticObstacle
			select obj.gameObject
			   ).ToList<GameObject>();

		return results.ToArray();
	}

	public static bool GetClosestUnit(Vector3 position, float range, out GameObject result) {
		List<GameObject> results = (
			from obj in Instance.objects
			where CommonUtils.Distance(position, obj.gameObject.transform.position) < range
			//&& obj.team == team
			orderby CommonUtils.Distance(position, obj.gameObject.transform.position) ascending
			select obj.gameObject
			   ).ToList<GameObject>();
		if ( results.Count == 0 ) {
			result = null;
			return false;
		}
		result = results[0];
		return true;
	}
}