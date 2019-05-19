using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Porter {
	/// <summary>
	/// Attaches to some game object with a collider and deals damage to
	/// any units (or otherwise damagable objects ) that collide with this one.
	/// </summary>
	public class DamageOnCollision : MonoBehaviour {

		public bool on = true;

		

		private void OnTriggerEnter(Collider other) {
			if ( !on ) {
				return;
			}

			Damagable damagable = other.GetComponent<Damagable>();
			if ( damagable!=null ) {


				DamageSource damage = new DamageSource() {
					amount = 3,
					
				};
				damagable.ApplyDamage(damage);
			}
			
		}
	}
}