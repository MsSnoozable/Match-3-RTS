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
    #endregion

    #region Private Fields
    int xPos = 0;
    int yPos = 0;
	Direction currentDirection;
	#endregion

	private void Update()
	{
		Movement();
		Rotation();
		Swapping();
	}
	private void Start()
	{
		currentDirection = Direction.Down;
	}
		
	void test (InputAction.CallbackContext contxt)
	{

	}
	void Rotation ()
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
	}

	void Swapping ()
	{
		if (Input.GetKeyDown("x"))
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
	
	private void Movement()
	{
		if (Input.GetKeyDown("w") &&
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
		}
	}

	private int directionCheck(Direction direction)
	{
		return (currentDirection == direction) ? 1 : 0;
	}
}
