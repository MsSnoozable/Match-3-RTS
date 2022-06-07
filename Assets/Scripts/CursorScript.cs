using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CursorScript : MonoBehaviour
{
	#region Public Fields
    public float moveDuration = 0;
    public PlayerGrid pg;
    #endregion

    #region Private Fields
    int xPos = 0;
    int yPos = 0;
	#endregion
	Direction currentDirection;

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
		//TODO: restrict movement based on direction of secondary selection
		if (Input.GetKeyDown("d") && xPos < PlayerGrid.GridWidth - 1)
		{
			transform.DOMoveX(pg.cols[++xPos].position.x, moveDuration);
		}
		else if (Input.GetKeyDown("a") && xPos > 0)
		{
			transform.DOMoveX(pg.cols[--xPos].position.x, moveDuration);
		}
		else if (Input.GetKeyDown("s") && yPos < PlayerGrid.GridHeight - 1)
		{
			transform.DOMoveY(pg.rows[++yPos].position.y, moveDuration);
		}
		else if (Input.GetKeyDown("w") && yPos > 0)
		{
			transform.DOMoveY(pg.rows[--yPos].position.y, moveDuration);
		}
	}


}
