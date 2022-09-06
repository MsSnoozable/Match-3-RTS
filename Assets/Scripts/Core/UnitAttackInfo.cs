using System.Collections.Generic;
using UnityEngine;

public class UnitAttackInfo
{
	public int row;
	public int leftMost;
	public int rightMost;
	public PlayerGrid pg;
	public List<UnitController> attackers;

	public UnitAttackInfo(int row, int leftMost, int rightMost, List<UnitController> attackers, PlayerGrid pg)
	{
		this.row = row;
		this.leftMost = leftMost;
		this.rightMost = rightMost;
		this.attackers = attackers;
		this.pg = pg;
	}
}
