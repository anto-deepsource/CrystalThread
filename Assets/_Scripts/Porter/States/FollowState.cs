
using System.Collections;
using System.Collections.Generic;
using UnitAnimation;
using UnityEngine;

namespace Porter {
    public class FollowState : ParallelState {

        public GameObject target;
        public float distance = 1;

        public Awareness awareness;

        public IState onSpottedEnemy;

        private bool sequenced = false;

        MoveToTargetTask moveToTargetTask;

		private Blackboard blackboard;

		Sequence follow;
		//public static readonly string Key = "Follow";

		//      public override string UniqueKey() { return Key; }

		public FollowState(StateMachine machine) :
                base(machine) {
            SetupSequence();
        }

        public override void OnEnable() {
            base.OnEnable();
            if ( !sequenced ) {
                SetupSequence();
            }
			follow.RestartSequence();

		}

        //private void Start() {
        //    Initialize(true);
        //    SetupSequence();
        //}

        private void SetupSequence() {
            if (awareness==null) {
                awareness = GetComponentInChildren<Awareness>();
            }
			if (blackboard == null) {
				blackboard = GetComponentInParent<Blackboard>();
			}
			follow = new Sequence();
            follow.Add(new Gig(UpdateTargetAndDistance));
            moveToTargetTask = new MoveToTargetTask(blackboard,distance);
            follow.Add(moveToTargetTask); // uses hexnavagent. ensure that hexnavagent is stopped when we're done
            Add(follow);

            //Sequence awarenessSequence = new Sequence();
            //awarenessSequence.Add(new Gig(WatchForEnemies));
            //Add(awarenessSequence);

            sequenced = true;
        }

        bool UpdateTargetAndDistance() {
			moveToTargetTask.TargetTransform = target.transform;
			blackboard.target = target;
			moveToTargetTask.stopDistance = distance;
            return true;
        }

        bool WatchForEnemies() {
            if (awareness.EnemiesNearby) {
                // alert the state machine to engage
                blackboard.Play(AnimationKey.Alerted, AnimationData.none);

				
				if ( onSpottedEnemy!=null ) {
					stateMachine.ChangeState(onSpottedEnemy);
				}
            }

            return true; // the sequence keeps looping so return finished
        }

        /// <summary>
        /// Called whenever the statemachine changes from this state.
        /// Make sure that the NavMeshAgent is disabled.
        /// </summary>
        public override void Exit(Blackboard blackboard) {
            base.Exit(blackboard);
            Utilities.StopHexNavAgentMaybe(blackboard);
        }
    }
}