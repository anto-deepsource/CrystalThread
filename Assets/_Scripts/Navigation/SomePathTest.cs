using System;
using HexMap;
using HexMap.Pathfinding;
using UnityEngine;

public class SomePathTest : MonoBehaviour {

	public HexMap.HexMap map;

	public float radius = 30f;

	public Transform from;
	public Transform to;

	public Transform status;

	public LineRenderer lineRenderer;

	public float lineOffset = 1;

	public void Start() {
		from.position = transform.position + UnityEngine.Random.insideUnitCircle.FromXZ() * radius;
		to.position = transform.position + UnityEngine.Random.insideUnitCircle.FromXZ() * radius;

		var startPosition = map.WorldPositionToAxialCoords(from.position);
		var endPosition = map.WorldPositionToAxialCoords(to.position);
		GlobalPathManager.RequestPath(startPosition, endPosition,
				RequestPathCallback);
		status.name = "Status: Path Requested";
		//GlobalPathManager.RequestPath()
	}

	private void RequestPathCallback(PathJobResult<Vector2Int> work) {
		if ( work.Exists) {
			var path = work.GetPath();
			lineRenderer.positionCount = path.Count;
			for (int i = 0; i < path.Count; i++) {
				Vector3 point = path[i];
				float height = map.Metrics.XZPositionToHeight(point, true);
				lineRenderer.SetPosition(i, point + Vector3.up * lineOffset + Vector3.up * height);
			}

			status.name = "Status: Path Found and Rendered";
		} else {
			status.name = "Status: Path does not exist";
		}
		

		Destroy(gameObject, 20);
	}
}
