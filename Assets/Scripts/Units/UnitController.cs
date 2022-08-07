using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    #region Public Fields

    public UnitData unit;
    public bool justMoved = false;
    [HideInInspector] public PlayerGrid pg;
    public GameManager gameManager;

    #endregion
    
    #region Private Fields

    float moveDuration;
    [HideInInspector] public int xPos = 0;
    [HideInInspector] public int yPos = 0;
    
    //References
    Animator anim;

    #endregion

    public example ex;

	private void Awake()
	{
        anim = this.GetComponent<Animator>();
        anim.SetTrigger("idle");
        moveDuration = UnitData.moveDuration;
        gameManager = FindObjectOfType<GameManager>();
    }

	public UnitController Move(int xDestination, int yDestination)
	{
        anim.SetBool("isRun", true);

        Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
        float xflip = this.transform.localScale.x;

        if (xDestination < xPos)
        {
            //this.GetComponent<SpriteRenderer>().flipX = true;
            xflip *= -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y); //flip when going back
            
        }
        transform.DOMove(Destination, moveDuration).OnComplete(() =>
        {
            anim.SetBool("isRun", false);

            if (xflip < 0) xflip *= -1;
            //this.GetComponent<SpriteRenderer>().flipX = false; //todo:alternate with sprites later
            this.transform.localScale = new Vector2(xflip, transform.localScale.y);
        });

        UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
        pg.GridArray[xDestination, yDestination] = this;
        xPos = xDestination;
        yPos = yDestination;

        if (gameManager.isSetupComplete)
        {
            var thing = MatchCheck(xDestination, yDestination);
            print("Match was " + (thing ? "made succesffuly!!!" : "not made... you suck"));
        }
        return secondaryUnit;
    }

    //calls for each moved unit
    private bool MatchCheck (int x, int y)
	{
        bool matchMade = false;
        List<UnitController> attackers = new List<UnitController>();
        List<UnitController> shielders = new List<UnitController>();

        attackers.Add(this);
        shielders.Add(this);

        var movedUnit = pg.GridArray[x, y];

        for (int left = -1; left >= -2; left--)
        {
            if (x + left > 0) //in bounds
            {
                var nextUnit = pg.GridArray[x + left, y];
                if (movedUnit.unit == nextUnit.unit)
                {
                    attackers.Add(nextUnit);
                }
            }
        }
        for (int up = -1; up >= -2; up--)
        {
            if (y + up > 0) //in bounds
            {
                var nextUnit = pg.GridArray[x, y + up];
                if (movedUnit.unit == nextUnit.unit)
                {
                    shielders.Add(nextUnit);
                }
            }
        }
        for (int right = 1; right <= 2; right++)
		{
            if (x + right < PlayerGrid.GridWidth)
            {
                var nextUnit = pg.GridArray[x + right, y];
                if (movedUnit.unit == nextUnit.unit)
                {
                    attackers.Add(nextUnit);
                }
            }
        }
 
        for (int down = 1; down <= 2; down++)
        {
            if (y + down < PlayerGrid.GridHeight)
            {
                var nextUnit = pg.GridArray[x, y + down];
                if (movedUnit.unit == nextUnit.unit)
                {
                    shielders.Add(nextUnit);
                }
            }
        }


        //does attack and shield for 
        if (shielders.Count >= 3)
        {
            foreach (UnitController uc in shielders)
            {
                uc.Shield();
                matchMade = true;
            }
        }
        if (attackers.Count >= 3)
        {
            int rightMost = 0;
            int leftMost = PlayerGrid.GridWidth - 1;
            foreach (UnitController uc in attackers)
            {
                uc.Attack();
                matchMade = true;

                if (uc.yPos < leftMost)
				{
                    leftMost = uc.yPos;
				}
                else if (uc.yPos > rightMost)
				{
                    rightMost = uc.yPos;
				}
            }
            pg.MoveSubRow(xPos, leftMost - 1, rightMost, Direction.Right);
        }

        return matchMade;
    }
/*
    private bool StartMatchCheck(int x, int y)
    {
        var thing = pg.GridArray[x, y];

        List<UnitController> attackers = new List<UnitController>();
        List<UnitController> shielders = new List<UnitController>();

        attackers.Add(this);
        shielders.Add(this);

        MatchCheck(x, y, ref attackers, "A");
        MatchCheck(x, y, ref shielders, "S");
        //check right and add to list
        //check left and add to list
        //Call attack for whole list

        if (shielders.Count >= 3)
		{
            foreach (UnitController uc in shielders)
			{
                uc.unit.Shield();
                return true;
            }
        }
        
        //Repeat for up/down
        if (attackers.Count >= 3)
        {
            foreach (UnitController uc in attackers)
            {
                uc.unit.Attack(uc);
                return true;
            }
        }
        return false;
    }

    void MatchCheck(int x, int y, ref List<UnitController> list, string AorS)
	{
        //todo: oob errors gallore!!!!!

        if (AorS == "A")
		{
            UnitController right = pg.GridArray[x + 1, y];
            UnitController left = pg.GridArray[x - 1, y];

            if (pg.GridArray[x, y] == right)
            {
                list.Add(right);
                MatchCheck(x + 1, y, ref list, "A");
            }
            if (pg.GridArray[x, y] == left)
            {
                list.Add(left);
                MatchCheck(x - 1, y, ref list, "A");
            }
        }
        else if (AorS == "S")
		{
            UnitController up = pg.GridArray[x, y - 1];
            UnitController down = pg.GridArray[x, y + 1];

            if (pg.GridArray[x, y] == up)
		    {
                list.Add(up);
                MatchCheck(x, y - 1, ref list, "S");
            }
            if (pg.GridArray[x,y] == down)
		    {
                list.Add(down);
                MatchCheck(x, y + 1, ref list, "S");
            }
		}
        else
		{
            //error;
		}
        return;
	}*/



    private void Shield ()
	{
        //print(String.Format("Shield; x: {0}, y: {1}", xPos, yPos));
	}
    private void Attack ()
	{
        //print(String.Format("Attack; x: {0}, y: {1}", xPos, yPos));
        
    }
}




[System.Serializable]
public class example
{
    public float mana;
}