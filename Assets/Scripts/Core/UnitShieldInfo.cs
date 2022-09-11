using System.Collections.Generic;
using UnityEngine;

public class UnitShieldInfo : UnitFormationInfo
{
	public int col;
	public int topMost;
	public int bottomMost;
	public List<UnitController> shielders;


	public UnitShieldInfo(int col, int topMost, int bottomMost, List<UnitController> shielders, PlayerGrid pg) : base(pg)
	{
		this.col = col;
		this.topMost = topMost;
		this.bottomMost = bottomMost;
		this.shielders = shielders;
	}
}
