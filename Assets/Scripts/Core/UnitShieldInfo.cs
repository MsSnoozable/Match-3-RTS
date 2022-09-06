using System.Collections.Generic;
using UnityEngine;

public class UnitShieldInfo
{
	public int col;
	public int topMost;
	public int bottomMost;
	public PlayerGrid pg;
	public List<UnitController> shielders;


	public UnitShieldInfo(int col, int topMost, int bottomMost, List<UnitController> shielders, PlayerGrid pg)
	{
		this.col = col;
		this.topMost = topMost;
		this.bottomMost = bottomMost;
		this.shielders = shielders;
		this.pg = pg;
	}
}
