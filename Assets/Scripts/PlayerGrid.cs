using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Security.Cryptography;


public class PlayerGrid : MonoBehaviour
{
    #region Public Fields

    public Transform[] rows = new Transform[GridWidth];
    public Transform[] cols = new Transform[GridHeight];
    [HideInInspector] public const byte GridWidth = 7;
    [HideInInspector] public const byte GridHeight = 7;

    #endregion

    #region Private Fields

    public UnitController[,] GridArray = new UnitController[GridWidth, GridHeight];

    #endregion

    [SerializeField] Transform fieldUnits;
    [SerializeField] GameObject UnitA;
    [SerializeField] GameObject UnitB;
    [SerializeField] GameObject UnitC;

    int numOfSmallUnits;

    GameObject[] availableSmallUnits;

    List<UnitController> successfulMatches;

    private void Start()
	{
        availableSmallUnits = new GameObject[] { UnitA, UnitB, UnitC };
		InitSpawnUnits();
	}

    public void MoveSubRow(int row, int rightMostUnit, int emptySpace, Direction direction)
    {
        if (direction == Direction.Right)
        {
            for (int i = 0; i <= rightMostUnit; i++)
            {
                GridArray[rightMostUnit - i, row].Move(emptySpace - i, row);
            }
        }

        //if rightmost = -1 pull from queue before iterating
    }

    private void InitSpawnUnits()
	{
		for (int i = 0; i < GridWidth; i++)
		{
			for (int j = 0; j < GridHeight; j++)
			{
                //todo: maybe make more random with RandomNumberGenerator
                //RandomNumberGenerator rng = RandomNumberGenerator.Create();
                //rng.GetBytes(new byte[3]);

                int r = Random.Range(0, 3);
				GameObject unit = Instantiate(availableSmallUnits[r], fieldUnits);

                UnitController uc = unit.GetComponent<UnitController>();
				uc.pg = this;
				uc.Move(i, j);
			}
		}
	}

    bool MatchCheck (Vector2 currentlyCheckedUnitPosition)
	{
        return true;
/*        UnitController = 
        if (currentlyCheckedUnit > 0)
		{
            return MatchCheck(something - 1);
		}
        else
            return true;*/
	}
}
