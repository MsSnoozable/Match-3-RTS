using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class UnitData : ScriptableObject
{
    #region Public Fields
    #endregion
	public GameObject model;

	public unitColors color;

	public unitType type;

	#region Private Fields
	#endregion
	//color enumerator
	float baseDamage;
	float baseDefence;
	float chargeTime;
	Sprite sprite;
	//sfx
	//animations

	//functions
	public abstract void Attack();
	public abstract void Shield();
	//move to pos

	//todo: might need modifications for this based on how its implemented
	public static bool operator == (UnitData a, UnitData b) => a.color == b.color && a.type == b.type;

	public static bool operator != (UnitData a, UnitData b) => a.color != b.color || a.type != b.type;
}
