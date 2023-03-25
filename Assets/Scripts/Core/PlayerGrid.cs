using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//todo: decouple this from all the nonsense so we can assembly def properly
public partial class PlayerGrid : MonoBehaviour
{
    #region Public Fields

    public Transform[] rows = new Transform[GridHeight];
    public Transform[] cols = new Transform[GridWidth];
    [HideInInspector] public const int GridWidth = 8;
    [HideInInspector] public const int GridHeight = 7;
    [HideInInspector] public const int MinColumn = 1;

    [HideInInspector] public bool matchMakingComplete = false;
    public UnitController[,] GridArray = new UnitController[GridWidth, GridHeight];
    [SerializeField] GameObject[] availableSmallUnits = new GameObject[5];
    public bool inCombo = false; //true when stuff is still currently moving. Certain functions dont go until this is true

    #endregion

    #region Private Fields

    bool needsMoveUpdating = false;
    [HideInInspector] List<UnitController> toBeMoved = new List<UnitController>();

    [SerializeField] int MinShieldRequired = 3;
    [SerializeField] int MinAttackRequired = 3;
    float defaultStartingShieldTime = 12f;
	
    int comboCount = 0;
    //A grid is considered neutral if nothing is moving at the time
    public bool isNeutral = true;
	#endregion

	#region Main
	void Start()
	{
		InitSpawnUnits();
        
        GameObject cursor = Instantiate(GameManager._.GetCursor(this.gameObject.tag), transform);
        cursor.tag = this.tag;
        //optimization: could add pg in cursor here for cleaner observer pattern
        GameManager._.OnSwapFailed += SwapFailed;

        GameManager._.OnAttackCreated += StartAttackSequence;
        GameManager._.OnAttackCombine += Attack_Combine;
        GameManager._.OnAttackHoldStart += Attack_HoldStart;
        GameManager._.OnAttackFuse += Attack_Fusion;

        GameManager._.OnShieldCreated += StartShieldSequence;
        GameManager._.OnShieldHoldStart += Shield_HoldStart;
        GameManager._.OnShieldFusion += Shield_Fusion;

        GameManager._.OnDoubleCursorSwap += DoubleCursorSwap;
        GameManager._.OnMoreButtonCursorSwap += MoreButtonsSwap;
    }
    private void OnDestroy()
	{
        GameManager._.OnSwapFailed -= SwapFailed;

        GameManager._.OnAttackCreated -= StartAttackSequence;
        GameManager._.OnAttackCombine -= Attack_Combine;
        GameManager._.OnAttackHoldStart -= Attack_HoldStart;
        GameManager._.OnAttackFuse -= Attack_Fusion;

        GameManager._.OnShieldCreated -= StartShieldSequence;
        GameManager._.OnShieldHoldStart -= Shield_HoldStart;
        GameManager._.OnShieldFusion -= Shield_Fusion;

        GameManager._.OnDoubleCursorSwap -= DoubleCursorSwap;
        GameManager._.OnMoreButtonCursorSwap -= MoreButtonsSwap;
    }
    void SwapFailed ()
	{
        Debug.LogWarning("not swapped");
	}
	//todo: toBeChecked of tobechecked but somehow say which directions are valid or not??? like toBeChecked vs toBeChecked only vertically.
	//essentially trying to store which ones in the check algorithm can skip certain parts
	void Update()
    {
        if (needsMoveUpdating)
		{
            List<UnitController> toBeChecked = new List<UnitController>();
            foreach (UnitController unit in toBeMoved)
			{
                if (unit != null)
                {
                    unit.Move();
                    //todo: have more elaborate ways to NOT add to be checked
                    //for now just have one exception for if in queue
                    if (unit.xPos != 0)
                        toBeChecked.Add(unit);
                }
			}
            MatchCheck(toBeChecked);
            needsMoveUpdating = false;
            toBeMoved.Clear();
		}

        if (bufferQueued)
		{
            if (GridArray[bufferXPos, bufferYPos].swappable &&
                GridArray[bufferX2Pos, bufferY2Pos].swappable)
			{
                MoveInGridOnly(bufferXPos, bufferYPos, bufferX2Pos, bufferY2Pos);
                bufferQueued = false;
			}
		}
    }

	//todo: make this on GM or PG? so matches are handled outside of here
	//broken: this is comboing an unbelievable amount right now.... maybe design issue not engineering?
	void MatchCheck(List<UnitController> passedList)
    {
        List<UnitController> toBeChecked = new List<UnitController>(passedList);

        int numOfMatches = 0;

        foreach (UnitController checking in toBeChecked)
        {
            if (!checking.isIdle) continue;
            List<UnitController> attackers = new List<UnitController>() { checking };
            List<UnitController> shielders = new List<UnitController>() { checking };

            #region AddMatchesToList
            int x = checking.xPos;
            int y = checking.yPos;

            UnitController movedUnit = GridArray[x, y];
            if (movedUnit == null) Debug.LogWarning("movedUnit null"); 
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

			

            if (shielders.Count >= MinShieldRequired && attackers.Count >= MinAttackRequired)
			{
				UnitController extraUnit = CreateUnit(shielders[0].unitIndex, shielders[0].yPos);
                extraUnit.Pos = shielders[0].Pos; //THIS IS THE PROBLEM. GRID ISN'T UPDATED WITH THIS PROPERLY
                extraUnit.transform.localPosition = shielders[0].transform.localPosition;
                shielders[0] = extraUnit; //remove doubled unit from shielders and add extra to shielders
                //broken: the extraUnit doesn't change how the "move row" works....
            }
            if (shielders.Count >= MinShieldRequired)
            {
                numOfMatches++;
                UnitShieldInfo info = new UnitShieldInfo(shielders, this);
                GameManager._.ShieldCreated(info);
            }

            if (attackers.Count >= MinAttackRequired)
            {
                numOfMatches++;
                UnitAttackInfo info = new UnitAttackInfo(attackers[0].yPos, attackers, this);
                GameManager._.AttackCreated(info);
            }
            #endregion
        } //for each end

        //if ANY are true. Make in combo true here.
        inCombo = numOfMatches > 0;
        if (inCombo)
		{
            comboCount++;
            GameManager._.UpdateComboCount(comboCount, this.tag);
		}
        else
		{
            if (comboCount != 0) // if the combo is being reset
            {
                GameManager._.FinalizeComboCount(comboCount);
                comboCount = 0;
            }
		}
        

        /*todo: after whole loop foreach ends. move all to total to mostRight/Left/Up/Down? instead.. pg.MoveRow etc.
        add delay for each shield/attack fusion and movement animation*/
    }
	#endregion

	#region Inputs
	public void ResetBoard(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DOTween.KillAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void TesterFunction(InputAction.CallbackContext context)
	{
        if (context.performed)
		{
            MoveToFront(2, 3, false);
		}
	}

    public void GridArrayChecker(InputAction.CallbackContext context)
	{
        if (context.performed)
		{
            string output = "all good!";
            bool nullFound = false;
            List<string> coordinates = new List<string>();

            for (int i = 0; i < GridWidth; i++)
            {
                for (int j = 0; j < GridHeight; j++)
			    {
                    if (GridArray[i, j] == null)
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

    public void DoubleCursorSwap(string playerTag, int xPos, int yPos, Direction currentDirection)
    {
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];

            if (unit == null)
			{
                //swap failed
                //fire event
                GameManager._.SwapFailed();
                //return;
			}

            switch (currentDirection)
            {
                case Direction.Down:
                    MoveInGridOnly(xPos, yPos, xPos, yPos + 1);
                    break;
                case Direction.Right:
                    MoveInGridOnly(xPos, yPos, xPos + 1, yPos);
                    break;
                case Direction.Up:
                    MoveInGridOnly(xPos, yPos, xPos, yPos - 1);
                    break;
                case Direction.Left:
                    MoveInGridOnly(xPos, yPos, xPos - 1, yPos);
                    break;
            }
        }
    }

    //allows for buffering the next swap
    int bufferXPos;
    int bufferYPos;
    int bufferX2Pos;
    int bufferY2Pos;
    bool bufferQueued;

    public void MoreButtonsSwap (string playerTag, int xPos, int yPos, Vector2 swapDirection)
	{
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];

            //OOB swap failed
			if ((swapDirection == Vector2.down && yPos >= GridHeight - 1) ||
                (swapDirection == Vector2.right && xPos >= GridWidth - 1) ||
                (swapDirection == Vector2.up && yPos <= 0) ||
                (swapDirection == Vector2.left && xPos <= MinColumn))
			{
                GameManager._.SwapFailed();
                return;
            }

            //optimization: I could configure x2, y2 from the cursor side and just pass those in directly instead
            int x2 = xPos + (int)swapDirection.x;
            int y2 = yPos - (int)swapDirection.y;

            UnitController secondaryUnit = GridArray[x2, y2];

            if (unit.locked || secondaryUnit.locked)
            {
                //todo: event for can't swap eh eh sound and such
                GameManager._.SwapFailed();
                return;
            }

            if (!(secondaryUnit.swappable && unit.swappable)/*|| !secondaryUnit.movable*//*|| !unit.movable*/)
            {
                bufferXPos = xPos;
                bufferYPos = yPos;
                bufferX2Pos = x2;
                bufferY2Pos = y2;
                bufferQueued = true;
                return;
            }

            
            MoveInGridOnly(xPos, yPos, x2, y2);
        }
    }

	#endregion

	#region Moves
	public void MoveInGridOnly (UnitController unitToMove, int xDestination, int yDestination)
	{
        MoveInGridOnly(unitToMove.xPos, unitToMove.yPos, xDestination, yDestination);
	}
    public void MoveInGridOnly(Vector2Int unit1, Vector2Int unit2)
	{
        MoveInGridOnly(unit1.x, unit1.y, unit2.x, unit2.y);
    }
    public void MoveInGridOnly(int x1, int x2, int row)
	{
        MoveInGridOnly(x1, row, x2, row);
	}

    //todo: Refactor all this !!!!
    //optimization: make it have a (row, x1, x2) so that move row is cleaner as a call.
    public void MoveInGridOnly(int x1, int y1, int x2, int y2)
    {
        //need to learn: Use pointers and unsafe block to make this section more readable
        /*UnitController* unit1 = GridArray[x1, y1];
        UnitController* unit2 = GridArray[x2, y2];*/

        //swaps data in unit1 of gridArray to unit2 and viceversa.
        //doesn't actually move until lateUpdate
        if (GridArray[x1, y1] == null && GridArray[x2, y2] == null)
		{
            //Debug.LogWarning("both things to swap are null");
            return;
		}
        //todo: have call backs and a class to memorize
        /*if (!unit1.movable || !unit2.movable)
		{
            Debug.LogWarning("new error. immobile stuff attemted move");
            return;
		}*/
        
        //swaps the references in GridArray
        UnitController temp = GridArray[x1, y1];
        GridArray[x1, y1] = GridArray[x2, y2];
        GridArray[x2, y2] = temp;

        //Swaps the positions on the actual unit
        if (GridArray[x1, y1] != null)
            GridArray[x1, y1].Pos = new Vector2Int(x1, y1);
        if (GridArray[x2, y2] != null)
            GridArray[x2, y2].Pos = new Vector2Int(x2, y2);


        //optimization: I could change the "move to front" and other algorithms to just not allow dupes, but this is easier
        //also it might be slightly more efficient to have an XPos and a target x for each UC than this check would
        //just be for if the target and current pos are the same anymore. and the actual pos doesn't update till the move
        //is completed.
        if (GridArray[x1, y1] != null && !GridArray[x1, y1].queuedToBeMoved)
		{
            toBeMoved.Add(GridArray[x1, y1]);
            GridArray[x1, y1].queuedToBeMoved = true;

        }
        //need to learn: how do I rewrite this with null propagation??? the... "?." thing
        if (GridArray[x2, y2] != null  && !GridArray[x2, y2].queuedToBeMoved)
		{
            toBeMoved.Add(GridArray[x2, y2]);
            GridArray[x2, y2].queuedToBeMoved = true;

        }

        needsMoveUpdating = true;
    }

    public void MoveRow (int row, int pulledFrom, int destination)
	{
        MoveRow(row, pulledFrom, destination, false);
    }
    void MoveRow(int row, int pulledFrom, int destination, bool fromAttackCombine)
	{
        if (pulledFrom < destination) //pull from left/queue
        {
            //optimization: could be cleaned here
            for (int i = 0; i <= destination; i++)
            {
                int currentlyPulling = pulledFrom - i;
                int currentDestination = destination - i;

                if (currentlyPulling < 0) //from queue 
				{
                    AddToGridFromOffScreen(row, currentlyPulling, currentDestination, fromAttackCombine);
                    continue;
				}

                if (fromAttackCombine)
				{
                    //optimization: could be slightly better if encapsulated in the Move func of UC
                    UnitController movingUnit = GridArray[currentlyPulling, row];
                    movingUnit.AnimationMove(currentDestination, row);
                    movingUnit.Pos = new Vector2Int(currentDestination, row);
                    GridArray[currentDestination, row] = movingUnit;
				}
                else
				{
                    //todo: more specific with which get checked in matches
                    MoveInGridOnly(currentlyPulling, currentDestination, row);
				}

            }
        }
        else { throw new System.Exception(string.Format("pulledFrom >= destination... ????!!! pF: {0}, dest: {1}", pulledFrom, destination)); }
    }
    void MoveToFront (int start_x, int start_y, bool fromShield)
	{
        int frontCol = GridWidth - 1;
        if (!fromShield)
        {
            //moves nonshields behind any existing shields
            var check = Shield_ExistCheck(start_y, -1); //pass in -1 because no shield was made
            if (check.doesShieldExist)
                frontCol = check.col - 1;
        }

        for (int i = frontCol; i > start_x; i--)
            MoveInGridOnly(start_x, start_y, i, start_y);
    }

	#endregion

    //todo: maybe pass in a function so that shield and attack do their hold after they've finished the swap
    //might be very complex to restructure
    #region Helper functions
	void AddToGridFromOffScreen(int row, int distanceFromStart, int destination, bool fromAttackCombine)
    {
        int rng = 0;
        do
        {
            rng = RandomUnitIndex();
        } while (OffScreenMatchNegation(row, destination, rng));

        UnitController uc = CreateUnit(rng, row);
        GridArray[destination, row] = uc;
        uc.Pos = new Vector2Int(destination, row);
        uc.transform.localPosition = new Vector2(distanceFromStart + 1, 0);

        if (fromAttackCombine)
            uc.AnimationMove(destination, row);
        else
            toBeMoved.Add(uc);
    }

    //qa check: don'_ know if this works properly right now. Might be messed up from the artifacting glitch
    bool OffScreenMatchNegation (int row, int destination, int attemptedIndex)
	{
        int topRow = Mathf.Clamp(row - (MinShieldRequired - 1), 0, GridHeight - 1);
        int bottomRow = Mathf.Clamp(row + (MinShieldRequired - 1), 0, GridHeight - 1);

        int verticalMatches = 0;
        for (int i = row - 1; i > topRow; i--) //check above
        {
            if (GridArray[destination, i] == null) continue;
            if (GridArray[destination, i].data.color.Equals(attemptedIndex))
                verticalMatches++; 
		}

        for (int i = row + 1; i < bottomRow; i++) //check below
        {
            if (GridArray[destination, i] == null) continue;
            if (GridArray[destination, i].data.color.Equals(attemptedIndex))
                verticalMatches++; 
        }

        if (verticalMatches >= MinShieldRequired) return true;

        int rightMatches = 0;
        for (int i = destination; i < MinAttackRequired; i++) //check right
        {
            if (GridArray[i, row] == null) continue;
            if (GridArray[i, row].data.color.Equals(attemptedIndex))
                rightMatches++; 
        }

        if (rightMatches >= MinAttackRequired) return true;

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
    int RandomUnitIndex () => UnityEngine.Random.Range(0, availableSmallUnits.Length);

    UnitController CreateUnit (int unitIndex, int row)
	{
        GameObject unit = Instantiate(availableSmallUnits[unitIndex], rows[row]);
        UnitController uc = unit.GetComponent<UnitController>();
        uc.pg = this;
        uc.unitIndex = unitIndex;  

        return uc;
    }
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;

        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                if (GridArray[i, j] == null)
                {
                    Vector2 pos = new Vector2(cols[i].transform.position.x, rows[j].transform.position.y);
                    Gizmos.DrawCube(pos, Vector2.one);
                }
            }
        }

	}
	#endregion

	#region Shield Creation

    public void StartShieldSequence (UnitShieldInfo info)
	{
        if (info.pg == this)
        {
            StartCoroutine(ShieldSequence(info));
        }
	}

    float pauseTime = 1f;

	//todo: move to unit actions and do dependcy magic 
	IEnumerator ShieldSequence(UnitShieldInfo info)
    {
        if (info.pg == this)
        {
            List<UnitController> shielders = new List<UnitController>(info.shielders);
            if (info.fromSwap)
                yield return new WaitForSeconds(GameManager._.unitMoveSpeed); //waits for swap to complete

            foreach (UnitController shielder in shielders)
            {
                var (doesShieldExist, col) = Shield_ExistCheck(shielder.yPos, shielder.xPos);
                info.existingShieldCol = col;

                info.currentShield = shielder;
                if (doesShieldExist) // shield exists and is fusing to it
                {
                    //todo: pull from left for only cols that apply
                    //also make sure to only call this once when pulling from left
                    GameManager._.ShieldFusion(info);
                }
                else // no shield in the row. Create a shield
				{
                    GameManager._.ShieldHoldStart(info);
                }
            }
        }
    }

    void Shield_Fusion(UnitShieldInfo info)
    {
        if (info.pg == this) { 
            int col = info.existingShieldCol;
            UnitController shielder = info.currentShield;
            shielder.isShield = true;


            shielder.AnimationMove(col, shielder.yPos, () =>
            {
                GridArray[col, shielder.yPos].remainingShieldTime -= defaultStartingShieldTime;
                //"shield fuse event"

                Destroy(shielder.gameObject);

            });
            shielder.RemoveFromGrid();
            MoveRow(shielder.yPos, shielder.xPos - 1, shielder.xPos); //move remaining in
        }
    }

    void Shield_HoldStart (UnitShieldInfo info)
    {
        if (info.pg == this)
        {
            UnitController shielder = info.currentShield;

            MoveToFront(shielder.xPos, shielder.yPos, true);
            //todo: make the event call as a callback after movetoFront. Don't know setup for it yet
            shielder.remainingShieldTime = defaultStartingShieldTime;

            shielder.isShield = true;
        }
    }

    (bool doesShieldExist, int col) Shield_ExistCheck(int row, int addedShieldCol)
    {
        for (int i = GridWidth - 1; i >= MinColumn; i--)
        {
            UnitController checking = GridArray[i, row];
            if (checking == null) continue;
            if (checking.isShield && i != addedShieldCol)
                return (true, i);
        }
        return (false, 0);
    }
    #endregion

	#region Attack Creation
	public void StartAttackSequence(UnitAttackInfo info)
    {
        if (info.pg.tag == this.tag)
        {
            StartCoroutine(AttackSequence(info));
        }
    }

    IEnumerator AttackSequence(UnitAttackInfo info)
	{
        if (info.pg == this)
        {
            List<UnitController> attackers = new List<UnitController>(info.attackers);
            foreach (UnitController attacker in attackers)
            {
                attacker.locked = true;
            }

            if (info.fromSwap)
                yield return new WaitForSeconds(GameManager._.unitMoveSpeed); //waits for swap to complete
            GameManager._.AttackCombine(info);
            //do a unit action?? destroy and move row again
        }
    }

    //broken: if the front unit is removed at the right time then the combine gets interupted before completing
    //and then a bunch pile up in queue and errors fly out
    void Attack_Combine (UnitAttackInfo info)
	{
        if (info.pg == this)
        {
            List<UnitController> attackers = new List<UnitController>(info.attackers);

            for (int i = 0; i < attackers.Count; i++)
            {
                attackers[i].locked = true;

                //optimization: maybe move deallocates here???? Not sure if that helps any.
                attackers[i].AnimationMove(attackers[info.rightMostIndex].xPos, attackers[info.rightMostIndex].yPos, i, (i) =>
                {
                    if (attackers[i] != null && i != info.rightMostIndex)
                    {
                        //attackers[i].RemoveFromGrid(); //deallocates
                        Destroy(attackers[i].gameObject); //actually destroyed
                    }
                    if (i == info.rightMostIndex)
                    {
                        var (doesAttackExist, col) = Attack_ExistCheck(info.row, info.attackers[0].xPos, info.attackers[0].data);
                        info.existingAttackCol = col;

                        if (doesAttackExist) //fuse attack
                        {
                            GameManager._.AttackFuse(info);
                        }
                        else //new attack
                        {
                            GameManager._.AttackHoldStart(info);
                        }

                    }
                });
            } //for end
            MoveRow(info.row, info.leftMost - 1, info.rightMostPosition - 1, true); //move left in 2
        }
    }  

    IEnumerator Attack_Release (UnitAttackInfo info)
	{
        float time = 5f;
        yield return new WaitForSeconds(time);
	}

    //fuse to existing attack and move grid accordingly
    void Attack_Fusion (UnitAttackInfo info) 
	{
        if (info.pg == this)
        {
            UnitController addedAttack = info.attackers[0];
            int col = info.existingAttackCol;

            addedAttack.AnimationMove(col, addedAttack.yPos, () =>
            {
                addedAttack.isAttack = true;
                Destroy(addedAttack.gameObject);
            });
            addedAttack.RemoveFromGrid();
            MoveRow(info.row, info.leftMost - 1, info.rightMostPosition); //move left in 3
        }
        //todo: add my attack value to existing attack
    }

    void Attack_HoldStart (UnitAttackInfo info)
	{
        if (info.pg == this)
        {
            UnitController addedAttack = info.attackers[info.rightMostIndex];
            MoveToFront(addedAttack.xPos, addedAttack.yPos, false);
            addedAttack.isAttack = true;

            StartCoroutine(Attack_Release(info));
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
	#endregion
}
