using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    #region Public Fields

    public UnitData unit;
    public bool justMoved = false;
    [HideInInspector] public PlayerGrid pg;

    #endregion
    
    #region Private Fields

    [SerializeField] float moveDuration;
    int xPos = 0;
    int yPos = 0;
    
    //References
    Animator anim;

    Color color;

    #endregion

    public example ex;

    private void Start()
    {
        
    }

	private void Awake()
	{
        anim = this.GetComponent<Animator>();
        anim.SetTrigger("idle");
    }

	public UnitController Move(int xDestination, int yDestination)
	{
        anim.SetBool("isRun", true);

        Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);

        if (xDestination < xPos)
        {
            //this.GetComponent<SpriteRenderer>().flipX = true;
            float xflip = this.transform.localScale.x * -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y);
            transform.DOMove(Destination, moveDuration).OnComplete(() =>
            {
                anim.SetBool("isRun", false);
                //this.GetComponent<SpriteRenderer>().flipX = false;
                float xflip = this.transform.localScale.x * -1;
                this.transform.localScale = new Vector2(xflip, transform.localScale.y);
            });
        }
        else
        {
            transform.DOMove(Destination, moveDuration).OnComplete(() =>
            {
                anim.SetBool("isRun", false);
            });
        }
        

        UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
        pg.GridArray[xDestination, yDestination] = this;
        xPos = xDestination;
        yPos = yDestination;

        return secondaryUnit;
    }
}

[System.Serializable]
public class example
{
    public float mana;
}