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

	public override void Swapping(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 swapDirection = context.ReadValue<Vector2>();
            Vector2Int intSwapDirection = new Vector2Int((int)swapDirection.x, (int)swapDirection.y);
            GameManager.i.MoreButtonsCursorSwap(this.tag, xPos, yPos, intSwapDirection);
        }
    }
}
