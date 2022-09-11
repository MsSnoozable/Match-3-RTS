using System.Collections.Generic;
using UnityEngine;

public class UnitAttackInfo : UnitFormationInfo
{
	public int row;
	public int leftMost;
	public int rightMost;
	public List<UnitController> attackers;

	public UnitAttackInfo(int row, int leftMost, int rightMost, List<UnitController> attackers, PlayerGrid pg) : base(pg)
	{
		this.row = row;
		this.leftMost = leftMost;
		this.rightMost = rightMost;
		this.attackers = attackers;
	}
}
