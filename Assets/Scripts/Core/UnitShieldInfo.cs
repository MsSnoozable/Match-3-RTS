using System.Collections.Generic;
using UnityEngine;

public class UnitShieldInfo : UnitFormationInfo
{
	public int col;
	public List<int> rows;
	public List<UnitController> shielders;


	public UnitShieldInfo(int col, List<int> rows, List<UnitController> shielders, PlayerGrid pg) : base(pg)
	{
		this.col = col;
		this.rows = rows;
		this.shielders = shielders;
	}
}
