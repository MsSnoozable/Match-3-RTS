using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFormationInfo
{
	public PlayerGrid pg;
	public bool fromSwap;
    public UnitFormationInfo (PlayerGrid pg)
	{
		this.pg = pg;
	}
}
