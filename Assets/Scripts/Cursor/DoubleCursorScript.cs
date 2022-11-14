using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DoubleCursorScript : CursorScript
{
	Direction currentDirection;

	Transform secondaryCursor;

	

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
		//aesthetic: repeat ramp speed on hold
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
		//aesthetic: repeat ramp speed on hold
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
		if (context.started)
		{
			GameManager._.DoubleCursorSwap(this.tag, xPos, yPos, currentDirection);
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

	public override void QuickAttack(InputAction.CallbackContext context)
	{
		throw new System.NotImplementedException();
	}

	public override void QuickShield(InputAction.CallbackContext context)
	{
		throw new System.NotImplementedException();
	}

	public override void BoltAttack(InputAction.CallbackContext context)
	{
		throw new System.NotImplementedException();
	}

	public override void RotateAbility(InputAction.CallbackContext context)
	{
		throw new System.NotImplementedException();
	}
}
