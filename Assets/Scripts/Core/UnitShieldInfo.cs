using System.Collections.Generic;
using UnityEngine;

public class UnitShieldInfo : UnitFormationInfo
{
	public int col;
	public List<int> rows;
	public List<UnitController> shielders;


	public UnitShieldInfo(List<UnitController> shielders, PlayerGrid pg) : base(pg)
	{
		List<int> rows = new List<int>();

		foreach (UnitController uc in shielders)
		{
			rows.Add(uc.yPos);
		}

		this.col = shielders[0].xPos;
		this.rows = rows;
		this.shielders = shielders;
		this.fromSwap = true;
	}
	public UnitShieldInfo(List<UnitController> shielders, PlayerGrid pg, bool fromSwap) : base(pg)
	{
		List<int> rows = new List<int>();

		foreach (UnitController uc in shielders)
		{
			rows.Add(uc.yPos);
		}

		this.col = shielders[0].xPos;
		this.rows = rows;
		this.shielders = shielders;
		this.fromSwap = fromSwap;
	}
}
