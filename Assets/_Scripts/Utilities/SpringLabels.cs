using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringLabels : MonoBehaviour {

	public Dictionary<string, Joint> springs = new Dictionary<string, Joint>();

	public bool HasSpring(string label) {
		return springs.ContainsKey(label);
	}

	public bool GetSpring(string label, out Joint outSpring ) {
		return springs.TryGetValue(label, out outSpring);
	}

	public void SetSpring(string label, Joint spring) {
		springs[label] = spring;
	}

	public void RemoveSpring(string label) {
		springs.Remove(label);
	}
}
