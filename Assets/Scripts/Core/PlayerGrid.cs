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


    public bool matchMakingComplete = false;


    #endregion
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

        if (secondaryUnit != null) secondaryUnit.Move(xPos, yPos); //swaps secondary to current pos
        MoveHappened(new List<UnitController> { unit, secondaryUnit });
    }

    public void ResetBoard(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            foreach (UnitController uc in GridArray)
            {
                if (uc != null)
                    Destroy(uc.gameObject);
            }
            GameManager.instance.isSetupComplete = false;
            StartCoroutine(GameManager.instance.RoundStartCountdown());
            InitSpawnUnits();
        }
    }

	public void MoveRowMulti(int topRow, int bottomRow, int pulledFrom, int destination)
	{
        if (topRow == bottomRow) throw new System.Exception("not a multi row, but called multirow function");

        //print(String.Format("bot: {0}, top: {1}",bottomRow, topRow));

        for (int i = topRow; i <= bottomRow; i++)
		{
            StartCoroutine(MoveRow(i, pulledFrom, destination));
		}

        //if topRow == 0, bottom row == grid height -1.... then only check front column for matches
	}


    //todo: might split into two types of functions
    public IEnumerator MoveRow(int row, int pulledFrom, int destination)
    {
        //print(string.Format("r: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));

        yield return new WaitUntil(() => matchMakingComplete);
        yield return new WaitForSeconds(UnitData.moveDuration);

        List<UnitController> toBeChecked = new List<UnitController>();

        if (pulledFrom > destination && pulledFrom <= PlayerGrid.GridWidth - 1) //pull from right
        {
            //needs to move from left most not right most
            for (int i = pulledFrom; i <= PlayerGrid.GridWidth - 1; i++)
            {
                GridArray[i, row].Move(i - 1, row);

                //todo: only on certain conditions add to list
                toBeChecked.Add(GridArray[i - 1, row]);
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

                //todo: more specific with which get added
                toBeChecked.Add(GridArray[destination - i, row]);
            }
        }
        else if (pulledFrom == destination) { throw new System.Exception(string.Format("pulledFrom == destination... ????!!! pF: {0}, dest: {1}", pulledFrom, destination)); }

        MoveHappened(toBeChecked);
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
				uc.Summon(col, row);

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

    List<UnitController> toBeChecked = new List<UnitController>();

    //todo: list of tobechecked but somehow say which directions are valid or not??? like toBeChecked vs toBeChecked only vertically.
    //essentially trying to store which ones in the check algorithm can skip certain parts

    bool wasMoved = false;
    public void MoveHappened(List<UnitController> toBeAdded)
    {
        foreach (UnitController item in toBeAdded)
        {
            if (item == null) print("problem");

            toBeChecked.Add(item);
        }
        wasMoved = true;
    }

    private void LateUpdate()
    {
        if (wasMoved)
        {
            MatchCheck(toBeChecked);

            toBeChecked.Clear();
            wasMoved = false;
        }
    }


    //calls for each moved unit
    //todo: make this on GM or PG? so matches are handled outside of here
    public void MatchCheck(List<UnitController> passedList)
    {
        int absoluteRightMost = PlayerGrid.MinColumn;

        List<UnitController> list = new List<UnitController>(passedList);
        foreach (UnitController checking in list)
        {
            if (!checking.movable) continue;
            List<UnitController> attackers = new List<UnitController>() { checking };
            List<UnitController> shielders = new List<UnitController>() { checking };

            int x = checking.xPos;
            int y = checking.yPos;

            #region AddMatchesToList
            UnitController movedUnit = checking.pg.GridArray[x, y];
            UnitController nextUnit;

            for (int left = -1; left >= -checking.xPos; left--)
            {
                if (x + left >= PlayerGrid.MinColumn) //in bounds
                {
                    nextUnit = checking.pg.GridArray[x + left, y];
                    if (movedUnit.data == nextUnit.data) attackers.Add(nextUnit);
                    else break;
                }
            }
            for (int up = -1; up >= -checking.yPos; up--)
            {
                if (y + up >= 0) //in bounds
                {
                    nextUnit = checking.pg.GridArray[x, y + up];
                    if (movedUnit.data == nextUnit.data) shielders.Add(nextUnit);
                    else break;
                }
            }
            for (int right = 1; right <= PlayerGrid.GridWidth - 1 - checking.xPos; right++)
            {
                if (x + right < PlayerGrid.GridWidth)
                {
                    nextUnit = checking.pg.GridArray[x + right, y];
                    if (movedUnit.data == nextUnit.data) attackers.Add(nextUnit);
                    else break;
                }
            }

            for (int down = 1; down <= PlayerGrid.GridHeight - 1 - checking.yPos; down++)
            {
                if (y + down < PlayerGrid.GridHeight)
                {
                    nextUnit = checking.pg.GridArray[x, y + down];
                    if (movedUnit.data == nextUnit.data) shielders.Add(nextUnit);
                    else break;
                }
            }
            #endregion

            #region Shield/Attack
            if (shielders.Count >= 3)
            {
                int bottomMost = 0;
                int topMost = PlayerGrid.GridHeight - 1;

                string output = "shield: ";

                foreach (UnitController uc in shielders)
                {
                    output += uc.yPos + ", ";
                    if (uc.yPos > bottomMost) bottomMost = uc.yPos;
                    if (uc.yPos < topMost) topMost = uc.yPos;
                }

                print(output);
                checking.StartCoroutine(checking.Shield(shielders));
                MoveRowMulti(topMost, bottomMost, shielders[0].xPos + 1, shielders[0].xPos);
            }

            if (attackers.Count >= 3)
            {
                int rightMost = PlayerGrid.MinColumn;
                int leftMost = PlayerGrid.GridWidth - 1;

                string output = "attack: ";

                foreach (UnitController uc in attackers)
                {
                    output += uc.xPos + ", ";

                    if (uc.xPos < leftMost) leftMost = uc.xPos;
                    if (uc.xPos > rightMost) rightMost = uc.xPos;
                }
                checking.StartCoroutine(checking.Attack(attackers));

                StartCoroutine(MoveRow(attackers[0].yPos, leftMost - 1, rightMost - 1)); //moves the left side in
                StartCoroutine(MoveRow(attackers[0].yPos, rightMost + 1, rightMost)); //moves the right side back

                print(output);
                //todo: add this to modify the movment of things after a match
                if (rightMost > absoluteRightMost) absoluteRightMost = rightMost;
            }
            #endregion
        } //for each end

        matchMakingComplete = true;
        print("match check finished");
        /*todo: after whole loop foreach ends. move all to total to mostRight/Left/Up/Down? instead.. pg.MoveRow etc.
        add delay for each shield/attack fusion and movement animation*/
    }

    int RandomUnitIndex () => Random.Range(0, availableSmallUnits.Length);

}
