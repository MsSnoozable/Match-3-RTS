using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class UnitData : ScriptableObject
{
    #region Public Fields
	public GameObject model;
	public unitColors color;
	public unitType type;
	public unitRange range;
	public readonly static float moveDuration = 0.5f;
	public readonly static float attackFusionDelay = 0.01f;

	#endregion

	#region Private Fields
	#endregion
	float baseDamage;
	float baseDefence;
	[Tooltip ("seconds from attack formation being made to the attack launching")]
	public float chargeTime;
	Sprite sprite;
	//sfx
	//animations

	public static bool operator == (UnitData a, UnitData b) => a.color == b.color && a.type == b.type;

	public static bool operator != (UnitData a, UnitData b) => a.color != b.color || a.type != b.type;
}
