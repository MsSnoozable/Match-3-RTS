using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Security.Cryptography;



//todo: decouple this from all the nonsense so we can assembly def properly
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


    [HideInInspector] public bool matchMakingComplete = false;



    #endregion
	private void OnDestroy()
	{
        GameManager.i.OnDoubleCursorSwap -= DoubleCursorSwap;
        GameManager.i.OnAttackCreated -= AttackCreated;
        GameManager.i.OnAttackFusion -= AttackFuse;
        GameManager.i.OnShieldCreated -= ShieldCreated;
        GameManager.i.OnMoreButtonCursorSwap -= MoreButtonsSwap;

    }

    private void Awake()
	{
		InitSpawnUnits();
        
        GameObject cursor = Instantiate(GameManager.i.GetCursor(this.gameObject.tag), transform);
        cursor.tag = this.tag;
        //optimization: could add pg in cursor here for cleaner observer pattern

        GameManager.i.OnDoubleCursorSwap += DoubleCursorSwap;


        GameManager.i.OnAttackCreated += AttackCreated;
        GameManager.i.OnAttackFusion += AttackFuse;
        GameManager.i.OnMoreButtonCursorSwap += MoreButtonsSwap;
        GameManager.i.OnShieldCreated += ShieldCreated;
    }

    public void DoubleCursorSwap(string playerTag, int xPos, int yPos, Direction currentDirection)
    {
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];
            UnitController secondaryUnit = null;

            if (unit == null)
			{
                //swap failed
                //fire event
                GameManager.i.SwapFailed();
                return;
			}

            //broken: unit move still gets called if unit is good but secondary is null;
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

            if (secondaryUnit == null)
			{
                //swap failed
                //fire event
                GameManager.i.SwapFailed();
                return;
            }

            secondaryUnit.Move(xPos, yPos); //swaps secondary to current pos
            MoveHappened(new List<UnitController> { unit, secondaryUnit });
        }

        
    }

    public void MoreButtonsSwap (string playerTag, int xPos, int yPos, Vector2 swapDirection)
	{
        if (playerTag == this.tag)
        {
            UnitController unit = this.GridArray[xPos, yPos];
            UnitController secondaryUnit = null;

            //todo: if unit/secondary are not swappable : swap failed.

            if (unit == null || !unit.swappable)
            {
                //swap failed
                //fire event
                //todo: event for can't swap eh eh sound and such
                GameManager.i.SwapFailed();
                return;
            }

            Vector2Int moveToSpace;

            //broken: unit move still gets called if unit is good but secondary is null;
            if (swapDirection == Vector2.down && yPos < PlayerGrid.GridHeight - 1)
            {
                moveToSpace = new Vector2Int(xPos, yPos + 1);
            }
            else if (swapDirection == Vector2.right && xPos < PlayerGrid.GridWidth - 1)
            {
                moveToSpace = new Vector2Int(xPos + 1, yPos);
            }
            else if (swapDirection == Vector2.up && yPos > 0)
            {
                moveToSpace = new Vector2Int(xPos, yPos - 1);
            }
            else if (swapDirection == Vector2.left && xPos > PlayerGrid.MinColumn)
            {
                moveToSpace = new Vector2Int(xPos - 1, yPos);
            }
            else
            {
                return; // doesn't run if OOB
            }

            secondaryUnit = GridArray[moveToSpace.x, moveToSpace.y];

            /*if (yPos > 0 &&
                xPos < PlayerGrid.GridWidth - 1 &&
                yPos < PlayerGrid.GridHeight - 1 && 
                xPos > PlayerGrid.MinColumn)
			{
                moveToSpace = swapDirection + new Vector2Int(xPos, yPos);
                secondaryUnit = GridArray[moveToSpace.x, moveToSpace.y];
			}
            else { return; }*/

            //unit.Move(xPos + (int)swapDirection.x, yPos - (int)swapDirection.y);

            if (secondaryUnit == null || !secondaryUnit.swappable)
            {
                //swap failed
                //fire event
                GameManager.i.SwapFailed();
                return;
            }

            secondaryUnit = unit.Move(moveToSpace);

            secondaryUnit.Move(xPos, yPos); //swaps secondary to current pos
            MoveHappened(new List<UnitController> { unit, secondaryUnit });
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

    void MoveRowMulti(int topRow, int bottomRow, int pulledFrom, int destination)
	{
        if (topRow == bottomRow) throw new System.Exception("not a multi row, but called multirow function");

        //print(String.Format("bot: {0}, top: {1}",bottomRow, topRow));

        for (int i = topRow; i <= bottomRow; i++)
		{
            MoveRow(i, pulledFrom, destination);
		}

        //if topRow == 0, bottom row == grid height -1.... then only check front column for matches
	}


    //todo: might split into two types of functions
    public void MoveRow(int row, int pulledFrom, int destination)
    {
        //print(string.Format("r: {0}, pulled: {1}, dest: {2}" , row, pulledFrom, destination));

        List<UnitController> toBeChecked = new List<UnitController>();

        if (pulledFrom > destination && pulledFrom <= PlayerGrid.GridWidth - 1) //pull from right
        {
            //needs to move from left most not right most
            for (int i = pulledFrom; i <= PlayerGrid.GridWidth - 1; i++)
            {
                GridArray[i, row].Move(i - 1, row);

                //todo: only on certain conditions add to toBeChecked
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
                    //note: why is that null and not sending null error?
                    GridArray[pulledFrom - i, row].Move(destination - i, row);
                }
                else //pulling from queue
                {
                    AddToQueue(row);
                    GridArray[0, row].Move(destination - i, row);
                }

                //todo: more specific with which get added
                toBeChecked.Add(GridArray[destination - i, row]);
            }
        }
        else if (pulledFrom == destination) { throw new System.Exception(string.Format("pulledFrom == destination... ????!!! pF: {0}, dest: {1}", pulledFrom, destination)); }

        MoveHappened(toBeChecked);
    }

    void AddToQueue (int row)
	{
        int rng = 0;
        do
        {
            rng = RandomUnitIndex();
        } while (AdjacentMatches(row, rng));

        UnitController uc = CreateUnit(rng, row);
        uc.Move(0, row);
    }

    //qa check: don'i know if this works properly right now. Might be messed up from the artifacting glitch
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
                do rng = RandomUnitIndex();
                while ((col > PlayerGrid.MinColumn) ? rng == twoAgoChoice_r ||
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

    List<UnitController> toBeChecked = new List<UnitController>();

    //todo: toBeChecked of tobechecked but somehow say which directions are valid or not??? like toBeChecked vs toBeChecked only vertically.
    //essentially trying to store which ones in the check algorithm can skip certain parts

    bool wasMoved = false;
    void MoveHappened(List<UnitController> toBeAdded)
    {
        foreach (UnitController item in toBeAdded)
        {
            if (item == null)
            {
                Debug.LogError("problem: item from move happened is null.\n" +
					"wrong call from move(s)");
                continue;
            }
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


        List<UnitController> toBeChecked = new List<UnitController>(passedList);

        foreach (UnitController checking in toBeChecked)
        {
            if (!checking.swappable) continue;
            List<UnitController> attackers = new List<UnitController>() { checking };
            List<UnitController> shielders = new List<UnitController>() { checking };

            #region AddMatchesToList
            int x = checking.xPos;
            int y = checking.yPos;

            UnitController movedUnit = checking.pg.GridArray[x, y];
            UnitController nextUnit;

            for (int left = -1; left - PlayerGrid.MinColumn >= -x; left--)
            {
                nextUnit = checking.pg.GridArray[x + left, y];
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) attackers.Add(nextUnit);
                else break;
            }
            for (int up = -1; up >= -y; up--)
            {
                nextUnit = checking.pg.GridArray[x, y + up];
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) shielders.Add(nextUnit);
                else break;
            }
            for (int right = 1; right <= PlayerGrid.GridWidth - 1 - x; right++)
            {
                nextUnit = checking.pg.GridArray[x + right, y];
                if (movedUnit.data == nextUnit.data && nextUnit.isIdle) attackers.Add(nextUnit);
                else break;
            }

            for (int down = 1; down <= PlayerGrid.GridHeight - 1 - y; down++)
            {
                nextUnit = checking.pg.GridArray[x, y + down];
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
                int bottomMost = 0;
                int topMost = PlayerGrid.GridHeight - 1;

                string output = "shield: ";

                foreach (UnitController uc in shielders)
                {
                    output += uc.yPos + ", ";
                    if (uc.yPos > bottomMost) bottomMost = uc.yPos;
                    if (uc.yPos < topMost) topMost = uc.yPos;
                }

                UnitShieldInfo info = new UnitShieldInfo(shielders[0].xPos, topMost, bottomMost, shielders, this);
                //StartCoroutine(GameManager.i.ShieldCreated(info));
                StartCoroutine(ShieldCreated(info));

                print(output);
            }

            else if (attackers.Count >= 3)
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
                UnitAttackInfo info = new UnitAttackInfo(attackers[0].yPos, leftMost, rightMost, attackers, this);

                StartCoroutine(GameManager.i.AttackCreated(info));
                StartCoroutine(AttackCreated(info));

                //print(output);

                //todo: add this to modify the movment of things after a match
                if (rightMost > absoluteRightMost) absoluteRightMost = rightMost;
            }
            #endregion
        } //for each end

        /*todo: after whole loop foreach ends. move all to total to mostRight/Left/Up/Down? instead.. pg.MoveRow etc.
        add delay for each shield/attack fusion and movement animation*/
    }

    int RandomUnitIndex () => Random.Range(0, availableSmallUnits.Length);

    public void AttackFuse(UnitAttackInfo info)
    {
        MoveRow(info.row, info.leftMost - 1, info.rightMost - 1);
        MoveRow(info.row, info.rightMost + 1, info.rightMost);
    }

    public void AttackMoveToFront(UnitAttackInfo info)
    {

    }

    public void AttackComplete(UnitAttackInfo info)
    {

    }

    //aesthetic: shield animate
    //todo: move to unit actions and do dependcy magic 
    public IEnumerator ShieldCreated(UnitShieldInfo info)
    {
        if (info.pg == this)
        {
            //optimization: added a fraction so that the shield move happens after multi row move. It's possible to fix this at some point
            yield return new WaitForSeconds(UnitData.moveDuration + 0.01f); //waits for swap to complete
            List<UnitController> shielders = new List<UnitController>(info.shielders);

            foreach (UnitController shielder in shielders)
            {
                shielder.swappable = false;
                shielder.isShield = true;
                //info.shielders[i].anim.SetBool("isShield", true);
            }

            //broken: doesn't accomodate swapping and temp values... go back and rework a bunch of stuff
            MoveRowMulti(info.topMost, info.bottomMost, info.col + 1, info.col);

            for (int i = 0; i < info.shielders.Count; i++)
            {
                info.shielders[i].Move(PlayerGrid.GridWidth - 1, info.shielders[i].yPos); //move to front
            }
            yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front

            for (int i = 0; i < info.shielders.Count; i++)
            {
                info.shielders[i].swappable = false;
                //info.shielders[i].anim.SetBool("isShield", true);
            }
        }
    }
    //todo: split up into different parts and use function events from animations to sync up
    public IEnumerator AttackCreated(UnitAttackInfo info)
    {
        if (info.pg == this)
        {
            //attack created
            yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

            List<UnitController> attackers = new List<UnitController>(info.attackers);

            //end

            foreach (UnitController attacker in attackers)
            {
                attacker.swappable = false;
                attacker.isAttack = true;
            }


            //attack fusion
            for (int i = 1; i < attackers.Count; i++) attackers[i].Move(attackers[0].xPos, attackers[0].yPos); //fusion
            yield return new WaitForSeconds(UnitData.moveDuration + UnitData.attackFusionDelay);

            GameManager.i.AttackFusion(info);
            
            //attack fusion
            //todo: works only with Attack info instead of formation info. Either make more functions or learn how delegate casting works
            for (int i = 1; i < attackers.Count; i++)
            {
                attackers[i].Move(attackers[0].xPos, attackers[0].yPos, () => {}); 
                //fusion //todo: currently calls fusion 3 times in a 3 match. Make it call once for the whole match
                //using if (1) then call gm, else don't
            }
            yield return new WaitForSeconds(UnitData.moveDuration + UnitData.attackFusionDelay);

            GameManager.i.AttackFusion(info);
            //end


            //move to front
            for (int i = 1; i < attackers.Count; i++) { Destroy(attackers[i].gameObject); }
            attackers[0].Move(PlayerGrid.GridWidth - 1, attackers[0].yPos); //moves to front
            yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front
                                                                           //end

            //end


            //hold
            //end

            //complete
            //end
            attackers[0].swappable = false;
            //attackers[0].anim.SetBool("isAttack", true);
            //do a unit action?? destroy and move row again
        }
    }

    public void ShieldMoveToFront(UnitShieldInfo info)
    {

    }

}
