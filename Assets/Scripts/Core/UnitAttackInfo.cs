using System.Collections.Generic;
using UnityEngine;

public class UnitAttackInfo : UnitFormationInfo
{
	public int row;
	public int leftMost;
	public int rightMost;
	public List<UnitController> attackers;

	public int existingAttackCol;

	public UnitAttackInfo(int row, List<UnitController> attackers, PlayerGrid pg) : base(pg)
	{
		int rightMost = PlayerGrid.MinColumn;
		int leftMost = PlayerGrid.GridWidth - 1;

		foreach (UnitController uc in attackers)
		{
			if (uc.xPos < leftMost) leftMost = uc.xPos;
			if (uc.xPos > rightMost) rightMost = uc.xPos;
		}

		this.row = row;
		this.leftMost = leftMost;
		this.rightMost = rightMost;
		this.attackers = attackers;
		fromSwap = true;
	}
	public UnitAttackInfo(int row, List<UnitController> attackers, PlayerGrid pg, bool fromSwap) : base(pg)
	{
		int rightMost = PlayerGrid.MinColumn;
		int leftMost = PlayerGrid.GridWidth - 1;

		foreach (UnitController uc in attackers)
		{
			if (uc.xPos < leftMost) leftMost = uc.xPos;
			if (uc.xPos > rightMost) rightMost = uc.xPos;
		}

		this.row = row;
		this.leftMost = leftMost;
		this.rightMost = rightMost;
		this.attackers = attackers;
		this.fromSwap = fromSwap;
	}
}
