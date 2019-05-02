using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringJointVisualizer : MonoBehaviour {

	public LineRenderer lineRenderer;

	private int flickeringIndex = 0;

	void Start () {
		if (lineRenderer == null) {
			lineRenderer = GetComponent<LineRenderer>();
		}
	}
	
	void Update () {
		lineRenderer.positionCount = 0;
		int i = 0;
		var springJoints = GetComponents<SpringJoint>();

		if (springJoints.Length == 0) {
			return;
		}

		var justOneJoint = springJoints[flickeringIndex];
		{
			lineRenderer.positionCount += 2;
			var anchor = justOneJoint.transform.TransformPoint(justOneJoint.anchor);
			lineRenderer.SetPosition(i++, anchor);
			var connectedAnchor = justOneJoint.connectedBody.transform.TransformPoint(justOneJoint.connectedAnchor);
			lineRenderer.SetPosition(i++, connectedAnchor);
		}

		flickeringIndex += 1;
		if (flickeringIndex >= springJoints.Length) {
			flickeringIndex = 0;
		}

		//foreach( var sprintJoint in springJoints ) {
		//	lineRenderer.positionCount += 2;
		//	var anchor = sprintJoint.transform.TransformPoint(sprintJoint.anchor);
		//	lineRenderer.SetPosition(i++, anchor);
		//	var connectedAnchor = sprintJoint.connectedBody.transform.TransformPoint(sprintJoint.connectedAnchor);
		//	lineRenderer.SetPosition(i++, connectedAnchor);
		//}
	}
}
