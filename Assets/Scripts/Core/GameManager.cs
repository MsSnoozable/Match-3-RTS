using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

internal enum cursorOptions {
	doubleCursor, singleCursor, moreButtonCursor
}

public class GameManager : MonoBehaviour
{
	public static GameManager _;

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
		_ = this;
		player1ChosenCursor = SetCurosrs(player1CursorMode);
		player2ChosenCursor = SetCurosrs(player2CursorMode);
	}

	public event Action<String, int, int, Direction> OnDoubleCursorSwap;
	public event Action<String, int, int, Vector2> OnMoreButtonCursorSwap;
	public event Action OnSwapFailed;

	//Attacks
	public event Action<UnitAttackInfo> OnAttackCreated;
	public event Action<UnitAttackInfo> OnAttackCombine;
	public event Action<UnitAttackInfo> OnAttackFuse;
	public event Action<UnitAttackInfo> OnAttackHoldStart;
	public event Action<UnitController> OnAttackRelease;

	//Shields
	public event Action<UnitShieldInfo> OnShieldCreated;
	public event Action<UnitShieldInfo> OnShieldFusion;
	public event Action<UnitShieldInfo> OnShieldHoldStart;
	public event Action<UnitController> OnShieldRelease;

	public event Action<int, string> OnUpdateComboCount;
	public event Action<int, string> OnFinalizeComboCount;

	[HideInInspector] public int numOfHeldShields = 0;
	[HideInInspector] public int numOfHeldAttacks = 0;

	public void DoubleCursorSwap(String playerTag, int xPos, int yPos, Direction curosrOrientation) => OnDoubleCursorSwap?.Invoke(playerTag, xPos, yPos, curosrOrientation);
	public void MoreButtonsCursorSwap(String playerTag, int xPos, int yPos, Vector2 swapDirection) => OnMoreButtonCursorSwap?.Invoke(playerTag, xPos, yPos, swapDirection);
	public void SwapFailed() => OnSwapFailed?.Invoke();

	public void AttackCreated(UnitAttackInfo attackInfo) => OnAttackCreated?.Invoke(attackInfo);
	public void AttackCombine(UnitAttackInfo attackInfo) => OnAttackCombine?.Invoke(attackInfo);
	public void AttackFuse(UnitAttackInfo attackInfo) => OnAttackFuse?.Invoke(attackInfo);
	public void AttackHoldStart(UnitAttackInfo attackInfo) => OnAttackHoldStart?.Invoke(attackInfo);
	public void AttackRelease(UnitController attack) => OnAttackRelease?.Invoke(attack);

	public void ShieldHoldStart(UnitShieldInfo shieldInfo) => OnShieldHoldStart?.Invoke(shieldInfo);
	public void ShieldCreated(UnitShieldInfo shieldInfo) => OnShieldCreated?.Invoke(shieldInfo);
	public void ShieldFusion(UnitShieldInfo shieldInfo) => OnShieldFusion?.Invoke(shieldInfo);
	public void ShieldRelease(UnitController shield) => OnShieldRelease?.Invoke(shield);

	public void UpdateComboCount(int comboCount, string tag) => OnUpdateComboCount?.Invoke(comboCount, tag);
	public void FinalizeComboCount (int comboCount) => OnFinalizeComboCount?.Invoke(comboCount, tag);

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


	//need to learn: might be useful as ahelper but doesn't work because ref doesn't work with
	//coroutines....
/*
	public IEnumerator boolDelayFlip (ref bool var, bool value, float delayInSeconds)
	{
		var = value;
		print(value);
		yield return new WaitForSeconds(delayInSeconds);
		var = !value;
		print(value);
	}*/

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