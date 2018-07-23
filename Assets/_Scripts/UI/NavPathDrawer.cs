using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NavPathDrawer : MonoBehaviour {

	public float yOffset = 0.5f;

	private NavMeshAgent agent;
	private LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.GetComponentMaybe<NavMeshAgent>(ref agent);
		//AssignAgentMaybe(ref agent, GetComponent<NavMeshAgent>);
		//NavMeshAgent a = agent ?? GetComponent<NavMeshAgent>();
		//agent = a;
		////if ( agent == null ) {
		////	agent = GetComponent<NavMeshAgent>();
		////}

		if ( agent!=null && agent.pathStatus == NavMeshPathStatus.PathComplete ) {
			if ( agent.isOnOffMeshLink ) {
				OffMeshLinkData data = agent.currentOffMeshLinkData;
				Vector3 endPos = data.endPos;
				lineRenderer.positionCount = 2;
				lineRenderer.SetPosition(0, GetComponent<Rigidbody>().position + Vector3.up * yOffset);
				lineRenderer.SetPosition(1, endPos + Vector3.up * agent.baseOffset + Vector3.up * yOffset);
			} else {
				lineRenderer.positionCount = agent.path.corners.Length;
				for( int i = 0; i < agent.path.corners.Length; i ++ ) {
					lineRenderer.SetPosition( i, agent.path.corners[i] + Vector3.up * yOffset );
				}
				//lineRenderer.SetPositions(agent.path.corners);
			}
			
		}
		
	}

	private void AssignAgentMaybe( ref NavMeshAgent agent, Func<NavMeshAgent> f ) {
		if ( agent == null ) {
			agent = f();
		}
	}
}
