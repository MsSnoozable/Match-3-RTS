using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class CursorScript : MonoBehaviour
{
	#region Public Fields
	public float moveDuration = 0;
	public PlayerGrid pg;
	public PlayerInput playerInput;
	#endregion

	#region Private Fields
	int xPos = 0;
	int yPos = 0;
	Direction currentDirection;
	//bool isHoldingMove = false;
	#endregion

	#region MonoBehavriour
	private void Start()
	{
		currentDirection = Direction.Down;
	}
	#endregion

	public void RotateLeft(InputAction.CallbackContext context)
	{
		//todo: repeat ramp speed on hold
		if (context.started)
		{
			switch (currentDirection)
			{
				case Direction.Down:
					currentDirection = Direction.Right;
					if (xPos == PlayerGrid.GridWidth - 1) Move(Vector2.left);
					break;
				case Direction.Right:
					currentDirection = Direction.Up;
					if (yPos == 0) Move(Vector2.down);
					break;
				case Direction.Up:
					currentDirection = Direction.Left;
					if (xPos == 0) Move(Vector2.right);
					break;
				case Direction.Left:
					currentDirection = Direction.Down;
					if (yPos == PlayerGrid.GridHeight - 1) Move(Vector2.up);
					break;
			}
			transform.DORotate(new Vector3(0, 0, (float)currentDirection), moveDuration).SetEase(Ease.OutCirc);
		}
    }

	public void RotateRight(InputAction.CallbackContext context)
	{
		//todo: repeat ramp speed on hold
		if (context.started)
		{
			switch (currentDirection)
			{
				case Direction.Down:
					currentDirection = Direction.Left;
					if (xPos == 0) Move(Vector2.right);
					break;
				case Direction.Left:
					currentDirection = Direction.Up;
					if (yPos == 0) Move(Vector2.down);
					break;
				case Direction.Up:
					currentDirection = Direction.Right;
					if (xPos == PlayerGrid.GridWidth - 1) Move(Vector2.left);
					break;
				case Direction.Right:
					currentDirection = Direction.Down;
					if (yPos == PlayerGrid.GridHeight - 1) Move(Vector2.up);
					break;
			}
			transform.DORotate(new Vector3(0, 0, (float)currentDirection), moveDuration).SetEase(Ease.OutCirc);
		}
	}

	public void Swapping (InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			UnitController unit = pg.GridArray[xPos, yPos];
			UnitController secondaryUnit = null;
			switch (currentDirection)
			{
				case Direction.Down:
					secondaryUnit = unit.Move(xPos, yPos + 1);
					break;
				case Direction.Right:
					secondaryUnit = unit.Move(xPos + 1, yPos);
					break;
				case Direction.Up:
					secondaryUnit = unit.Move(xPos, yPos - 1);
					break;
				case Direction.Left:
					secondaryUnit = unit.Move(xPos - 1, yPos);
					break;
			}
			secondaryUnit.Move(xPos, yPos); //swaps secondary to current pos

			UnitData u1 = unit.GetComponent<UnitController>().unit;
			UnitData u2 = secondaryUnit.GetComponent<UnitController>().unit;
			if (u1 == u2)
			{
				u1.Attack();
			}
		}
	}
	void Move (Vector2 moveDirection)
	{
/*		do
		{*/
			xPos += Mathf.RoundToInt(moveDirection.x);
			yPos -= Mathf.RoundToInt(moveDirection.y); //needs minus to invert

			//restricts based on direction
			int leftEdge = directionCheck(Direction.Left);
			int topEdge = directionCheck(Direction.Up);
			int rightEdge = PlayerGrid.GridWidth - 1 - directionCheck(Direction.Right);
			int bottomEdge = PlayerGrid.GridHeight - 1 - directionCheck(Direction.Down);

			xPos = Mathf.Clamp(xPos, leftEdge, rightEdge);
			yPos = Mathf.Clamp(yPos, topEdge, bottomEdge);

			transform.DOMove(new Vector2(
				pg.cols[xPos].position.x,
				pg.rows[yPos].position.y), moveDuration).SetEase(Ease.OutCirc);
			
	/*		yield return new WaitForSeconds(1f);

		} while (isHoldingMove);*/
	}

	public void Move(InputAction.CallbackContext context)
	{
		//todo: repeat ramp speed on hold
		if (context.started)
		{
			Move(context.ReadValue<Vector2>());
		}
		/*else if (context.performed)
		{
			isHoldingMove = true;
		}
		else if (context.canceled)
		{
			isHoldingMove = false;
		}*/
	}
	private int directionCheck(Direction direction) => currentDirection == direction ? 1 : 0;
	
}
