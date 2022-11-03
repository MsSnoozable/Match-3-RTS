using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

internal enum cursorOptions {
	doubleCursor, singleCursor, moreButtonCursor
}

public class GameManager : MonoBehaviour
{
	public static GameManager i;

	[HideInInspector] public PlayerStats P1;
	[HideInInspector] public PlayerStats P2;

	[HideInInspector] public bool isSetupComplete;

	[SerializeField] cursorOptions player1CursorMode;
	[SerializeField] cursorOptions player2CursorMode;

	private GameObject player1ChosenCursor;
	private GameObject player2ChosenCursor;

	[SerializeField] GameObject doubleCursor;
	[SerializeField] GameObject singleCursor;
	[SerializeField] GameObject moreButtonsCursor;

	public float cursorMoveSpeed;
	public float unitMoveSpeed;

	private void Start()
	{
		isSetupComplete = false;
		StartCoroutine(RoundStartCountdown());
	}

	private void Awake()
	{
		i = this;
		player1ChosenCursor = SetCurosrs(player1CursorMode);
		player2ChosenCursor = SetCurosrs(player2CursorMode);
	}

	public event Action<String, int, int, Direction> OnDoubleCursorSwap;
	public event Action<String, int, int, Vector2> OnMoreButtonCursorSwap;
	public event Action OnSwapFailed;

	public event Action<UnitAttackInfo> OnAttackCreated;
	public event Action<UnitAttackInfo> OnAttackCombine;
	public event Action<UnitAttackInfo> OnAttackFuse;
	public event Action<UnitAttackInfo> OnAttackHold;

	public event Action<UnitShieldInfo> OnShieldCreated;
	public event Action<UnitShieldInfo> OnShieldFusion;
	public event Action<UnitShieldInfo> OnShieldHold;

	public void DoubleCursorSwap(String playerTag, int xPos, int yPos, Direction curosrOrientation) => OnDoubleCursorSwap?.Invoke(playerTag, xPos, yPos, curosrOrientation);
	public void MoreButtonsCursorSwap(String playerTag, int xPos, int yPos, Vector2 swapDirection) => OnMoreButtonCursorSwap?.Invoke(playerTag, xPos, yPos, swapDirection);
	public void SwapFailed() => OnSwapFailed?.Invoke();

	public void AttackCreated(UnitAttackInfo attackInfo) => OnAttackCreated?.Invoke(attackInfo);
	public void AttackCombine(UnitAttackInfo attackInfo) => OnAttackCombine?.Invoke(attackInfo);
	public void AttackHold(UnitAttackInfo attackInfo) => OnAttackHold?.Invoke(attackInfo);
	public void AttackFuse(UnitAttackInfo attackInfo) => OnAttackFuse?.Invoke(attackInfo);

	public void ShieldCreated(UnitShieldInfo shieldInfo) => OnShieldCreated?.Invoke(shieldInfo);
	public void ShieldFusion(UnitShieldInfo shieldInfo) => OnShieldFusion?.Invoke(shieldInfo);
	public void ShieldHold(UnitShieldInfo shieldInfo) => OnShieldHold?.Invoke(shieldInfo);


	//todo: refactor so no return like a proper setter
	private GameObject SetCurosrs(cursorOptions cO)
	{
		switch (cO)
		{
			case cursorOptions.doubleCursor:
				return doubleCursor;
			case cursorOptions.singleCursor:
				return singleCursor;
			case cursorOptions.moreButtonCursor:
				return moreButtonsCursor;
			default:
				return null;
		}
	}

	public GameObject GetCursor(string playerNumber) {
		if (playerNumber == "Player 1")
			return player1ChosenCursor;
		else if (playerNumber == "Player 2")
			return player2ChosenCursor;
		else
			throw new SystemException("player number does not match avilable player count");
	}
	
	public IEnumerator RoundStartCountdown ()
	{
		yield return new WaitForSeconds(3f);
		isSetupComplete=true;
	}

}