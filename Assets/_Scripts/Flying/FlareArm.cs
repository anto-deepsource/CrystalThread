using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareArm : MonoBehaviour {

	public Texture2D craterTexture;

	public Texture2D decalTexture;

	private MeshRenderer meshRenderer;

	private Material flareArmMaterial;
	

	public const string DECAL_TEXTURE = "_DecalTexture";

	private void OnEnable() {
		meshRenderer = GetComponent<MeshRenderer>();
		flareArmMaterial = meshRenderer.material;
		decalTexture = flareArmMaterial.GetTexture(DECAL_TEXTURE) as Texture2D;
		decalTexture = Instantiate(decalTexture);
		flareArmMaterial.SetTexture(DECAL_TEXTURE, decalTexture);

	}

	public void OnCollisionEnter(Collision collision) {
		if (collision.relativeVelocity.sqrMagnitude < 100 ) {
			return;
		}
		RaycastHit hit = new RaycastHit();
		Vector2 averageCenter = Vector2.zero;
		foreach( var contactPoint in collision.contacts ) {
			Ray ray = new Ray(contactPoint.point - contactPoint.normal, contactPoint.normal*10);
			if (contactPoint.thisCollider.Raycast(ray, out hit, 10)) {
				averageCenter += hit.textureCoord;
				//Debug.LogFormat("Tex coords: {0}", hit.textureCoord);
			}
		}
		averageCenter /= collision.contacts.Length;

		var sourceBytes = craterTexture.GetPixels();

		int destCenterX = Mathf.FloorToInt(averageCenter.x * decalTexture.width);
		int destCenterY = Mathf.FloorToInt(averageCenter.y * decalTexture.height);
		
		int left = -craterTexture.width / 2 + destCenterX;
		
		int bottom = -craterTexture.height / 2 + destCenterY;
		
		

		for ( int x = 0; x < craterTexture.width; x ++ ) {
			for (int y = 0; y < craterTexture.height; y++) {
				var sourcePixel = sourceBytes[x * craterTexture.width + y];
				var currentPixel = decalTexture.GetPixel(left + x, bottom + y);
				if (sourcePixel.a >= currentPixel.a) {
					decalTexture.SetPixel(left + x, bottom + y, sourcePixel);
				}

			}
		}

		
		decalTexture.Apply();
	}

	
}
