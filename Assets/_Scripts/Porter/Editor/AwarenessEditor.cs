
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Porter {
	[CustomEditor(typeof(Awareness))]
	public class AwarenessEditor : Editor {

		private void OnSceneGUI() {
			var awareness = (Awareness)target;
			Handles.color = new Color(0, 0, 1, .4f);
			Handles.DrawWireDisc(awareness.transform.position, Vector3.up, awareness.immediateDistance);

			Handles.color = new Color(1, .1f, .1f, .4f);
			Handles.DrawWireDisc(awareness.transform.position, Vector3.up, awareness.furthestDistance);

			foreach( var pair in awareness.units) {
				var status = pair.Value;
				var unit = pair.Key;

				switch (status.status) {
					case AwarenessStatus.UnawareOf:
						Handles.color = new Color(.1f, .1f, .1f, .4f);
						break;
					case AwarenessStatus.AlertedTo:
						Handles.color = new Color(.1f, 1f, 1f, .4f);
						break;
					case AwarenessStatus.Engaging:
						Handles.color = new Color(1f, .2f, .2f, .4f);
						break;
				}
				var position = awareness.GetLastKnownPosition(unit);
				Handles.DrawSolidDisc(position, Vector3.up, 1f);

			}
		}
	}
}