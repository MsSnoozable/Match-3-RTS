using System.Collections.Generic;

public class UnitAttackInfo : UnitFormationInfo
{
	public int row;
	public int leftMost;
	public int rightMostPosition;
	public int rightMostIndex;
	public List<UnitController> attackers;

	public int existingAttackCol;

	public UnitAttackInfo(int row, List<UnitController> attackers, PlayerGrid pg) : base(pg)
	{
		GenericConstructor(row, attackers, pg);
		fromSwap = true;
	}
	public UnitAttackInfo(int row, List<UnitController> attackers, PlayerGrid pg, bool fromSwap) : base(pg)
	{
		GenericConstructor(row, attackers, pg);
		this.fromSwap = fromSwap;
	}

	void GenericConstructor (int row, List<UnitController> attackers, PlayerGrid pg)
	{
		int rightMost = PlayerGrid.MinColumn;
		int leftMost = PlayerGrid.GridWidth - 1;

		int rightMostIndex = 0;

		for (int i = 0; i < attackers.Count; i++)
		{
			if (attackers[i].xPos < leftMost)
				leftMost = attackers[i].xPos;
			if (attackers[i].xPos > rightMost)
			{
				rightMost = attackers[i].xPos;
				rightMostIndex = i;
			}
		}

		this.row = row;
		this.leftMost = leftMost;
		this.rightMostPosition = rightMost;
		this.rightMostIndex = rightMostIndex;
		this.attackers = attackers;
	}
}
