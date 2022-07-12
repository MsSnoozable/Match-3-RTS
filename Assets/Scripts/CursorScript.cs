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
	#endregion

	#region MonoBehavriour
	private void Update()
	{
		/*Movement();
		Rotation();
		Swapping();*/
	}
	private void Start()
	{
		currentDirection = Direction.Down;

		playerInput.onControlsChanged += PlayerInput_onControlsChanged;
		playerInput.onDeviceLost += PlayerInput_onDeviceLost;
		playerInput.onDeviceRegained += PlayerInput_onDeviceRegained;

	}

	#region Not Used RN
	private void PlayerInput_onDeviceRegained(PlayerInput obj)
	{
		throw new System.NotImplementedException();
	}

	private void PlayerInput_onDeviceLost(PlayerInput obj)
	{
		throw new System.NotImplementedException();
	}

	private void PlayerInput_onControlsChanged(PlayerInput obj)
	{
		throw new System.NotImplementedException();
	}
	#endregion

	#endregion

	void test (InputAction.CallbackContext contxt)
	{

	}
	
	/*void Rotation ()
	{
		//changes direction of cursor
		if (Input.GetKeyDown("z") )
		{
			switch (currentDirection)
			{
				case Direction.Down:
					currentDirection = Direction.Right;
					break;
				case Direction.Right:
					currentDirection = Direction.Up;
					break;
				case Direction.Up:
					currentDirection = Direction.Left;
					break;
				case Direction.Left:
					currentDirection = Direction.Down;
					break;
			}
			transform.DORotate(new Vector3(0, 0, (float)currentDirection), moveDuration);
		}
	}*/

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
		}
	}
	
	public void Movement(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			print(yPos);
			Vector2 moveDirection = context.ReadValue<Vector2>();

			if (yPos > 0 + directionCheck(Direction.Up))
			{
				transform.DOMoveY(pg.rows[--yPos].position.y, moveDuration);
			}
			else if (yPos < PlayerGrid.GridHeight - 1 - directionCheck(Direction.Down))
			{
				transform.DOMoveY(pg.rows[++yPos].position.y, moveDuration);
			}
			/*
			else if (xPos > 0 + directionCheck(Direction.Left))
			{
				transform.DOMoveX(pg.cols[--xPos].position.x, moveDuration);
			}
			
			else if (xPos < PlayerGrid.GridWidth - 1 - directionCheck(Direction.Right))
			{
				transform.DOMoveX(pg.cols[++xPos].position.x, moveDuration);
			}*/


			/*if (Input.GetKeyDown("w") &&
				yPos > 0 + directionCheck(Direction.Up))
			{
				transform.DOMoveY(pg.rows[--yPos].position.y, moveDuration);
			}
			else if (Input.GetAxis("Horizontal") < 0 &&
			   xPos > 0 + directionCheck(Direction.Left))
			{
				transform.DOMoveX(pg.cols[--xPos].position.x, moveDuration);
			}
			else if (Input.GetKeyDown("s") &&
				yPos < PlayerGrid.GridHeight - 1 - directionCheck(Direction.Down))
			{
				transform.DOMoveY(pg.rows[++yPos].position.y, moveDuration);
			}
			else if (Input.GetKeyDown("d") &&
				xPos < PlayerGrid.GridWidth - 1 - directionCheck(Direction.Right))
			{
				transform.DOMoveX(pg.cols[++xPos].position.x, moveDuration);
			}*/
		}
	}

	private int directionCheck(Direction direction)
	{
		return (currentDirection == direction) ? 1 : 0;
	}
	
}
