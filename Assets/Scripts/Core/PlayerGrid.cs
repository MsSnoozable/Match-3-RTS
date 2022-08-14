using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Security.Cryptography;


public class PlayerGrid : MonoBehaviour
{
    #region Public Fields

    public Transform[] rows = new Transform[GridHeight];
    public Transform[] cols = new Transform[GridWidth];
    [HideInInspector] public const int GridWidth = 8;
    [HideInInspector] public const int GridHeight = 7;
    [HideInInspector] public const int MinColumn = 1;
    public UnitController[,] GridArray = new UnitController[GridWidth, GridHeight];
    [SerializeField] GameObject[] availableSmallUnits = new GameObject[5];

    #endregion

    #region Private Fields

    [SerializeField] Transform fieldUnits;


    int numOfSmallUnits;


    List<UnitController> successfulMatches;

    #endregion


    private void Start()
	{
	}
	private void OnDestroy()
	{
        GameManager.instance.onDoubleCursorSwap -= Swap;
    }

	private void Awake()
	{
		InitSpawnUnits();
        
        GameObject cursor = Instantiate(GameManager.instance.GetCursor(this.gameObject.tag), transform);
        //todo: could add pg in cursor here for cleaner observer pattern

        GameManager.instance.onDoubleCursorSwap += Swap;

    }

    public void Swap(int xPos, int yPos, Direction currentDirection)
    {
        UnitController unit = this.GridArray[xPos, yPos];
        UnitController secondaryUnit = null;
        //if (unit.isReadyToMove)
        //{
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

            List<UnitController> swapped = new List<UnitController>();
            swapped.Add(unit);

            if (secondaryUnit != null)
            {
                swapped.Add(secondaryUnit);
                secondaryUnit.Move(xPos, yPos); //swaps secondary to current pos
            }

            /*if (gameManager.isSetupComplete)
                //if (!fromSwap bool thing = MatchCheck(xDestination, yDestination);*/

            //todo: this match check should go somwhere else
            StartCoroutine(unit.MatchCheck(swapped));
        //}
    }

    public void ResetBoard(InputAction.CallbackContext context)
    {
        foreach (UnitController uc in GridArray)
		{
            if (uc != null)
                Destroy(uc.gameObject);
		}
        InitSpawnUnits();
	}

	public void MoveRowMulti(int topRow, int bottomRow, int pulledFrom, int destination)
	{
        if (topRow == bottomRow) throw new System.Exception("not a multi row, but called multirow function");

        for (int i = topRow; i <= bottomRow; i++)
		{
            MoveRow(i, pulledFrom, destination);
		}

        //if topRow == 0, bottom row == grid height -1.... then only check front column for matches
	}

    public void MoveRow(int row, int pulledFrom, int destination)
    {
        print(string.Format("r: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));

		if (pulledFrom >= PlayerGrid.MinColumn)
		{
			for (int i = PlayerGrid.MinColumn - 1; i <= pulledFrom; i++)
			{
				UnitController space = GridArray[pulledFrom - i, row].Move(destination - i, row);
			}
		}
        else
		{
            PullFromQueue();
		}
		//if rightmost = -1 pull from queue before iterating
	}

    void PullFromQueue ()
	{
        Debug.LogWarning("queue");
	}

    void AddToQueue (int topOfReplace, int bottomOfReplace, int[] firsColumn)
	{
		
    }

	private void InitSpawnUnits()
	{
        //int[] firstColumn = new int[GridWidth];

        int[] lastChoice_c = new int[GridWidth];
        int[] twoAgoChoice_c = new int[GridWidth]; 

        for (int col = 0; col < GridWidth; col++) //set to 0 to include Queue in initializing. It's easier
		{
            int lastChoice_r = Random.Range(0, availableSmallUnits.Length);
            int twoAgoChoice_r = Random.Range(0, availableSmallUnits.Length);

			for (int row = 0; row < GridHeight; row++)
			{
                //todo: maybe make more random with RandomNumberGenerator
                //RandomNumberGenerator rng = RandomNumberGenerator.Create();
                //rng.GetBytes(new byte[3]);

                int rng = 0;
                do
                {
                    rng = Random.Range(0, availableSmallUnits.Length);
                } while ((col > PlayerGrid.MinColumn) ? rng == twoAgoChoice_r || rng == twoAgoChoice_c[row] 
                    : rng == twoAgoChoice_r);

				GameObject unit = Instantiate(availableSmallUnits[rng], fieldUnits);

                UnitController uc = unit.GetComponent<UnitController>();
				
                uc.pg = this;
				uc.Move(col, row);
                //todo: make the uc add to grdiarray without move. They'll be summoned differently.

                lastChoice_c[row] = rng; //just placed one becomes last choice

                twoAgoChoice_r = lastChoice_r;  
                lastChoice_r = rng; 
			} //end row
            twoAgoChoice_c = lastChoice_c;
        } //end
    }


    bool MatchCheck (Vector2 currentlyCheckedUnitPosition)
	{
        return true;
/*        UnitController = 
        if (currentlyCheckedUnit > PlayerGrid.MinColumn)
		{
            return MatchCheck(something - 1);
		}
        else
            return true;*/
	}
}
