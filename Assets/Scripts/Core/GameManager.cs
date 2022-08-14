using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

internal enum cursorOptions {
	doubleCursor, singleCursor, moreButtonCursor
}

public class GameManager : MonoBehaviour
{
	//todo: refactor with singleton pattern... ie. static "instance"
	public static GameManager instance;


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
		StartCoroutine(Countdown());
	}

	private void Awake()
	{
		instance = this;
		player1ChosenCursor = SetCurosrs(player1CursorMode);
		player2ChosenCursor = SetCurosrs(player2CursorMode);
	}

	public event Action<int, int, Direction> onDoubleCursorSwap;

	public void DoubleCursorSwap(int xPos, int yPos, Direction curosrOrientation) => onDoubleCursorSwap?.Invoke(xPos, yPos, curosrOrientation);

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
	
	IEnumerator Countdown ()
	{
		yield return new WaitForSeconds(3f);
		isSetupComplete=true;
	}


	private void Update()
	{

	}
}