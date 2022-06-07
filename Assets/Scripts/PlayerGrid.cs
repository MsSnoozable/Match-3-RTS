using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public UnitController Wiz1;
    public UnitController Wiz2;

    private void Start()
	{
        GridArray[3, 4] = Wiz1;
        GridArray[3, 3] = Wiz2;
    }

	//should be used after any movement/swap/fusion
	//if the match was done from a swap it doesn't add to chain value
	/*void MatchCheck (bool fromSwap)
    {
        //use a for loop to only check things that moved
        //

        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                if (GridArray[i, j].functions.justMoved)
				{
                    if (GridArray[i + 1, j].color == GridArray[i , j].color)
					{
                        if (GridArray[i - 1, j].color == GridArray[i, j].color)
                        {

                        }
                    }
				}                    
            }
        }


        if (!fromSwap)
        {
            //add to chain multiplier
        }
    }*/
}
