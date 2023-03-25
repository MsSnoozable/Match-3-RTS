using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class MoreButtonsCursor : CursorScript
{
    [SerializeField] private GameObject leftAddon;
    [SerializeField] private GameObject rightAddon;
    [SerializeField] private GameObject topAddon;
    [SerializeField] private GameObject bottomAddon;
    protected override void Move(Vector2 moveDirection)
    {
        base.Move(moveDirection);


        if (xPos == PlayerGrid.MinColumn) leftAddon.SetActive(false);
        else leftAddon.SetActive(true);

        if (yPos == 0) topAddon.SetActive(false);
        else topAddon.SetActive(true);

        if (xPos == PlayerGrid.GridWidth - 1) rightAddon.SetActive(false);
        else rightAddon.SetActive(true);

        if (yPos == PlayerGrid.GridHeight - 1) bottomAddon.SetActive(false);
        else bottomAddon.SetActive(true);
    }
    bool inUpDownBounds() => yPos > 0  && yPos < PlayerGrid.GridHeight - 1;
    bool inLeftRightBounds() => xPos > PlayerGrid.MinColumn && xPos < PlayerGrid.GridWidth - 1;
    bool inBounds() => inUpDownBounds() && inLeftRightBounds();

    public override void QuickAttack (InputAction.CallbackContext context)
	{
        if (context.started)
        {
            if (inLeftRightBounds())
            {
                List<UnitController> attackers = new List<UnitController>();
                for (int i = -1; i <= 1; i++)
                    attackers.Add(pg.GridArray[xPos + i, yPos]);

                //swap so middle is where the others move into
                UnitController temp = attackers[0];
                attackers[0] = attackers[1];
                attackers[1] = temp;

                UnitAttackInfo info = new UnitAttackInfo(yPos, attackers, pg, false);
                GameManager._.AttackCreated(info);
            }
            else
            {
                //swap fail sound
            }
        }
    }

    public override void RotateAbility (InputAction.CallbackContext context)
	{
        if (context.started)
        {
            if (inBounds())
			{
                Vector2Int left = new Vector2Int (xPos - 1 , yPos);
                Vector2Int right = new Vector2Int (xPos + 1 , yPos);
                Vector2Int up = new Vector2Int (xPos, yPos + 1);
                Vector2Int down = new Vector2Int (xPos, yPos - 1);

                pg.MoveInGridOnly(left, up);
                pg.MoveInGridOnly(left, right);
                pg.MoveInGridOnly(left, down);
			}
            else
			{
                GameManager._.SwapFailed();
                //swap failed
			}
        }
    }

    public override void BoltAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (inBounds())
			{
                List<UnitController> attackers = new List<UnitController>();
                for (int i = -1; i <= 1; i++)
                     attackers.Add(pg.GridArray[xPos + i, yPos]);
                attackers.Add(pg.GridArray[xPos, yPos + 1]); //up
                attackers.Add(pg.GridArray[xPos, yPos - 1]); //down

                //swap so middle is where the others move into
                UnitController temp = attackers[0];
                attackers[0] = attackers[1];
                attackers[1] = temp;

                UnitAttackInfo info = new UnitAttackInfo(yPos, attackers, pg, false);
                GameManager._.AttackCreated(info);
            }
            else
            {
                //swap fail sound
            }
        }
    }
    public override void QuickShield(InputAction.CallbackContext context) {
        if (context.started)
        {
            if (inUpDownBounds())
            {
                List<UnitController> shielders = new List<UnitController>();
                for (int i = -1; i <= 1; i++)
                {
                    shielders.Add(pg.GridArray[xPos, yPos + i]);
                }
                UnitShieldInfo info = new UnitShieldInfo(shielders, pg, false);
                GameManager._.ShieldCreated(info);
            }
            else
			{
                //swap fail sound
			}
        }
    }
    public override void Swapping(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 swapDirection = context.ReadValue<Vector2>();

            Vector2Int intSwapDirection = new Vector2Int((int)swapDirection.x, (int)swapDirection.y);
            //todo: make a cursor info class
            GameManager._.MoreButtonsCursorSwap(this.tag, xPos, yPos, swapDirection);
        }
    }
}
