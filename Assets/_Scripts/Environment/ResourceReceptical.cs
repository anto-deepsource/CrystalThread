using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any structure or spot that can recieve resources from units including the player.
/// </summary>
public class ResourceReceptical : MonoBehaviour {
	
	public virtual bool CanProcessResourceable( Resourceable resource ) {
		return true;
	}

	public virtual void ProcessResourceable( Resourceable resource ) {
		Destroy(resource.gameObject);
	}
}

