using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitMaterialPack", menuName = "Unit Animations/Unit Material Pack", order = 1)]
public class UnitMaterialPack : ScriptableObject {
	public Material defaultMaterial;

	public List<ColorKeyframe> damagedAnimation = new List<ColorKeyframe>();
	public List<ColorKeyframe> staggeredAnimation = new List<ColorKeyframe>();
	public List<ColorKeyframe> resetAnimation = new List<ColorKeyframe>();
	public List<ColorKeyframe> deathAnimation = new List<ColorKeyframe>();
	//public List<ColorKeyframe> liftedAnimation = new List<ColorKeyframe>();
}
