using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraQuickPositioner : MonoBehaviour {

	public Transform target;

	public PositionType positionType;

	[Header("Polar Postion From Target")]
	public float r = 1;
	public float theta = 0;
	public float yOffset = 0;

	[Header("Top Down")]
	public float verticalDistance = 10;

	public void RepositionCamera() {
		switch (positionType) {
			case PositionType.MaintainCurrent:

				break;
			case PositionType.PolarPostionFromTarget:
				var vector = CommonUtils.AngleToVector(theta).FromXZ();
				transform.position = target.position + vector * r + Vector3.up * yOffset;
				break;
			case PositionType.TopDown:
				transform.position = target.position + Vector3.up * verticalDistance;
				break;
		}

		transform.LookAt(target);
	}
}
public enum PositionType {
	MaintainCurrent,
	PolarPostionFromTarget,
	TopDown,
}