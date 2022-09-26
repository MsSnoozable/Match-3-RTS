using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;
using System.Security.Cryptography;



//todo: decouple this from all the nonsense so we can assembly def properly
public partial class PlayerGrid : MonoBehaviour
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


    [HideInInspector] public bool matchMakingComplete = false;



    #endregion
	private void OnDestroy()
	{
        GameManager.i.OnDoubleCursorSwap -= DoubleCursorSwap;
        GameManager.i.OnAttackCreated -= Attack_Created;
        GameManager.i.OnAttackCombine -= Attack_Combine;
        GameManager.i.OnShieldCreated -= Shield_Created;
        GameManager.i.OnMoreButtonCursorSwap -= MoreButtonsSwap;

    }

    private void Awake()
	{
		InitSpawnUnits();
        
        GameObject cursor = Instantiate(GameManager.i.GetCursor(this.gameObject.tag), transform);
        cursor.tag = this.tag;
        //optimization: could add pg in cursor here for cleaner observer pattern

        GameManager.i.OnDoubleCursorSwap += DoubleCursorSwap;

        GameManager.i.OnMoreButtonCursorSwap += MoreButtonsSwap;
        GameManager.i.OnAttackCreated += Attack_Created;
        GameManager.i.OnAttackCombine += Attack_Combine;
        GameManager.i.OnShieldCreated += Shield_Created;
    }

    public void DoubleCursorSwap(string playerTag, int xPos, int yPos, Direction currentDirection)
    {
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];

            if (unit == null)
			{
                //swap failed
                //fire event
                GameManager.i.SwapFailed();
                return;
			}

            switch (currentDirection)
            {
                case Direction.Down:
                    SwapInGridOnly(xPos, yPos, xPos, yPos + 1);
                    break;
                case Direction.Right:
                    SwapInGridOnly(xPos, yPos, xPos + 1, yPos);
                    break;
                case Direction.Up:
                    SwapInGridOnly(xPos, yPos, xPos, yPos - 1);
                    break;
                case Direction.Left:
                    SwapInGridOnly(xPos, yPos, xPos - 1, yPos);
                    break;
            }
        }
    }

    void SwapInGridOnly (UnitController unitToMove, int xDestination, int yDestination)
	{
        SwapInGridOnly(unitToMove.xPos, unitToMove.yPos, xDestination, yDestination);
	}

    List<UnitController> toBeMoved = new List<UnitController>();
    //todo: Refactor all this !!!!
    void SwapInGridOnly(int x1, int y1, int x2, int y2)
    {
        //print(string.Format("x1:{0},y1:{1},x2:{2},y2:{3}", x1, y1, x2, y2));

        if (GridArray[x2, y2] == null || !GridArray[x2, y2].swappable)
        {
            //secondary is null
            //swap failed
            //fire event
            Debug.LogWarning("failed to swap");
            GameManager.i.SwapFailed();
            return;
        }

        //swaps data in unit1 of gridArray to unit2 and viceversa.
        //doesn't actually move yet

        //optimization: the set gridPos is the uggliest thing possible OMG
        UnitController temp = GridArray[x1, y1];
        GridArray[x1, y1] = GridArray[x2, y2];
        GridArray[x2, y2] = temp;

        Vector2Int tempV = GridArray[x1, y1].Pos;
        GridArray[x1, y1].Pos = GridArray[x2, y2].Pos;
        GridArray[x2, y2].Pos = tempV;

        //print(string.Format("({0},{1}),({2},{3})", GridArray[x1, y1].xPos, GridArray[x1, y1].yPos, GridArray[x2, y2].xPos, GridArray[x2, y2].yPos));

        //adds both to list to be moved later
        toBeMoved.Add(GridArray[x1, y1]);
        toBeMoved.Add(GridArray[x2, y2]);

        //print(toBeMoved.Count);


        needsMoveUpdating = true;
    }
    bool needsMoveUpdating = false;

    void SwapInGridOnly(Vector2Int unit1, Vector2Int unit2)
	{
        SwapInGridOnly(unit1.x, unit1.y, unit2.x, unit2.y);
    }

    public void MoreButtonsSwap (string playerTag, int xPos, int yPos, Vector2 swapDirection)
	{
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];

            //todo: if unit/secondary are not swappable : swap failed.
            if (unit == null || !unit.swappable)
            {
                //swap failed
                //fire event
                //todo: event for can't swap eh eh sound and such
                GameManager.i.SwapFailed();
                return;
            }

            if (swapDirection == Vector2.down && yPos < GridHeight - 1)
            {
                SwapInGridOnly(xPos, yPos, xPos, yPos + 1);
            }
            else if (swapDirection == Vector2.right && xPos < GridWidth - 1)
            {
                SwapInGridOnly(xPos, yPos, xPos + 1, yPos);
            }
            else if (swapDirection == Vector2.up && yPos > 0)
            {
                SwapInGridOnly(xPos, yPos, xPos, yPos - 1);
			}
            else if (swapDirection == Vector2.left && xPos > MinColumn)
            {
                SwapInGridOnly(xPos, yPos, xPos - 1, yPos);
            }
            else
            {
                return; // doesn't run if OOB
            }
        }
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
            GameManager.i.isSetupComplete = false;
            StartCoroutine(GameManager.i.RoundStartCountdown());
            InitSpawnUnits();
        }
    }

    public void GridArrayChecker(InputAction.CallbackContext context)
	{
        if (context.performed)
		{
            string output = "all good!";
            bool nullFound = false;
            List<string> coordinates = new List<string>();

            for (int i = 0; i < GridHeight; i++)
            {
                for (int j = 0; j < GridWidth; j++)
			    {
                    if (GridArray[j, i] == null)
                    {
                        nullFound = true;
                        coordinates.Add(string.Format("({0}, {1}) ", i, j));
                    }
                }
			}
            if (nullFound) 
            {
                output = "not good: "; 
                foreach (string c in coordinates)
				{
                    output += c;
				}
            }
            print(output);
		}
	}

    void MoveRowMulti(List<int> rows, int pulledFrom, int destination, Action OnMoveCompleteCallback)
	{
        //print(String.Format("bot: {0}, top: {1}",bottomRow, topRow));

        for (int i = 0; i <= rows.Count - 1; i++)
		{
            MoveRow(rows[i], pulledFrom, destination, OnMoveCompleteCallback);
		}

        //if topRow == 0, bottom row == grid height -1.... then only check front column for matches
	}

    void Internal_MoveRow(int row, int pulledFrom, int destination, Action OnMoveCompleteCallback)
	{
        //print(string.Format("rows: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));


        if (pulledFrom > destination && pulledFrom <= GridWidth - 1) //pull from right
        {
            //needs to move from left most not right most
            for (int i = pulledFrom; i <= GridWidth - 1; i++)
            {
                SwapInGridOnly(i, row, i - 1, row);
                //todo: only on certain conditions add to toBeChecked
            }
        }
        //optimization: super messy can be cleaned. Maybe get rid of "destination"?
        else if (pulledFrom < destination) //pull from left/queue
        {
            for (int i = 0; i < destination + 1; i++)
            {
                if (pulledFrom - i >= 0) //in array bounds
                {
                    SwapInGridOnly(pulledFrom - i, row, destination - i, row);
                }
                else //pulling from queue
                {
                    AddToQueue(row);
                    SwapInGridOnly(0, row, destination - i, row);
                }
                //todo: more specific with which get added
            }
        }
        else if (pulledFrom == destination) { throw new System.Exception(string.Format("pulledFrom == destination... ????!!! pF: {0}, dest: {1}", pulledFrom, destination)); }
    }

    public void MoveRow(int row, int pulledFrom, int destination)
    {
        Internal_MoveRow(row, pulledFrom, destination, null);
    }
    public void MoveRow(int row, int pulledFrom, int destination, Action OnMoveCompleteCallback)
    {
        Internal_MoveRow(row, pulledFrom, destination, OnMoveCompleteCallback);
    }

    void AddToQueue (int row)
	{
        int rng = 0;
        do
        {
            rng = RandomUnitIndex();
        } while (AdjacentMatches(row, rng));

        UnitController uc = CreateUnit(rng, row);
        uc.AnimationMove(0, row);
    }

    //qa check: don'i know if this works properly right now. Might be messed up from the artifacting glitch
    bool AdjacentMatches (int row, int attemptedIndex)
	{
        int topRow = Mathf.Clamp(row - 2, 0, GridHeight - 1);
        int bottomRow = Mathf.Clamp(row + 2, 0, GridHeight - 1);

        int verticalMatches = 0;
        for (int i = row; i > topRow; i--)
		{
            if (GridArray[0, i] == null) continue;
            if (GridArray[0, i].data.color.Equals(attemptedIndex))
                verticalMatches++; //check top
		}

        for (int i = row; i < bottomRow; i++)
		{
            if (GridArray[0, i] == null) continue;
            if (GridArray[0, i].data.color.Equals(attemptedIndex))
                verticalMatches++; //check down
        }

        if (verticalMatches >= 3) return true;

        int rightMatches = 0;
        for (int i = 0; i < 2; i++)
        {
            if (GridArray[i, row] == null) continue;
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
                do rng = RandomUnitIndex();
                while ((col > MinColumn) ? rng == twoAgoChoice_r ||
                        rng == twoAgoChoice_c[row] : rng == twoAgoChoice_r);

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

    //todo: toBeChecked of tobechecked but somehow say which directions are valid or not??? like toBeChecked vs toBeChecked only vertically.
    //essentially trying to store which ones in the check algorithm can skip certain parts

    private void LateUpdate()
    {
        if (needsMoveUpdating)
		{
            foreach (UnitController unit in toBeMoved)
			{
                unit.Move();
			}
            MatchCheck(toBeMoved);
            needsMoveUpdating = false;
            toBeMoved.Clear();
		}
    }

    //calls for each moved unit
    //todo: make this on GM or PG? so matches are handled outside of here
    public void MatchCheck(List<UnitController> passedList)
    {
        int absoluteRightMost = MinColumn;

        List<UnitController> toBeChecked = new List<UnitController>(passedList);

        foreach (UnitController checking in toBeChecked)
        {
            if (!checking.swappable) continue;
            List<UnitController> attackers = new List<UnitController>() { checking };
            List<UnitController> shielders = new List<UnitController>() { checking };

            #region AddMatchesToList
            int x = checking.xPos;
            int y = checking.yPos;

            UnitController movedUnit = GridArray[x, y];
            UnitController nextUnit;

            for (int left = -1; left - MinColumn >= -x; left--)
            {
                nextUnit = GridArray[x + left, y];
                if (nextUnit == null) break;
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) attackers.Add(nextUnit);
                else break;
            }
            for (int up = -1; up >= -y; up--)
            {
                nextUnit = GridArray[x, y + up];
                if (nextUnit == null) break;
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) shielders.Add(nextUnit);
                else break;
            }
            for (int right = 1; right <= GridWidth - 1 - x; right++)
            {
                nextUnit = GridArray[x + right, y];
                if (nextUnit == null) break;
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) attackers.Add(nextUnit);
                else break;
            }

            for (int down = 1; down <= GridHeight - 1 - y; down++)
            {
                nextUnit = GridArray[x, y + down];
                if (nextUnit == null) break;
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) shielders.Add(nextUnit);
                else break;
                
            }
            #endregion

            #region Shield/Attack
            if (shielders.Count >= 3 && attackers.Count >= 3)
			{
                print("double");
			}

            else if (shielders.Count >= 3)
            {
                string output = "shield: ";

                List<int> rows = new List<int>();

                foreach (UnitController uc in shielders)
                {
                    output += uc.yPos + ", ";

                    rows.Add(uc.yPos);
                }

                UnitShieldInfo info = new UnitShieldInfo(shielders[0].xPos, rows, shielders, this);
                //StartCoroutine(GameManager.i.Shield_Created(info));
                StartCoroutine(Shield_Created(info));
                

                print(output);
            }

            else if (attackers.Count >= 3)
            {
                int rightMost = MinColumn;
                int leftMost = GridWidth - 1;

                string output = "attack: ";

                foreach (UnitController uc in attackers)
                {
                    output += uc.xPos + ", ";

                    if (uc.xPos < leftMost) leftMost = uc.xPos;
                    if (uc.xPos > rightMost) rightMost = uc.xPos;
                }
                UnitAttackInfo info = new UnitAttackInfo(attackers[0].yPos, leftMost, rightMost, attackers, this);

                //StartCoroutine(GameManager.i.AttackCreated(info)); //broken: not being called
                StartCoroutine(Attack_Created(info));

                print(output);

                //todo: add this to modify the movment of things after a match
                if (rightMost > absoluteRightMost) absoluteRightMost = rightMost;
            }
            #endregion
        } //for each end
        /*todo: after whole loop foreach ends. move all to total to mostRight/Left/Up/Down? instead.. pg.MoveRow etc.
        add delay for each shield/attack fusion and movement animation*/
    }
    int RandomUnitIndex () => UnityEngine.Random.Range(0, availableSmallUnits.Length);


    //aesthetic: shield animate
    //todo: move to unit actions and do dependcy magic 
    public IEnumerator Shield_Created(UnitShieldInfo info)
    {
        if (info.pg == this)
        {
            List<UnitController> shielders = new List<UnitController>(info.shielders);
            yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

            foreach (UnitController shielder in shielders)
            {
                shielder.swappable = false;
                shielder.isShield = true;
                //creates shield event

                var (doesShieldExist, col) = Shield_ExistCheck(shielder.yPos, shielder.xPos);
                if (doesShieldExist)
                {
                    print("shield exists");
                    Shield_GridRearrange(info, true);  //Grid after fuse 
                    shielder.AnimationMove(col, shielder.yPos, () =>
					{
                        //"shield fuse event"
                        shielder.RemoveFromGrid();
                        Destroy(shielder.gameObject);
					});
                }
                else
				{
                    print("shield no thereldkdofkj");

                    //Shield_GridRearrange(info, false); //Grid after create

                    MoveToFront(shielder.xPos, shielder.yPos);
                    
                    //todo: make "SwapInGridOnly" have callbacks
                    //shielder.AnimationMove(GridWidth - 1, shielder.yPos,
                    /*() =>
                    {
                        //"shield create event"
                        
                        //Destroy(shielder.gameObject);

                        //todo: array for multirow instead of extremes
                    });*/
                }
            }
        }
    }
    void MoveToFront (int start_x, int start_y)
	{
        for (int i = GridWidth - 1; i > start_x; i--)
            SwapInGridOnly(start_x, start_y, i, start_y);
    }
	void Shield_GridRearrange(UnitShieldInfo info, bool fromFuse)
	{
        int colModifier = fromFuse ? -1 : 1; //if from a fuse pulls left else pull right

        MoveRowMulti(info.rows, info.col + colModifier, info.col, () =>
        {
            //shield "hold" event

        }); //moves inward from shield
        //Shield_MoveToFront(info);

    }

    (bool doesShieldExist, int col) Shield_ExistCheck(int row, int addedShieldCol)
    {
        //print("col:" + addedShieldCol);
        for (int i = GridWidth - 1; i >= MinColumn; i--)
        {
            UnitController checking = GridArray[i, row];
            if (checking == null) continue;
            //print(string.Format("i:{0}, isShield:{1}", i, checking.isShield));
            if (checking.isShield && i != addedShieldCol)
            {
                return (true, i);
            }
        }
        return (false, 0);
    }

    void Attack_Combine (UnitAttackInfo info)
	{
        List<UnitController> attackers = new List<UnitController>(info.attackers);
 
        for (int i = 1; i < attackers.Count; i++)
		{
            attackers[i].AnimationMove(attackers[0].xPos, attackers[0].yPos, () =>
			{
                for (int i = 1; i < attackers.Count; i++) 
                {
                    //deallocates
                    attackers[i].RemoveFromGrid();

                    Destroy(attackers[i].gameObject); 
                } //after combine

                var (doesAttackExist, col) = Attack_ExistCheck(info.row, info.attackers[0].xPos, info.attackers[0].data);
                if (doesAttackExist) //fuse attack
                {
                    print("attack exists");

                    //fuse to existing attack and move grid accordingly
                    //FuseToExistingAttack(info.attackers[0], addedShieldCol);
                    UnitController addedAttack = info.attackers[0];
                    addedAttack.AnimationMove(col, addedAttack.yPos, () => {
                        //"fuse event"
                        addedAttack.RemoveFromGrid();

                        Destroy(addedAttack.gameObject);
                    });
                    Attack_GridRearrange(info, true);
                }
                else //create attack
                {
                    print("attack no exists");

                    attackers[0].AnimationMove(GridWidth - 1, attackers[0].yPos, () => //moves to front
                    {
                        MoveToFront(attackers[0].xPos, attackers[0].yPos);
                        //PullRow from left

						//hold event
						attackers[0].isAttack = true;
                    });
                    //Attack_GridRearrange(info, false);
                }
            });
		}


    }

    //todo: split up into different parts and use function events from animations to sync up
    public IEnumerator Attack_Created(UnitAttackInfo info)
    {
        if (info.pg == this)
        {
            List<UnitController> attackers = new List<UnitController>(info.attackers);
            foreach (UnitController attacker in attackers)
            {
                attacker.swappable = false;
            }
            yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

            Attack_Combine(info);
            //GameManager.i.AttackCombine(info);
            
            //complete
            //end
            //attackers[0].anim.SetBool("isAttack", true);
            //do a unit action?? destroy and move row again
        }
    }
    public void Attack_GridRearrange(UnitAttackInfo info, bool fromFuse)
    {
        if (fromFuse)
        {
            MoveRow(info.row, info.leftMost - 1, info.rightMost); //move left in 3
        }
        else
        {
            MoveRow(info.row, info.leftMost - 1, info.rightMost - 1); //move left in 2
            MoveRow(info.row, info.rightMost + 1, info.rightMost); //moves right in 1
        }
    }

    (bool doesAttackExist, int col) Attack_ExistCheck (int row, int addedAttackCol, UnitData data)
	{
        for (int i = GridWidth - 1; i >= MinColumn; i--)
		{
            UnitController checking = GridArray[i, row];
            if (checking == null) continue;
            if (checking.data == data && checking.isAttack && i != addedAttackCol)
			{
                return (true, i);
			}
		}
        return (false, 0);
	}
}
