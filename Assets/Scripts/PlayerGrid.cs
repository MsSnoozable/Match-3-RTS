using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Security.Cryptography;


public class PlayerGrid : MonoBehaviour
{
    #region Public Fields

    public Transform[] rows = new Transform[GridWidth];
    public Transform[] cols = new Transform[GridHeight];
    public Transform[] queueTransforms = new Transform[GridHeight];
    [HideInInspector] public const byte GridWidth = 7;
    [HideInInspector] public const byte GridHeight = 7;
    public UnitController[,] GridArray = new UnitController[GridWidth, GridHeight];
    [HideInInspector] public UnitController[] Queue = new UnitController[GridHeight];

    #endregion

    #region Private Fields

    [SerializeField] Transform fieldUnits;
    [SerializeField] GameObject UnitA;
    [SerializeField] GameObject UnitB;
    [SerializeField] GameObject UnitC;

    int numOfSmallUnits;

    [SerializeField] static GameManager gm;

    GameObject[] availableSmallUnits;

    List<UnitController> successfulMatches;

    #endregion


    private void Start()
	{
        availableSmallUnits = new GameObject[] { UnitA, UnitB, UnitC };
		InitSpawnUnits();
	}

	private void Awake()
	{
        gm = FindObjectOfType<GameManager>();
        Instantiate(gm.GetCursor(this.gameObject.tag), transform);
    }


	public void ResetBoard(InputAction.CallbackContext context)
    {
        foreach (UnitController uc in GridArray)
		{
            Destroy(uc.gameObject);
		}
        InitSpawnUnits();
	}

	public void MoveRowMulti(int topRow, int bottomRow, int pulledFrom, int destination)
	{

        for (int i = topRow; i <= bottomRow; i++)
		{
            print(string.Format("r: {0}, pulled: {1}, dest: {2}", i, pulledFrom, destination));

            MoveRow(i, pulledFrom, destination);
		}
        if (topRow == bottomRow)
		{
            throw new System.Exception("not a multi row");
		}

        //if topRow == 0, bottom row == grid height -1.... then only check front column for matches
	}

    public void MoveRow(int row, int pulledFrom, int destination)
    {

        print(string.Format("r: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));

		if (pulledFrom >= 0)
		{
			for (int i = 0; i <= pulledFrom; i++)
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

	private void InitSpawnUnits()
	{
        int[] firstColumn = new int[GridWidth];

        int[] lastChoice_c = new int[GridWidth];
        int[] twoAgoChoice_c = new int[GridWidth]; 

        for (int col = 0; col < GridWidth; col++)
		{
            int lastChoice_r = Random.Range(0, availableSmallUnits.Length);
            int twoAgoChoice_r = Random.Range(0, availableSmallUnits.Length);

            if (col == 1) { firstColumn = lastChoice_c; }

			for (int row = 0; row < GridHeight; row++)
			{
                //todo: maybe make more random with RandomNumberGenerator
                //RandomNumberGenerator rng = RandomNumberGenerator.Create();
                //rng.GetBytes(new byte[3]);

                int rng = 0;
                do
                {
                    rng = Random.Range(0, availableSmallUnits.Length);
                } while ((col > 1) ? rng == twoAgoChoice_r || rng == twoAgoChoice_c[row] 
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
        AddToQueue(0, GridHeight - 1, firstColumn);
    }

    void AddToQueue (int topOfReplace, int bottomOfReplace, int[] firsColumn)
	{
		int rng = 0;
		int lastChoice_q = Random.Range(0, availableSmallUnits.Length);
		int twoAgoChoice_q = Random.Range(0, availableSmallUnits.Length);

        for (int i = topOfReplace; i <= bottomOfReplace; i++)
		{
            do
            {
                rng = Random.Range(0, availableSmallUnits.Length);
                //todo: refactor like this
                // have each unit be assigned a number on start. Then check if the unit corresponds to the number
                //can be done to optimize Initialize units as well.
            } while (rng == firsColumn[i] || rng == twoAgoChoice_q);

            GameObject unit = Instantiate(availableSmallUnits[rng], fieldUnits);

            UnitController uc = unit.GetComponent<UnitController>();

            uc.pg = this;
            uc.gameObject.transform.position = rows[i].position;
            Queue[i] = uc;
            //overload move with single parameter for move to queue
            //uc.Move(col, row);

            twoAgoChoice_q = lastChoice_q;
            lastChoice_q = rng;
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
