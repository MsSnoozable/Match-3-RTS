using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class MoreButtonsCursor : CursorScript
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	protected override void Move(Vector2 moveDirection)
	{
		base.Move(moveDirection);


        //todo: make addons invisible on edges
    }

	public override void Swapping(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 swapDirection = context.ReadValue<Vector2>();
            GameManager.instance.MoreButtonsCursorSwap(this.tag, xPos, yPos, swapDirection);
        }
    }
}
