using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    #region Public Fields

    public UnitData unit;
    public bool justMoved = false;
    public PlayerGrid pg;

    #endregion
    
    #region Private Fields

    [SerializeField] float moveDuration;
    [SerializeField] int xPos = 3;
    [SerializeField] int yPos = 4;
    
    //References
    Animator anim;

    #endregion

    public example ex;


    private void Start()
    {
        anim = this.GetComponent<Animator>();
        anim.SetTrigger("idle");
        transform.DOMoveY(pg.rows[yPos].position.y, moveDuration);
        transform.DOMoveX(pg.cols[xPos].position.x, moveDuration);
    }

    private void Update()
    {
        Movement();
    }

    public UnitController Move(int xDestination, int yDestination)
	{
        anim.SetBool("isRun", true);
        Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
        transform.DOMove(Destination, moveDuration).OnComplete(() => {
            anim.SetBool("isRun", false);
        });

        UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
        pg.GridArray[xDestination, yDestination] = this;

        return secondaryUnit;

    }

    private void Movement()
    {
        /*if (Input.GetKeyDown("d") && xPos < PlayerGrid.GridWidth - 1)
        {
            anim.SetBool("isRun", true);
            transform.DOMoveX(pg.cols[++xPos].position.x, moveDuration).OnComplete(() => {
                anim.SetBool("isRun", false);
            });
        }
        else if (Input.GetKeyDown("a") && xPos > 0)
        {
            anim.SetBool("isRun", true);
            transform.DOMoveX(pg.cols[--xPos].position.x, moveDuration).OnComplete(() => {
                anim.SetBool("isRun", false);
            });
        }
        else if (Input.GetKeyDown("w") && yPos < PlayerGrid.GridHeight - 1)
        {
            anim.SetBool("isRun", true);
            transform.DOMoveY(pg.rows[++yPos].position.y, moveDuration).OnComplete(() => {
                anim.SetBool("isRun", false);
            });
        }
        else if (Input.GetKeyDown("s") && yPos > 0)
        {
            transform.DOMoveY(pg.rows[--yPos].position.y, moveDuration).OnComplete(() => {
                anim.SetBool("isRun", false);
            });
        }

        justMoved = false;
        */
    }
}

[System.Serializable]
public class example
{
    public float mana;
}