using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitFacing {
	public static int Up = 0;
	public static int Right = 1;
	public static int Left = 2;
	public static int Down = 3;
}

public enum SpriteType {
	FourFrames,
	ThreeFrames
}

/// <summary>
/// The order of the directions per row in this sprite sheet from the top.
/// </summary>
public enum FacingOrder {
	DownLeftRightUp,
	UpRightDownLeft,

}

//[ExecuteInEditMode]
public class UnitAnimator : AnimationHandler {

	public UnitMaterialPack materialPack;

	public Sprite spriteSheet;

	public bool scaleToCollider = true;

	public SpriteType spriteType = SpriteType.FourFrames;

	public FacingOrder facingOrder = FacingOrder.DownLeftRightUp;

	public Sprite[][] sprites;

	public bool billboard = true;

	private SpriteRenderer renderer;

	public float tilt = 0;

	public bool IsInAir { get; set; }
	public bool IsRunning { get; set;  }

	// Use this for initialization
	void Start () {
		Setup();
	}

	public void Setup() {
		renderer = GetComponent<SpriteRenderer>();
		if (renderer == null)
			renderer = gameObject.AddComponent<SpriteRenderer>();
		if (materialPack != null) {
			renderer.material = materialPack.defaultMaterial;
		}
		GenerateSpriteSheet();

		Update();
	}

	float t;
	int i = 0;

	// Update is called once per frame
	void Update () {
		t += Time.deltaTime;

		Camera mainCamera = Camera.main;

		float theta = CommonUtils.ThetaBetweenD(
			CommonUtils.HorPosition(transform),
			CommonUtils.HorPosition(mainCamera.transform));
		float difference = theta + transform.parent.rotation.eulerAngles.y;
		while (difference > 180)
			difference -= 360;
		while (difference < -180)
			difference += 360;

		int facing = UnitFacing.Up;
		if ( difference >= -135 && difference < - 45 ) {
			facing = UnitFacing.Up;
		} else if (difference >= -45 && difference < 45) {
			facing = UnitFacing.Left;
		} else if (difference < -135 || difference > 135) {
			facing = UnitFacing.Right;
		} else if (difference >= 45 && difference < 135) {
			facing = UnitFacing.Down;
		}
		if ( t > 0.1f ) {
			t = 0f;

			int frame = IndexToFrame(i);
			renderer.sprite = sprites[facing][frame];
			
			i++;
			if (i >= 4) {
				i = 0;

			}
		}

		Vector3 billboardTarget = Camera.main.transform.position;

		billboardTarget.y = transform.position.y;
		transform.LookAt(billboardTarget, Vector3.up );

		transform.RotateAround(transform.position, transform.position - billboardTarget, tilt);
	}

	private int IndexToFrame(int i ) {
		if ( spriteType == SpriteType.FourFrames ) {
			if (IsInAir) {
				return 1;
			} else if (IsRunning) {
				return i;
			} else {
				return 0;
			}
		} else {
			if (IsInAir) {
				return 0;
			} else if (IsRunning) {
				if (i == 3) {
					return 1;
				} else {
					return i;
				}
			} else {
				return 1;
			}
		}
	}

	private void GenerateSpriteSheet() {
		int rows = 4;
		int columns = 4;
		sprites = new Sprite[rows][];
		
		if ( spriteType == SpriteType.ThreeFrames ) {
			columns = 3;
		}

		int cellWidth = (int)(spriteSheet.rect.width / columns);
		int cellHeight = (int)(spriteSheet.rect.height / rows );

		Rect rect = new Rect(spriteSheet.rect.x, spriteSheet.rect.y, cellWidth, cellHeight);
		//Vector2 pivot = new Vector2(cellWidth * 0.5f, cellHeight * 0.5f);
		Vector2 pivot = new Vector2(  0.5f,   0.0f);
		for ( int r = 0; r < rows; r ++ ) { // bottom to top
			int facing = r;
			switch (facingOrder) {
				case FacingOrder.DownLeftRightUp:
					facing = r;
					break;
				case FacingOrder.UpRightDownLeft:
					switch( r ) {
						case 3: facing = UnitFacing.Up; break;
						case 2: facing = UnitFacing.Right; break;
						case 1: facing = UnitFacing.Down; break;
						case 0: facing = UnitFacing.Left; break;
					}
					break;
			}
			sprites[facing] = new Sprite[columns];
			for ( int c = 0; c < columns; c ++ ) {
				rect = new Rect(spriteSheet.rect.x+ cellWidth * c, spriteSheet.rect.y + cellHeight * r,
					cellWidth, cellHeight);
				Sprite newSprite = Sprite.Create(spriteSheet.texture, rect, pivot,
					100.0f, 0, SpriteMeshType.Tight, Vector4.zero);
				sprites[facing][c] = newSprite;

				//rect.x += spriteSheet.width / columns;
			}
			
		}

		if (scaleToCollider) {
			CapsuleCollider collider = GetComponentInParent<CapsuleCollider>();
			float f =  collider.height / (sprites[0][0].textureRect.height / sprites[0][0].pixelsPerUnit);
			transform.localScale = new Vector3(f, f, 1);
		}
	}

	override public void Play(AnimationKeys.Key key, float playLength, params AnimationKeys.Mod[] mods) {
		if (materialPack != null) {
			switch( key ) {
				case AnimationKeys.Key.Lifted:
					StartCoroutine(ColorAnimation(materialPack.staggeredAnimation));
					
					break;
				case AnimationKeys.Key.LiftedEnd:
					StartCoroutine(ColorAnimation(materialPack.resetAnimation));

					break;
				case AnimationKeys.Key.Damaged:
					StartCoroutine(ColorAnimation(materialPack.damagedAnimation));

					break;
				case AnimationKeys.Key.Staggered:
					StartCoroutine(ColorAnimation(materialPack.staggeredAnimation));

					break;
				case AnimationKeys.Key.StaggeredEnd:
					StartCoroutine(ColorAnimation(materialPack.resetAnimation));

					break;

				case AnimationKeys.Key.Death:
					StartCoroutine(ColorAnimation(materialPack.deathAnimation, AnimationKeys.Event.DeathEnd ));

					break;
			}
			
		}
	}
	
	private IEnumerator ColorAnimation(List<ColorKeyframe> animation, 
			AnimationKeys.Event animEvent = AnimationKeys.Event.None) {
		float timer = 0;
		Color lastColor = renderer.material.color;
		float lastTilt = tilt;
		foreach( var keyframe in animation ) {
			while( timer < keyframe.time ) {
				timer += Time.deltaTime;
				renderer.material.color = Color.Lerp(lastColor,keyframe.color,timer/keyframe.time);
				tilt = Mathf.Lerp(lastTilt, keyframe.tilt, timer / keyframe.time);
				yield return null;
			}
			timer -= keyframe.time;
			lastColor = keyframe.color;
			lastTilt = keyframe.tilt;
		}
		if (animEvent != AnimationKeys.Event.None) {
			TriggerEvent(animEvent);
		}
	}
}
