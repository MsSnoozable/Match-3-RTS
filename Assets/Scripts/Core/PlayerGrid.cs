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
    //todo: numOfSmallUnits goes to this, but added dynammically?
	[SerializeField] GameObject[] availableSmallUnits = new GameObject[5];

    #endregion

    #region Private Fields

    int numOfSmallUnits;
    //note: maybe needed? //List<UnitController> successfulMatches;

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
        //optimization: could add pg in cursor here for cleaner observer pattern

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

            //note: this match check should go somwhere else
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
        GameManager.instance.isSetupComplete = false;
        StartCoroutine(GameManager.instance.Countdown());
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
    //todo: might split into two types of functions
    public void MoveRow(int row, int pulledFrom, int destination)
    {
        print(string.Format("r: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));

        if (pulledFrom > destination && pulledFrom <= PlayerGrid.GridWidth - 1) //pull from right
        {
            for (int i = PlayerGrid.GridWidth - 1; i >= pulledFrom; i--)
            {
                print(i);
                GridArray[i, row].Move(i - 1, row);
            }
        }
        //optimization: super messy can be cleaned. Maybe get rid of "destination"?
        else if (pulledFrom < destination) //pull from left/queue
        {
            for (int i = 0; i < destination + 1; i++)
            {
                if (pulledFrom - i >= 0) //in array bounds
                {
                    GridArray[pulledFrom - i, row].Move(destination - i, row);
                }
                else //pulling from queue
                {
                    StartCoroutine(AddToQueue(row));
                    GridArray[0, row].Move(destination - i, row);
                }
            }
        }
        else { throw new System.Exception(string.Format("pulledFrom == destination... ????!!! pF: {0}, dest: {1}", pulledFrom, destination)); }


        /*if (pulledFrom < PlayerGrid.GridWidth - 1)
		{
		}
        else if (pulledFrom >= PlayerGrid.GridWidth - 1)
		{
            //donothign?
            print("do nothing?");
		}
        else
		{
            throw new System.Exception("out of bounds pull attempt");
		}*/
	}

    IEnumerator AddToQueue (int row /*int topOfReplace, int bottomOfReplace, int[] firsColumn*/)
	{
        int rng = 0;
        do
        {
            rng = RandomUnitIndex();
        } while (AdjacentMatches(row, rng));

        UnitController uc = CreateUnit(rng, row);
        uc.Move(0, row);
        yield return new WaitForSeconds(UnitData.moveDuration);
    }

    //qa check: don't know if this works properly right now. Might be messed up from the artifacting glitch
    bool AdjacentMatches (int row, int attemptedIndex)
	{
        int topRow = Mathf.Clamp(row - 2, 0, PlayerGrid.GridHeight - 1);
        int bottomRow = Mathf.Clamp(row + 2, 0, PlayerGrid.GridHeight - 1);

        int verticalMatches = 0;
        for (int i = row; i > topRow; i--)
		{
            if (GridArray[0, i].data.color.Equals(attemptedIndex))
                verticalMatches++; //check top
		}

        for (int i = row; i < bottomRow; i++)
		{
            if (GridArray[0, i].data.color.Equals(attemptedIndex))
                verticalMatches++; //check down
        }

        if (verticalMatches >= 3) return true;

        int rightMatches = 0;
        for (int i = 0; i < 2; i++)
        {
            if (GridArray[i, row].data.color.Equals(attemptedIndex))
                rightMatches++; //check right
        }

        if (rightMatches >= 3) return true;

        return false;
	}

    private void InitSpawnUnits()
	{
        int[] lastChoice_c = new int[GridWidth];
        int[] twoAgoChoice_c = new int[GridWidth]; 

        for (int col = 0; col < GridWidth; col++) //set to 0 to include Queue in initializing. It's easier
		{
            int lastChoice_r = RandomUnitIndex();
            int twoAgoChoice_r = RandomUnitIndex();

			for (int row = 0; row < GridHeight; row++)
			{
                //todo: maybe make more random with RandomNumberGenerator
                //RandomNumberGenerator rng = RandomNumberGenerator.Create();
                //rng.GetBytes(new byte[3]);

                int rng = 0;
                do
                {
                    rng = RandomUnitIndex();
                } while ((col > PlayerGrid.MinColumn) ? rng == twoAgoChoice_r || rng == twoAgoChoice_c[row] : rng == twoAgoChoice_r);

                UnitController uc = CreateUnit(rng, row);
				uc.Move(col, row);
                //todo: make the uc add to grdiarray without move. They'll be summoned differently.

                lastChoice_c[row] = rng; //just placed one becomes last choice

                twoAgoChoice_r = lastChoice_r;  
                lastChoice_r = rng; 
			} //end row
            twoAgoChoice_c = lastChoice_c;
        } //end
    }

    UnitController CreateUnit (int unitIndex, int row)
	{
        GameObject unit = Instantiate(availableSmallUnits[unitIndex], rows[row]);
        UnitController uc = unit.GetComponent<UnitController>();
        uc.pg = this;
        uc.unitIndex = unitIndex;  
        return uc;
    }

    int RandomUnitIndex () => Random.Range(0, availableSmallUnits.Length);

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
