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
	int xPos = 3;
	int yPos = 2;
	Direction currentDirection;
	Transform secondaryCursor;
	//bool isHoldingMove = false;
	IEnumerator VertMove;
	IEnumerator HoriMove;

	/*Vector2 currentVertDirection;
	Vector2 currentHoriDirection;*/
	#endregion

	#region MonoBehavriour
	private void Start()
	{
		currentDirection = Direction.Down;
		secondaryCursor = transform.GetChild(0);
	}

	private void Awake()
	{
		transform.position = new Vector2(pg.cols[xPos].position.x, pg.rows[yPos].position.y);
	}
	#endregion

	#region Controls

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
					if (xPos == 0) Move(Vector2.right);
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
					if (xPos == 0) Move(Vector2.right);
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

			CheckMatches(unit);
			CheckMatches(secondaryUnit);
		}
	}

	public void MoveHorizontal (InputAction.CallbackContext context)
	{
		Vector2 moveDirection = new Vector2(context.ReadValue<float>(), 0);

		if (context.started)
		{
			//currentHoriDirection = moveDirection;
			Move(moveDirection);
		}
		else if (context.performed)
		{
			HoriMove = MoveFromHoldInput(moveDirection);
			StartCoroutine(HoriMove);
			//todo: trying to fix overlap 1d axis bug
			/*if (currentHoriDirection != moveDirection)
			{
				StopCoroutine(HoriMove);
				StartCoroutine(HoriMove);
			}*/
		}
		else if (context.canceled)
		{
			if (HoriMove != null)
				StopCoroutine(HoriMove);
		}
	}
	public void MoveVertical(InputAction.CallbackContext context)
	{
		Vector2 moveDirection = new Vector2 (0, context.ReadValue<float>());

		if (context.started)
		{
			Move(moveDirection);
		}
		else if(context.performed)
		{
			VertMove = MoveFromHoldInput(moveDirection);
			StartCoroutine(VertMove);
		}
		else if (context.canceled)
		{
			if (VertMove != null)
				StopCoroutine(VertMove);
		}
	}

	private int directionCheck(Direction direction) => currentDirection == direction ? 1 : 0;

	#endregion

	/*	private bool CheckMatches(UnitController unit, UnitController secondaryUnit)
		{
			UnitData u1 = unit.GetComponent<UnitController>().unit;
			UnitData u2 = secondaryUnit.GetComponent<UnitController>().unit;
			UnitData u3 = pg.GridArray[unit.xPos + 1, unit.yPos].unit;
			if (u1 == u2)
			{
				pg.MoveSubRow(unit.yPos, unit.xPos - 1, secondaryUnit.xPos, currentDirection);

				u1.Attack(unit.gameObject);
				u2.Attack(secondaryUnit.gameObject);
				return true;
			}

			return false;
		}*/

	private void CheckMatches (UnitController Check)
	{

	}


	void Move (Vector2 moveDirection)
	{
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
	}

	IEnumerator MoveFromHoldInput (Vector2 moveDirection)
	{
		while (true)
		{
			Move (moveDirection);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
