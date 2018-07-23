using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringFactory {

	public static SpringLabels SpringLabels(GameObject gameObject) {
		SpringLabels labels = gameObject.GetComponent<SpringLabels>();
		if (labels == null) {
			labels = gameObject.AddComponent<SpringLabels>();
		}
		return labels;
	}

	public static SpringJoint SnapSpring( Rigidbody freeBody, Rigidbody fixedBody, Vector3 anchorPoint ) {
		SpringLabels labels = SpringLabels(freeBody.gameObject);
		RemoveSnapSpring(freeBody);

		SpringJoint newSpring = freeBody.gameObject.AddComponent<SpringJoint>();
		newSpring.autoConfigureConnectedAnchor = false;
		newSpring.connectedBody = fixedBody;
		if (fixedBody == null) {
			Debug.Log("Fixed body was null");
			newSpring.connectedAnchor = anchorPoint;
		} else {
			newSpring.connectedAnchor = fixedBody.transform.InverseTransformPoint(anchorPoint);
		}
		
		//newSpring.autoConfigureDistance = false;
		newSpring.anchor = Vector2.zero;
		
		newSpring.spring = 100;
		newSpring.damper = 20.1f;

		
		labels.SetSpring("SnapSpring", newSpring);

		return newSpring;
	}

	public static SpringJoint LiftSpring(Rigidbody freeBody, Rigidbody fixedBody, Vector3 anchorPoint) {
		string labelName = "Lift";

		SpringLabels labels = SpringLabels(freeBody.gameObject);
		RemoveSpringWithLabel(labelName, freeBody);

		SpringJoint newSpring = freeBody.gameObject.AddComponent<SpringJoint>();
		newSpring.autoConfigureConnectedAnchor = false;
		newSpring.connectedBody = fixedBody;
		if (fixedBody == null) {
			Debug.Log("Fixed body was null");
			newSpring.connectedAnchor = anchorPoint;
		} else {
			newSpring.connectedAnchor = fixedBody.transform.InverseTransformPoint(anchorPoint);
		}

		//newSpring.autoConfigureDistance = false;
		newSpring.anchor = Vector2.zero;

		newSpring.spring = 100;
		newSpring.damper = 20.1f;


		labels.SetSpring(labelName, newSpring);

		return newSpring;
	}

	//public static ConfigurableJoint RopeJoint( Rigidbody linkBody, Rigidbody parentBody, float length ) {
	//	ConfigurableJoint joint = linkBody.gameObject.AddComponent<ConfigurableJoint>();
	//	joint.autoConfigureConnectedAnchor = false;
	//	joint.anchor = new Vector3(0, -length, 0);
	//	joint.connectedAnchor = new Vector3(0, 0,0);
	//	//joint.connectedAnchor = new Vector3(0, length,0);
	//	joint.connectedBody = parentBody;
	//	joint.xMotion = ConfigurableJointMotion.Limited;
	//	joint.yMotion = ConfigurableJointMotion.Limited;
	//	joint.zMotion = ConfigurableJointMotion.Limited;

	//	SoftJointLimit sjl = new SoftJointLimit();
	//	sjl.limit = 0;
	//	sjl.bounciness = 0;
	//	sjl.contactDistance = 0.1f;
	//	joint.linearLimit = sjl;

	//	SoftJointLimitSpring sjls = new SoftJointLimitSpring();
	//	sjls.spring = 1;
	//	sjls.damper = 0;
	//	joint.linearLimitSpring = sjls;

	//	//JointDrive jd = new JointDrive();
	//	//jd.positionSpring = 100;
	//	//joint.xDrive = jd;
	//	//joint.yDrive = jd;
	//	//joint.zDrive = jd;

	//	joint.angularXMotion = ConfigurableJointMotion.Limited;
	//	joint.angularYMotion = ConfigurableJointMotion.Limited;
	//	joint.angularZMotion = ConfigurableJointMotion.Limited;



	//	joint.projectionMode = JointProjectionMode.PositionAndRotation;

	//	return joint;
	//}

	public static ConfigurableJoint RopeJoint(Rigidbody linkBody, Rigidbody parentBody, float length) {
		ConfigurableJoint joint = linkBody.gameObject.AddComponent<ConfigurableJoint>();
		joint.autoConfigureConnectedAnchor = false;
		joint.anchor = new Vector3(0, -length, 0);
		joint.connectedAnchor = new Vector3(0, 0, 0);
		joint.connectedBody = parentBody;
		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;

		joint.angularXMotion = ConfigurableJointMotion.Free;
		joint.angularYMotion = ConfigurableJointMotion.Free;
		joint.angularZMotion = ConfigurableJointMotion.Free;

		joint.projectionMode = JointProjectionMode.PositionAndRotation;

		return joint;
	}

	public static void RemoveSpringWithLabel(string label, Rigidbody body) {
		SpringLabels labels = SpringLabels(body.gameObject);
		Joint spring;
		if (labels.GetSpring(label, out spring)) {
			RemoveSpring(spring);
			labels.RemoveSpring(label);
		}
	}

	public static void RemoveSnapSpring(Rigidbody body) {
		SpringLabels labels = SpringLabels(body.gameObject);
		Joint spring;
		if (labels.GetSpring("SnapSpring", out spring)) {
			RemoveSpring(spring);
			labels.RemoveSpring("SnapSpring");
		}
	}

	public static void RemoveSpring(Joint spring) {
		GameObject.Destroy(spring);
	}
}
