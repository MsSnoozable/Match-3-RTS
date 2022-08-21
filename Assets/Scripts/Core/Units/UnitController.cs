using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    #region Public Fields

    public UnitData data;
    public bool justMoved = false;
    [HideInInspector] public PlayerGrid pg;
    public GameManager gameManager;
    public int unitIndex; //note: might need later for more optimized randomization.
    #endregion
    
    #region Private Fields

    [HideInInspector] public int xPos = 0;
    [HideInInspector] public int yPos = 0;
    public bool isReadyToMove = true;

    //References
    Animator anim;

    #endregion

    public example ex;

	private void Start()
	{
		
	}

	private void Awake()
	{
        anim = this.GetComponent<Animator>();

        anim.SetBool("isIdle", true);
        gameManager = FindObjectOfType<GameManager>();
    }
    public UnitController Summon(int xDestination, int yDestination)
	{
        return null;
	}

    //todo: maybe pass in a function as a parameter so you callback oncomplete instead of checking the move duration.???
    public UnitController Move(int xDestination, int yDestination)
	{
        //if (isReadyToMove)
        //{
            anim.SetBool("isMove", true);
            //isReadyToMove = false;

            Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
            float xflip = this.transform.localScale.x;

		    //note: not sure if I need this. Makes speed the same even if travel distance is bigger
		    int speedMofifier = 1;
		   
            /*if (xDestination != xPos)
				speedMofifier = Mathf.Abs(xPos - xDestination);
			else if (yDestination != yPos)
				speedMofifier = Mathf.Abs(yPos - yDestination);
			else
			{
				speedMofifier = 1;
				Debug.LogError("you moved diagonally. Tha'ts illegalls"); //todo: weird errors here 
			}
            */

		    if (xDestination < xPos)
            {
                //this.GetComponent<SpriteRenderer>().flipX = true;
                xflip *= -1;
                this.transform.localScale = new Vector2(xflip, transform.localScale.y); //flip when going back

            }
            transform.DOMove(Destination, UnitData.moveDuration * speedMofifier).OnComplete(() =>
            {
                anim.SetBool("isMove", false);

                if (xflip < 0) xflip *= -1;
                //this.GetComponent<SpriteRenderer>().flipX = false; //todo:alternate with sprites later
                this.transform.localScale = new Vector2(xflip, transform.localScale.y);

                //isReadyToMove = true;
            });

            UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
            pg.GridArray[xDestination, yDestination] = this;
            xPos = xDestination;
            yPos = yDestination;
            
            List<UnitController> list = new List<UnitController>();
            //list.Add(secondaryUnit);
            list.Add(this);
            
            if (GameManager.instance.isSetupComplete)
                StartCoroutine(MatchCheck(list));    

            return secondaryUnit;
        //}
        //return null;
    }

    //calls for each moved unit
    //todo: make this static and refactor accordingly?
    public static IEnumerator MatchCheck (List<UnitController> list)
	{
        int absoluteRightMost = PlayerGrid.MinColumn;

        foreach (UnitController checking in list) {
            //bool matchMade = false;
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


			if (shielders.Count >= 3)
            {
                int bottomMost = 0;
                int topMost = PlayerGrid.GridHeight - 1;

                string output = "shield: ";

                foreach (UnitController uc in shielders)
                {
                    output += uc.yPos + ", ";
                    //matchMade = true;

                    if (uc.yPos > bottomMost) bottomMost = uc.yPos;
                    if (uc.yPos < topMost) topMost = uc.yPos;
                }

                checking.StartCoroutine(Shield(shielders));
                yield return new WaitForSeconds(UnitData.moveDuration);

                checking.pg.MoveRowMulti(topMost, bottomMost, shielders[0].xPos + 1, shielders[0].xPos);

                //optimization: refactor this to be less stupid
                print(output);
            }

            if (attackers.Count >= 3)
            {
                int rightMost = PlayerGrid.MinColumn;
                int leftMost = PlayerGrid.GridWidth - 1;

                string output = "attack: ";

                foreach (UnitController uc in attackers)
                {
                    output += uc.xPos + ", ";
                    //matchMade = true;

                    if (uc.xPos < leftMost) leftMost = uc.xPos;
                    if (uc.xPos > rightMost) rightMost = uc.xPos;
                }
                checking.StartCoroutine(Attack(attackers));
                yield return new WaitForSeconds(UnitData.attackFusionDelay + UnitData.moveDuration * 2);

                checking.pg.MoveRow(attackers[0].yPos, leftMost - 1, rightMost - 1); //moves the left side in
                checking.pg.MoveRow(attackers[0].yPos, rightMost + 1, rightMost); //moves the right side back

                print(output);
                if (rightMost > absoluteRightMost) absoluteRightMost = rightMost;
            }

        } //for each end
        
        /*todo: after whole loop foreach ends. move all to total to mostRight/Left/Up/Down? instead.. pg.MoveRow etc.
        add delay for each shield/attack fusion and movement animation*/
    }

    void RemoveUnit (int xPos, int yPos)
	{
        //remove unit then move row in

        //"hide"
        pg.MoveRow(yPos, xPos - 1, xPos);

        Destroy(this.gameObject);
	}
    
    //note: might not need depending on implenmentation
    /*
    void RemoveUnit(int xPos, int topRow, int botRow)
    {
        //remove unit then move row in
        pg.MoveRowMulti(topRow, botRow, xPos - 1, xPos);
    }*/


    //aesthetic: shield animate
    //todo: move to unit actions and do dependcy magic 
    private static IEnumerator Shield (List<UnitController> list)
	{
        //optimization: added a fraction so that the shield move happens after multi row move. It's possible to fix this at some point
        yield return new WaitForSeconds(UnitData.moveDuration + 0.01f); //waits for swap to complete

        /*foreach (UnitController uc in list)
            Destroy(uc.gameObject);*/

        for (int i = 0; i < list.Count; i++) list[i].Move(PlayerGrid.GridWidth - 1, list[i].yPos); //move to front
        yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front
    }

    //aesthetic: split up into different parts and use function events from animations to sync up
    private static IEnumerator Attack (List<UnitController> list)
	{
        yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

        //isReadyToMove = false;

        for (int i = 1; i < list.Count; i++) { list[i].Move(list[0].xPos, list[0].yPos); } //fusion
        yield return new WaitForSeconds(UnitData.moveDuration + UnitData.attackFusionDelay);

        for (int i = 1; i < list.Count; i++) { Destroy(list[i].gameObject); } 
		list[0].Move(PlayerGrid.GridWidth - 1, list[0].yPos); //moves to front
        yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front

        list[0].anim.SetBool("isAttack", true);
        //animation of attack
        //do a unit action?? destroy and move row again
    }
}




[Serializable]
public class example
{
    public float mana;
}