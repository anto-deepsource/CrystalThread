using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObjectSpawner : MonoBehaviour {

	public float radius = 15f;

	public int targetNumObjects = 20;

	public GameObject prefabObject;
	public GameObject folder;

	public List<GameObject> childObjects;

	public LayerMask hitLayer;

	public float baseScale = 1f;
	public float scaleVariance = 0.25f;

	private void Start() {
		SpawnObjects();
	}

	public void SpawnObjects() {
		DestroyChildObjects();

		GameObject parentFolder = folder != null ? folder : gameObject;

		var hexMap = HexNavMeshManager.GetHexMap();

		for ( int i = 0; i < targetNumObjects; i ++ ) {
			GameObject spawn = Object.Instantiate(prefabObject, parentFolder.transform);

			float scale = baseScale + ((Random.value * 2f - 1f) * scaleVariance);
			spawn.transform.localScale = new Vector3(scale, scale, scale);

			if (PlaceInRandomBuildablePlace(spawn)) {
				childObjects.Add(spawn);
				hexMap.NotifyOfStaticObstacleAdd(spawn);
			} else {
				Debug.LogWarning("Couldn't find a place for this one.");
				// Destroy the object this time
				if (!Application.isPlaying) {
					DestroyImmediate(spawn);
				}
				else {
					Destroy(spawn);
				}
			}
			
		}

		
	}

	private bool PlaceInRandomBuildablePlace(GameObject spawn, int triesLeft = 20) {
		var position = HexNavMeshManager.GetAnyPointInArea(transform.position, radius);
		position = HexNavMeshManager.WorldPosToWorldPosWithGround(position);
		spawn.transform.position = position;
		
		float rotation = Random.value * 360f;
		spawn.transform.rotation = Quaternion.Euler(0, rotation, 0);

		var polyShape = spawn.GetComponentInChildren<PolyShape>();
		
		if (polyShape == null) {
			return true;
		} else
		if (!HexNavMeshManager.CheckIsBuildablePosition(polyShape)) {
			if ( triesLeft == 0 ) {
				return false;
			}
			return PlaceInRandomBuildablePlace(spawn, triesLeft - 1);
		} else {
			return true;
		}
	}

	private float GetGroundPoint(Vector3 worldXZ) {
		float distance = 10000;
		RaycastHit hitRayHit;
		if ( Physics.Raycast(worldXZ.DropY() + Vector3.up * distance, Vector3.down,
			out hitRayHit, distance* distance, hitLayer)) {
			return hitRayHit.point.y;
		}
		return HexNavMeshManager.XZPositionToHeight(worldXZ, true);
	}

	private void DestroyChildObjects() {

		var hexMap = HexNavMeshManager.GetHexMap();

		foreach ( var child in childObjects ) {
			hexMap.NotifyOfStaticObstacleRemove(child);
			if ( !Application.isPlaying ) {
				DestroyImmediate(child);
			} else {
				Destroy(child);
			}
			
		}
		childObjects.Clear();
	}
}
