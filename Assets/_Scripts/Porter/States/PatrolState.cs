
using HexMap;
using HexMap.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
	public class PatrolState : ParallelState {

		public Vector3 center;
        public float radius;

        public Awareness awareness;

        public IState engageState;

        //public static readonly string Key = "Patrol";

        //public override string UniqueKey() { return Key; }

		private MoveToPointTask moveTask;

		/// <summary>
		/// Patrols the circle in the world that is centered at the given center and has the given radius
		/// </summary>
		public PatrolState(StateMachine machine, Vector3 center,
				float radius, Awareness awarenessShape) :
				base(machine, true ) {

			this.center = center;
			this.radius = radius;
			this.awareness = awarenessShape;
            SetupSequence();
        }

        private void Start() {
            Initialize(true);
            SetupSequence();
        }

        private void SetupSequence() {
            if (awareness == null) {
                awareness = GetComponentInChildren<Awareness>();
            }

            center = transform.position;

            // typically a state is a sequence, but here we want to essentially do two things concurrently -> use a parrallel
            // -first thing we're doing is picking a patrol point, moving there, pausing for a second, then repeating
            // -the second thing to do is look for enemies and, upon finding one, changing from the patrol state into the alert state

            Sequence patrolSequence = new Sequence();
            patrolSequence.Add(new Gig(PickNextPatrolPoint));
			moveTask = new MoveToPointTask(1);
			patrolSequence.Add(moveTask);
            patrolSequence.Add(new WaitFor(1f));
            Add(patrolSequence);

            Sequence awarenessSequence = new Sequence();
            awarenessSequence.Add(new Gig(WatchForEnemies));
            Add(awarenessSequence);
        }

        bool PickNextPatrolPoint() {
			Vector3 newPatrolPoint = HexNavMeshManager.GetAnyPointInArea(center, radius);
			moveTask.target = newPatrolPoint;
			return true;
		}

		bool WatchForEnemies() {
			if ( awareness.EnemiesNearby) {
				// alert the state machine to engage
				var blackboard = GetComponentInParent<Blackboard>();
				blackboard.Play(AnimationKey.Alerted, AnimationData.none);
				stateMachine.ChangeState( engageState );
			}
			
			return true; // the sequence keeps looping so return finished
		}

		/// <summary>
		/// Called whenever the statemachine changes from this state.
		/// Make sure that the NavMeshAgent is disabled.
		/// </summary>
		public override void Exit(Blackboard blackboard) {
			base.Exit(blackboard);
			// since this state use navigation/move to task, we need to try and stop the nav agent
			Utilities.StopHexNavAgentMaybe(blackboard);
		}
	}
}