
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HexMap.Pathfinding {
	[RequireComponent(typeof(LineRenderer))]
	public class HexNavPathDrawer : MonoBehaviour {

		public bool showGlobal = false;
		public bool showLocal = true;

		public float yOffset = 0.5f;

		public HexMap hexMap;

		private HexNavAgent agent;
		private LineRenderer lineRenderer;

		private HexNavLocalAgent localAgent;



		// Use this for initialization
		void Start() {
			lineRenderer = GetComponent<LineRenderer>();
		}

		// Update is called once per frame
		void Update() {
			gameObject.GetComponentMaybe<HexNavAgent>(ref agent);
			gameObject.GetComponentMaybe<HexNavLocalAgent>(ref localAgent);

			if (showGlobal && agent != null && agent.Status==PathStatus.Succeeded) {

				lineRenderer.positionCount = agent.GlobalPathPoints.Count;
				for (int i = 0; i < agent.GlobalPathPoints.Count; i++) {
				Vector3 worldCoords = hexMap.AxialCoordsToWorldPositionWithHeight(agent.GlobalPathPoints[i]);
					lineRenderer.SetPosition(i, worldCoords + Vector3.up * yOffset);
				}

			} else {
				lineRenderer.positionCount = 0;
			}

			if (showLocal && localAgent != null && 
				(localAgent.Status == PathStatus.Succeeded || localAgent.Status == PathStatus.Partial)) {

				lineRenderer.positionCount = localAgent.Path.Count;
				for (int i = 0; i < localAgent.Path.Count; i++) {
					Vector3 point = localAgent.Path[i];
					float height = hexMap.Metrics.XZPositionToHeight(point, true);
					lineRenderer.SetPosition(i, point + Vector3.up * yOffset + Vector3.up * height);
				}

			} else {
				lineRenderer.positionCount = 0;
			}
		}
		
	}
}