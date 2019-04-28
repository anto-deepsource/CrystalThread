
using HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
    public class BasicAgent : MonoBehaviour {

        public int frameWait = 2;

        public Blackboard blackboard;

		public UnitEssence essence;

		public PrimitiveTask rootTask;

        //public StateMachine stateMachine; 

        #region Private Variables

        private int fw = 0;

        //private ITask task;

        private bool running = false;
        
        private QuickListener listener;

        #endregion
        
        void Awake() {
            fw = frameWait;
			

		}

        void Start() {
            if (blackboard==null) {
                blackboard = GetComponent<Blackboard>();
            }
            if (blackboard == null) {
                blackboard = GetComponentInParent<Blackboard>();
            }
            //if (blackboard!=null) {
            //    listener = new QuickListener(BlackboardEventCallback);
            //    blackboard.events.Add(listener);
            //}
			essence = gameObject.GetUnitEssence();
			essence.Events.Add(gameObject, EssenceEventCallback);

			//rootTask.transform.DisableChildren();
			rootTask.Stop();
        }

        void OnDestroy() {
            blackboard?.events.Remove(listener);
        }

        void Update() {
            if (running && rootTask != null) {
                //rootTask.UpdateTask(blackboard);
            }
            else
            // delay for a few frames at first, then start
            if (fw > 0) {
                fw--;
                if (fw == 0)
                    StartTask();
            }
        }

        private void StartTask() {
            running = true;
            rootTask.Begin();
        }
        

        //private void BlackboardEventCallback(int eventCode, object data) {

        //    switch ((BlackboardEventType)eventCode) {

        //        case BlackboardEventType.Damaged:

        //            // TODO:
        //            break;

        //        case BlackboardEventType.Death:
        //            StopAgent();
        //            break;
        //    }
        //}

		private void EssenceEventCallback(int eventCode, object data) {

			switch ((BlackboardEventType)eventCode) {

				case BlackboardEventType.Damaged:

					// TODO:
					break;

				case BlackboardEventType.Death:
					StopAgent();
					break;
			}
		}

		private void StopAgent() {
            running = false;
            //StopAllCoroutines();
			rootTask.Stop();
		}
    }
}