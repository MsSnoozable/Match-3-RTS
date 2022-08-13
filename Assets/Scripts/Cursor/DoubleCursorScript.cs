using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DoubleCursorScript : CursorScript
{
	Direction currentDirection;

	Transform secondaryCursor;

	// Start is called before the first frame update
	private void Start()
	{
		currentDirection = Direction.Down;
		secondaryCursor = transform.GetChild(0);
	}

	private void Awake()
	{
		pg = this.GetComponentInParent<PlayerGrid>();
		transform.position = new Vector2(pg.cols[xPos].position.x, pg.rows[yPos].position.y);
	}


	public void RotateLeft(InputAction.CallbackContext context)
	{
		//todo: repeat ramp speed on hold
		if (context.started)
		{
			Vector3 secondaryCursorMove = Vector3.zero;
			switch (currentDirection)
			{
				case Direction.Down:
					currentDirection = Direction.Right;
					secondaryCursorMove = Vector3.right;
					if (xPos == PlayerGrid.GridWidth - 1) Move(Vector2.left);
					break;
				case Direction.Right:
					currentDirection = Direction.Up;
					secondaryCursorMove = Vector3.up;
					if (yPos == 0) Move(Vector2.down);
					break;
				case Direction.Up:
					currentDirection = Direction.Left;
					secondaryCursorMove = Vector3.left;
					if (xPos == PlayerGrid.MinColumn) Move(Vector2.right);
					break;
				case Direction.Left:
					currentDirection = Direction.Down;
					secondaryCursorMove = Vector3.down;
					if (yPos == PlayerGrid.GridHeight - 1) Move(Vector2.up);
					break;
			}
			secondaryCursor.DOLocalMove(secondaryCursorMove, moveDuration).SetEase(Ease.OutCirc);
		}
	}

	public void RotateRight(InputAction.CallbackContext context)
	{
		//todo: repeat ramp speed on hold
		if (context.started)
		{
			Vector3 secondaryCursorMove = Vector3.zero;
			switch (currentDirection)
			{
				case Direction.Down:
					currentDirection = Direction.Left;
					secondaryCursorMove = Vector3.left;
					if (xPos == PlayerGrid.MinColumn) Move(Vector2.right);
					break;
				case Direction.Left:
					currentDirection = Direction.Up;
					secondaryCursorMove = Vector3.up;
					if (yPos == 0) Move(Vector2.down);
					break;
				case Direction.Up:
					currentDirection = Direction.Right;
					secondaryCursorMove = Vector3.right;
					if (xPos == PlayerGrid.GridWidth - 1) Move(Vector2.left);
					break;
				case Direction.Right:
					currentDirection = Direction.Down;
					secondaryCursorMove = Vector3.down;
					if (yPos == PlayerGrid.GridHeight - 1) Move(Vector2.up);
					break;
			}
			secondaryCursor.DOLocalMove(secondaryCursorMove, moveDuration).SetEase(Ease.OutCirc);
		}
	}

	public override void Swapping(InputAction.CallbackContext context)
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

			/*if (gameManager.isSetupComplete)
			{
				//if (!fromSwap)
				bool thing = MatchCheck(xDestination, yDestination);
				//print("Match was " + (thing ? "made succesffuly!!!" : "not made... you suck"));
			}*/


			//todo: fuse these into one by passing in a list of uc

			List<UnitController> swapped = new List<UnitController>();
			swapped.Add(unit);
			swapped.Add(secondaryUnit);

			//StartCoroutine(unit.MatchCheck(unit.xPos, unit.yPos));
			//StartCoroutine(secondaryUnit.MatchCheck(secondaryUnit.xPos, secondaryUnit.yPos));

		}
	}

	private int directionCheck(Direction direction) => currentDirection == direction ? 1 : 0;

	protected override void Move(Vector2 moveDirection)
	{
		xPos += Mathf.RoundToInt(moveDirection.x);
		yPos -= Mathf.RoundToInt(moveDirection.y); //needs minus to invert

		//restricts based on direction
		int leftEdge = directionCheck(Direction.Left) + 1;
		int topEdge = directionCheck(Direction.Up);
		int rightEdge = PlayerGrid.GridWidth - 1 - directionCheck(Direction.Right);
		int bottomEdge = PlayerGrid.GridHeight - 1 - directionCheck(Direction.Down);

		xPos = Mathf.Clamp(xPos, leftEdge, rightEdge);
		yPos = Mathf.Clamp(yPos, topEdge, bottomEdge);

		transform.DOMove(new Vector2(
			pg.cols[xPos].position.x,
			pg.rows[yPos].position.y), moveDuration).SetEase(Ease.OutCirc);
	}
}
