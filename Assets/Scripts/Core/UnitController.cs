using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{

    #region Public Fields
    public void SetGridPosition(int x, int y)
	{
        xPos = x;
        yPos = y;
	}

    public UnitData data;
    public bool justMoved = false;
    [HideInInspector] public PlayerGrid pg;
    public int unitIndex; //note: might need later for more optimized randomization.
    [HideInInspector] public bool isIdle
    {
        get { return _isIdle; }
        set
        {
            _isIdle = value;
            _isAttack = !value;
            _isShield = !value;
            anim.SetBool("isIdle", value);
            anim.SetBool("isAttack", !value);
            anim.SetBool("isShield", !value);
        }
    }
    [HideInInspector] public bool isAttack
    {
        get { return _isAttack; }
        set
        {
            _isIdle = !value;
            _isAttack = value;
            _isShield = !value;
            anim.SetBool("isIdle", !value);
            anim.SetBool("isAttack", value);
            anim.SetBool("isShield", !value);
        }
    }
    [HideInInspector] public bool isShield
    {
        get { return _isShield; }
        set
        {
            _isIdle = !value;
            _isAttack = !value;
            _isShield = value;
            anim.SetBool("isIdle", !value);
            anim.SetBool("isAttack", !value);
            anim.SetBool("isShield", value);
        }
    }
    [HideInInspector] public int xPos = 0;
    [HideInInspector] public int yPos = 0;
    [HideInInspector] public Vector2Int Pos
	{
        get { return new Vector2Int(xPos,yPos); }
        set { xPos = value.x; yPos = value.y; }
	}

    [HideInInspector] public bool swappable;

    #endregion

    #region Private Fields

    bool _isIdle;
    bool _isAttack;
    bool _isShield;

    //References
    Animator anim;

    #endregion

    private void OnDestroy()
    {

    }

    public example ex;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
        swappable = true;
        isIdle = true;
    }
    public void Summon(int xDestination, int yDestination)
    {
        //smoke effects
        //play summon animation
        //sfx
        transform.position = new Vector2(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y);
        pg.GridArray[xDestination, yDestination] = this;
        xPos = xDestination;
        yPos = yDestination;
    }

    public void Move(Action OnMoveCompleteCallback)
    {
        Internal_Move(xPos, yPos, OnMoveCompleteCallback, null, -1);
    }
    public void Move()
    {
        Internal_Move(xPos, yPos, null, null, -1);
    }

    //anim move is used when units go on top of each other.
    public void AnimationMove(int xDestination, int yDestination)
	{
        Internal_Move(xDestination, yDestination, null, null, -1);
	}
    public void AnimationMove(int xDestination, int yDestination, Action OnMoveCompleteCallback)
    {
        Internal_Move(xDestination, yDestination, OnMoveCompleteCallback, null, -1);

    }
    public void AnimationMove(int xDestination, int yDestination, int iterator, Action<int> OnMoveCompleteCallback)
    {
        Internal_Move(xDestination, yDestination, null, OnMoveCompleteCallback, iterator); 

    }

    //todo: make multiple actions and only call one if it's not not null... not the best but it works
    void Internal_Move(int xDestination, int yDestination, Action DefaultAction, Action<int> AttackAction, int i)
    {
        anim.SetBool("isMove", true);

        float xTarget = pg.cols[xDestination].position.x;
        float yTarget = pg.rows[yDestination].position.y;

        //print(string.Format("x: {0}, y: {1}", xPos, yPos));
        Vector2 Destination = new Vector2(xTarget, yTarget);
        float xflip = this.transform.localScale.x;

        //note: not sure if I need this. Makes speed the same even if travel distance is bigger
        int speedMofifier = 1;
        {/*
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
        }
        if (xTarget < transform.position.x)
        {
            xflip *= -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y); //flip when going back
        }
        transform.DOMove(Destination, UnitData.moveDuration * speedMofifier).OnComplete(() =>
        {
            anim.SetBool("isMove", false);

            if (xflip < 0) xflip *= -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y);

            DefaultAction?.Invoke();
            AttackAction?.Invoke(i);
        });

        //todo: update grid array via event so that not every unit has reference to pg?
    }
    public void RemoveUnit()
    {
        this.RemoveFromGrid();
        pg.MoveRow(yPos, xPos - 1, xPos);
        Destroy(this.gameObject);
    }

    public void RemoveFromGrid()
	{
        pg.GridArray[xPos, yPos] = null;
	}
}

[Serializable]
public class example
{
    public float mana;
}