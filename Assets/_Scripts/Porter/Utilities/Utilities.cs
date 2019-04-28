using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HexMap.Pathfinding;
using HexMap;
using Poly2Tri;
using UnitAnimation;

public static class Utilities {

	// Gets and returns the stated component or adds an appropriate one

	public static NavMeshAgent NavAgent(GameObject targetObject) {
		NavMeshAgent agent = targetObject.GetComponent<NavMeshAgent>();

		if (agent == null) {
			agent = targetObject.AddComponent<NavMeshAgent>();
			//agent.radius = radius * 0.9f; // just slightly smaller than our capsule collider
			//agent.height = height * 0.9f;
			//agent.speed = speed;
			agent.angularSpeed = 270f;
			//agent.acceleration = speed * 1.99f;
			//agent.stoppingDistance = radius * 5.5f;
			// Don't update position automatically (for animation purposes)
			agent.updatePosition = false;
			agent.updateRotation = true;
			agent.autoTraverseOffMeshLink = false;

			//agent.destination = goal.position;
		}
		return agent;
	}
	
	public static HexNavAgent HexNavAgent(GameObject targetObject) {
		HexNavAgent agent = targetObject.GetComponent<HexNavAgent>();

		if (agent == null) {
			agent = targetObject.AddComponent<HexNavAgent>();
		}
		return agent;
	}

	/// <summary>
	/// Convenience method that checks the blackboard for a hexnavagent and stops if it there is one.
	/// Stopping the hexnavagent is not necessary but ensures that processing is not wasted on it.
	/// </summary>
	/// <param name="blackboard"></param>
	public static void StopHexNavAgentMaybe( Blackboard blackboard ) {
		HexNavAgent agent = blackboard.gameObject.GetComponent<HexNavAgent>();

		if (agent != null) {
			agent.Stop();

		}
	}

	public static Rigidbody Rigidbody(GameObject targetObject) {
		Rigidbody myBody = targetObject.GetComponent<Rigidbody>();

		if (myBody == null) {
			myBody = targetObject.AddComponent<Rigidbody>();
			myBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			myBody.angularDrag = .9f;
		}
		return myBody;
	}

	public static AnimationMaster AnimationMaster(GameObject targetObject) {
		AnimationMaster aniMaster = targetObject.GetComponent<AnimationMaster>();

		if (aniMaster == null) {
			aniMaster = targetObject.AddComponent<AnimationMaster>();
		}
		return aniMaster;
	}

	public static ResourcesComponent Resources(GameObject targetObject) {
		ResourcesComponent resources = targetObject.GetComponent<ResourcesComponent>();

		//if (aniMaster == null) {
		//	aniMaster = targetObject.AddComponent<AnimationMaster>();
		//}
		return resources;
	}

	public static PolyShape PolyShape( GameObject targetObject, int slices = 8, bool forceNew = false ) {
		PolyShape shape = targetObject.GetComponent<PolyShape>();
		if ( shape == null || forceNew) {

			if ( shape == null ) {
				shape = targetObject.AddComponent<PolyShape>();
			}
			
			shape.Clear();
			
			BoxCollider boxCollider = targetObject.GetComponent<BoxCollider>();
			if (boxCollider != null) {
				Vector3 extents = boxCollider.size * 0.5f;
				shape.CreateAABB(boxCollider.center, extents.x, extents.z);

			} else {
				CapsuleCollider capsuleCollider = targetObject.GetComponent<CapsuleCollider>();
				if (capsuleCollider != null) {
					shape.CreateCircle(capsuleCollider.center, capsuleCollider.radius, slices);
				} else {
					SphereCollider sphereCollider = targetObject.GetComponent<SphereCollider>();
					if (sphereCollider != null) {
						shape.CreateCircle(sphereCollider.center, sphereCollider.radius, slices);
					} else {
						// try to made it from a radius indicator
						RadiusIndicator radiusIndicator = targetObject.GetComponent<RadiusIndicator>();
						if( radiusIndicator!=null ) {
							shape.CreateSemiCircle(Vector3.zero, radiusIndicator.radius,
								radiusIndicator.value, radiusIndicator.startAngle, radiusIndicator.segments);
						}
					}
				}
			}
		}
		return shape;
	}

	public static void GetComponentMaybe<T>(this GameObject gameObject,  ref T reference ) {
		if ( reference == null ) {
			reference = gameObject.GetComponent<T>();
		}
	}
	
	public static List<DelaunayTriangle> TriangulatePoints( IEnumerable<Vector2> points ) {
		List<PolygonPoint> outsidePoints = new List<PolygonPoint>();
		foreach (var point in points) {
			PolygonPoint pp = new PolygonPoint(point.x, point.y);
			outsidePoints.Add(pp);
		}

		Poly2Tri.Polygon poly = new Poly2Tri.Polygon(outsidePoints);

		// Triangulate it!  Note that this may throw an exception if the data is bogus.
		try {
			DTSweepContext tcx = new DTSweepContext();
			tcx.PrepareTriangulation(poly);
			DTSweep.Triangulate(tcx);
			tcx = null;
		} catch (System.Exception e) {
			//UnityEngine.Profiling.Profiler.Exit(profileID);
			throw e;
		}

		return new List<DelaunayTriangle>(poly.Triangles);
	}
}
