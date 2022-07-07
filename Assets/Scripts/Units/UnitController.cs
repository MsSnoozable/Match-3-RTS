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
    public int startingColumn = 0;
    public int startingRow = 0;
    
    //References
    Animator anim;

    #endregion

    public example ex;

    private void Start()
    {
        anim = this.GetComponent<Animator>();
        anim.SetTrigger("idle");
        transform.DOMoveY(pg.rows[startingRow].position.y, moveDuration);
        transform.DOMoveX(pg.cols[startingColumn].position.x, moveDuration);
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
}

[System.Serializable]
public class example
{
    public float mana;
}