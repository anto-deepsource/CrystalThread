
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
		public bool showMoveVector = false;

		public float yOffset = 0.5f;

		public HexMap hexMap;

		//private HexNavAgent agent;
		private LineRenderer lineRenderer;

		private HexNavAgent localAgent;



		// Use this for initialization
		void Start() {
			lineRenderer = GetComponent<LineRenderer>();
		}

		// Update is called once per frame
		void Update() {
			//gameObject.GetComponentMaybe<HexNavAgent>(ref agent);
			gameObject.GetComponentMaybe<HexNavAgent>(ref localAgent);

			if ( localAgent == null ) {
				localAgent = gameObject.GetComponentInParent<HexNavAgent>();
			}

			if ( localAgent == null ) {
				return;
			}

			if ( hexMap == null ) {
				hexMap = HexNavMeshManager.GetHexMap();
			}

			lineRenderer.positionCount = 1;

			lineRenderer.SetPosition(0, transform.position);

			if (showMoveVector && localAgent != null) {
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(1, transform.position + localAgent.MoveVector );
			}

			if (showLocal && localAgent != null ) {

				int startIndex = lineRenderer.positionCount;
				lineRenderer.positionCount = startIndex + localAgent.LocalPath.Path.Count;
				for (int i = 0; i < localAgent.LocalPath.Path.Count; i++) {
					Vector3 point = localAgent.LocalPath.Path[i];
					float height = hexMap.Metrics.XZPositionToHeight(point, true);
					lineRenderer.SetPosition(i + startIndex, point + Vector3.up * yOffset + Vector3.up * height);
				}
			}

			if (showGlobal && localAgent != null ) {
				int startIndex = lineRenderer.positionCount;
				lineRenderer.positionCount = startIndex + localAgent.GlobalPath.Path.Count;
				for (int i = 0; i < localAgent.GlobalPath.Path.Count; i++) {
					Vector3 point = localAgent.GlobalPath.Path[i];
					float height = hexMap.Metrics.XZPositionToHeight(point, true);
					lineRenderer.SetPosition(i + startIndex, point + Vector3.up * yOffset + Vector3.up * height);
				}

			}

			
		}
		
	}
}