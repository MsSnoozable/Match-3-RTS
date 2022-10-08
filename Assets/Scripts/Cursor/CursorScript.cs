using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public abstract class CursorScript : MonoBehaviour
{
	#region Public Fields
	public float moveDuration = 0;
	public PlayerInput playerInput;
	[HideInInspector] public PlayerGrid pg;
	#endregion

	#region Private Fields
	protected int xPos = 3;
	protected int yPos = 2;
	IEnumerator VertMove;
	IEnumerator HoriMove;

	/*Vector2 currentVertDirection;
	Vector2 currentHoriDirection;*/
	#endregion

	#region MonoBehavriour

	protected void Awake()
	{
		pg = this.GetComponentInParent<PlayerGrid>();
		transform.position = new Vector2(pg.cols[xPos].position.x, pg.rows[yPos].position.y);
	}
	#endregion

	#region Controls

	//todo: make all the pg dependencies use events to gm so it's decoupled

	public abstract void Swapping(InputAction.CallbackContext context);
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
			//broken: trying to fix overlap 1d axis bug
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

	public void DeleteUnit (InputAction.CallbackContext context)
	{
		pg.GridArray[xPos, yPos].RemoveUnit();
	}

	public void MinorAbility(InputAction.CallbackContext context)
	{
		if (context.performed)
			print("minor");
	}
	public void UltimateAbility(InputAction.CallbackContext context)
	{
		if (context.performed)
			print("ultimate");
	}

	#endregion

	protected virtual void Move (Vector2 moveDirection)
	{
		xPos += Mathf.RoundToInt(moveDirection.x);
		yPos -= Mathf.RoundToInt(moveDirection.y); //needs minus to invert

		xPos = Mathf.Clamp(xPos, PlayerGrid.MinColumn, PlayerGrid.GridWidth - 1);
		yPos = Mathf.Clamp(yPos, 0, PlayerGrid.GridHeight - 1);

		transform.DOMove(new Vector2(
			pg.cols[xPos].position.x,
			pg.rows[yPos].position.y), moveDuration).SetEase(Ease.OutCirc);
	}

	IEnumerator MoveFromHoldInput (Vector2 moveDirection)
	{
		while (true)
		{
			Move (moveDirection);
			yield return new WaitForSeconds(0.05f);
		}
	}
}
