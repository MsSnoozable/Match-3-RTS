using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class UnitData : ScriptableObject
{
    #region Public Fields
	public unitColors color;
	public unitType type;
	public unitRange range;
	public readonly static float attackFusionDelay = 0.01f;
	[Tooltip("starting strength of shields that are made from this unit")]
	public int baseShieldStrength;
	[Tooltip("starting strength of attacks that are made from this unit during hold")]
	public int baseAttackHoldStrength;
	[Tooltip("starting strength of attacks that are made from this unit at release")]
	public int baseAttackReleaseStrength;
	[Tooltip ("seconds from attack formation being made to the attack launching")]
	public float chargeTime;
	#endregion

	#region Private Fields
	#endregion
	//sfx
	//animations

	public static bool operator == (UnitData a, UnitData b) => a.color == b.color && a.type == b.type;

	public static bool operator != (UnitData a, UnitData b) => a.color != b.color || a.type != b.type;
	public override bool Equals(object other)
	{
		return base.Equals(other);
	}
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
