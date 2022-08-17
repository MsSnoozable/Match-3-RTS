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

            return secondaryUnit;
        //}
        //return null;
    }

    //calls for each moved unit
    public IEnumerator MatchCheck (List<UnitController> list)
	{
        int mostRightRemoved = 0, mostLeftRemoved = 0;

        foreach (UnitController checking in list) {
            //bool matchMade = false;
            List<UnitController> attackers = new List<UnitController>() { checking };
            List<UnitController> shielders = new List<UnitController>() { checking };

            var x = checking.xPos;
            var y = checking.yPos;

            #region AddMatchesToList
            UnitController movedUnit = pg.GridArray[x, y];
            UnitController nextUnit;

		    for (int left = -1; left >= -xPos; left--)
            {
                if (x + left >= PlayerGrid.MinColumn) //in bounds
                {
                    nextUnit = pg.GridArray[x + left, y];
                    if (movedUnit.data == nextUnit.data) attackers.Add(nextUnit);
                    else break;
                }
            }
            for (int up = -1; up >= -yPos; up--)
            {
                if (y + up >= 0) //in bounds
                {
                    nextUnit = pg.GridArray[x, y + up];
                    if (movedUnit.data == nextUnit.data) shielders.Add(nextUnit);
                    else break;
                }
            }
            for (int right = 1; right <= PlayerGrid.GridWidth - 1 - xPos; right++)
		    {
                if (x + right < PlayerGrid.GridWidth)
                {
                    nextUnit = pg.GridArray[x + right, y];
                    if (movedUnit.data == nextUnit.data) attackers.Add(nextUnit);
                    else break;
                }
            }
 
            for (int down = 1; down <= PlayerGrid.GridHeight - 1 - yPos; down++)
            {
                if (y + down < PlayerGrid.GridHeight)
                {
                    nextUnit = pg.GridArray[x, y + down];
                    if (movedUnit.data == nextUnit.data) shielders.Add(nextUnit);
                    else break;
                }
            }
			#endregion

			#region Debugging
			string words = "shields: ";
                foreach (UnitController s in shielders)
                {
                    words += String.Format("({0},{1})", s.xPos, s.yPos);
                }

                words = "attacks: ";
                foreach (UnitController a in attackers)
                {
                    words += String.Format("({0},{1})", a.xPos, a.yPos);
                }
			#endregion

			if (shielders.Count >= 3)
            {
                int bottomRemoved = 0;
                int topRemoved = PlayerGrid.GridHeight - 1;

                string output = "shield: ";

                foreach (UnitController uc in shielders)
                {
                    StartCoroutine(Shield(shielders));
                    output += uc.yPos + ", ";
                    //matchMade = true;

                    if (uc.yPos > bottomRemoved) bottomRemoved = uc.yPos;
                    if (uc.yPos < topRemoved) topRemoved = uc.yPos;
                }
                //optimization: refactor this to be less stupid
                print(output);
                print(String.Format("bot: {0}, top: {1}",bottomRemoved, topRemoved));
                yield return new WaitForSeconds(2f);
                pg.MoveRowMulti(topRemoved, bottomRemoved, shielders[0].xPos - 1, shielders[0].xPos);
            }

            if (attackers.Count >= 3)
            {
                int rightRemoved = PlayerGrid.MinColumn;
                int leftRemoved = PlayerGrid.GridWidth - 1;

                string output = "attack: ";

                foreach (UnitController uc in attackers)
                {
                    StartCoroutine(Attack(attackers));
                    output += uc.xPos + ", ";
                    //matchMade = true;

                    if (uc.xPos < leftRemoved) leftRemoved = uc.xPos;
                    if (uc.xPos > rightRemoved) rightRemoved = uc.xPos;
                }

                yield return new WaitForSeconds(UnitData.attackFusionDelay + UnitData.moveDuration * 2);
                
                //broken: attack going to wrong row if double cursor reversed
                print(yPos);

                //optimization: might split into two types of functions
                pg.MoveRow(yPos, leftRemoved - 1, rightRemoved - 1); //moves the left side in
                pg.MoveRow(yPos, rightRemoved + 1, rightRemoved); //moves the right side back

                print(output);
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


    //aesthetic: move shield to front and animate
    //todo: move to unit actions and do dependcy magic 
    private IEnumerator Shield (List<UnitController> list)
	{
        yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

        Destroy(this.gameObject);
    }

    //aesthetic: split up into different parts and use function events from animations to sync up
    private IEnumerator Attack (List<UnitController> list)
	{
        yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

        isReadyToMove = false;

        for (int i = 1; i < list.Count; i++)
		{
            list[i].Move(list[0].xPos, list[0].yPos);
		}
        yield return new WaitForSeconds(UnitData.moveDuration + UnitData.attackFusionDelay); // waits for fusion to complete

        for (int i = 1; i < list.Count; i++)
		{
            Destroy(list[i].gameObject);
		}

		list[0].Move(PlayerGrid.GridWidth - 1, list[0].yPos);

        yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front

        anim.SetBool("isAttack", true);
        //animation of attack
        //do a unit action?? destroy and move row again
    }
}




[System.Serializable]
public class example
{
    public float mana;
}