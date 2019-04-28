
using UnityEngine;

namespace Porter {
    public abstract class PrimitiveTask : MonoBehaviour {
        public bool Running { get; protected set; }
        public bool Successful { get; protected set; }

        public virtual void Begin() {
            gameObject.SetActive(true);
			Running = true;
		}

        public virtual void Stop() {
            gameObject.SetActive(false);
			Running = false;
		}
    }
    
}
