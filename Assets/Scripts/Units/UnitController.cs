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

    #endregion
    
    #region Private Fields

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
        transform.DOMove(Destination, UnitData.moveDuration).OnComplete(() =>
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
        
        return secondaryUnit;
    }

    //calls for each moved unit
    public IEnumerator MatchCheck (int x, int y)
	{
        bool matchMade = false;
        List<UnitController> attackers = new List<UnitController>();
        List<UnitController> shielders = new List<UnitController>();

        attackers.Add(this);
        shielders.Add(this);

        UnitController movedUnit = pg.GridArray[x, y];
        UnitController nextUnit;

		#region AddMatchesToList
		for (int left = -1; left >= -xPos; left--)
        {
            if (x + left > 0) //in bounds
            {
                nextUnit = pg.GridArray[x + left, y];
                if (movedUnit.data == nextUnit.data) attackers.Add(nextUnit);
                else break;
            }
        }
        for (int up = -1; up >= -yPos; up--)
        {
            if (y + up > 0) //in bounds
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

        string list = "shields: ";
        foreach (UnitController s in shielders)
		{
            list += String.Format("({0},{1})", s.xPos, s.yPos);
		}

        list = "attacks: ";
        foreach (UnitController a in attackers)
        {
            list += String.Format("({0},{1})", a.xPos, a.yPos);
        }

        if (shielders.Count >= 3)
        {
            int bottomRemoved = 0;
            int topRemoved = PlayerGrid.GridHeight - 1;

            string output = "shield: ";

            foreach (UnitController uc in shielders)
            {
                uc.Invoke("Shield", UnitData.moveDuration);
                output += uc.yPos + ", ";
                matchMade = true;

                if (uc.yPos > bottomRemoved)
                {
                    bottomRemoved = uc.yPos;
                }
                if (uc.yPos < topRemoved)
                {
                    topRemoved = uc.yPos;
                }
            }
            //todo: refactor this to be less stupid
            print(output);
            print(String.Format("bot: {0}, top: {1}",bottomRemoved, topRemoved));
            yield return new WaitForSeconds(2f);
            pg.MoveRowMulti(topRemoved, bottomRemoved, shielders[0].xPos - 1, shielders[0].xPos);
        }

        if (attackers.Count >= 3)
        {
            int rightRemoved = 0;
            int leftRemoved = PlayerGrid.GridWidth - 1;

            string output = "attack: ";
            foreach (UnitController uc in attackers)
            {
                //uc.Attack();
                uc.Invoke("Attack", UnitData.moveDuration);
                output += uc.xPos + ", ";
                matchMade = true;

                if (uc.xPos < leftRemoved)
				{
                    leftRemoved = uc.xPos;
				}
                if (uc.xPos > rightRemoved)
				{
                    rightRemoved = uc.xPos;
				}
            }
            yield return new WaitForSeconds(2f);
            pg.MoveRow(yPos, leftRemoved - 1, rightRemoved);

            print(output);
        }
    }

    //todo: move shield/ atack to front and animate
    private void Shield ()
	{
        Destroy(this.gameObject);
    }
    private void Attack ()
	{
        Destroy(this.gameObject);
    }

}




[System.Serializable]
public class example
{
    public float mana;
}