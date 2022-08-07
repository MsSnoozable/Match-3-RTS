using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerStats P1;
    public PlayerStats P2;

	public bool isSetupComplete;

	private void Start()
	{
	    isSetupComplete = false;
		StartCoroutine(Countdown());
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