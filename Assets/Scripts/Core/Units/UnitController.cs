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
    [HideInInspector] public bool movable;

    //References
    Animator anim;

    #endregion

    public example ex;

	private void Awake()
	{
        anim = this.GetComponent<Animator>();

        anim.SetBool("isIdle", true);
        gameManager = FindObjectOfType<GameManager>();
        movable = true;
    }
    public void Summon(int xDestination, int yDestination)
	{
        //smoke effects
        //play summon animation
        //sfx
        transform.position = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
        pg.GridArray[xDestination, yDestination] = this;
        xPos = xDestination;
        yPos = yDestination;
    }

    //todo: maybe pass in a function as a parameter so you callback oncomplete instead of checking the move duration.???
    public UnitController Move(int xDestination, int yDestination)
	{
        if (movable)
        {
            anim.SetBool("isMove", true);

            Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
            float xflip = this.transform.localScale.x;

            //note: not sure if I need this. Makes speed the same even if travel distance is bigger
            int speedMofifier = 1;
/*
            if (xDestination != xPos)
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
                xflip *= -1;
                this.transform.localScale = new Vector2(xflip, transform.localScale.y); //flip when going back
            }
            transform.DOMove(Destination, UnitData.moveDuration * speedMofifier).OnComplete(() =>
            {
                anim.SetBool("isMove", false);

                if (xflip < 0) xflip *= -1;
                this.transform.localScale = new Vector2(xflip, transform.localScale.y);
            });

            UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
            pg.GridArray[xDestination, yDestination] = this;
            xPos = xDestination;
            yPos = yDestination;

            List<UnitController> list = new List<UnitController>();
            list.Add(this);

            return secondaryUnit;
        }
        else 
            return null;
    }

    public void RemoveUnit ()
	{
        //doesn't call because "wait until matchmakingcomplete"
        //todo: find a way to store the values and call later instead of doing it through coroutines maybe?
        pg.StartCoroutine(pg.MoveRow(yPos, xPos - 1, xPos));
        Destroy(this.gameObject);
	}

    //aesthetic: shield animate
    //todo: move to unit actions and do dependcy magic 
    public IEnumerator Shield (List<UnitController> shielders)
	{
        yield return new WaitUntil(() => pg.matchMakingComplete);
        //optimization: added a fraction so that the shield move happens after multi row move. It's possible to fix this at some point
        yield return new WaitForSeconds(UnitData.moveDuration + 0.01f); //waits for swap to complete

        pg.matchMakingComplete = false;

        for (int i = 0; i < shielders.Count; i++)
        {
            shielders[i].Move(PlayerGrid.GridWidth - 1, shielders[i].yPos); //move to front
        }
        yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front

        for (int i = 0; i < shielders.Count; i++)
        {
            shielders[i].movable = false;
            shielders[i].anim.SetBool("isShield", true);
        }
    }

    //aesthetic: split up into different parts and use function events from animations to sync up
    public IEnumerator Attack (List<UnitController> attackers)
	{
        yield return new WaitUntil(() => pg.matchMakingComplete);
        yield return new WaitForSeconds(UnitData.moveDuration); //waits for swap to complete

        pg.matchMakingComplete = false;

        for (int i = 1; i < attackers.Count; i++) 
        {
            attackers[i].Move(attackers[0].xPos, attackers[0].yPos); //fusion
        } 
        yield return new WaitForSeconds(UnitData.moveDuration + UnitData.attackFusionDelay);

        for (int i = 1; i < attackers.Count; i++) { Destroy(attackers[i].gameObject); } 
		attackers[0].Move(PlayerGrid.GridWidth - 1, attackers[0].yPos); //moves to front
        yield return new WaitForSeconds(UnitData.moveDuration + 0.4f); // waits for move to front

        attackers[0].movable = false;
        attackers[0].anim.SetBool("isAttack", true);
        //do a unit action?? destroy and move row again
    }
}

[Serializable]
public class example
{
    public float mana;
}