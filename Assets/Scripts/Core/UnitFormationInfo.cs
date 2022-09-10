using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitFormationInfo
{
	public PlayerGrid pg;

	public UnitFormationInfo(PlayerGrid pg)
	{
		this.pg = pg;
	}
}
