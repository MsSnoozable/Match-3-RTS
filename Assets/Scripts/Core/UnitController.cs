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

        anim.SetBool("isIdle", true);
        swappable = true;
        isIdle = true;
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

    public UnitController Move(int xDestination, int yDestination)
    {
        return Internal_Move(xDestination, yDestination, null);
    }
    public UnitController Move(int xDestination, int yDestination, Action OnMoveCompleteCallback)
    {
        return Internal_Move(xDestination, yDestination, OnMoveCompleteCallback);
    }
    public UnitController Move(Vector2Int Destination)
    {
        return Internal_Move(Destination.x, Destination.y, null);
    }
    public UnitController Move(Vector2Int Destination, Action OnMoveCompleteCallback)
    {
        return Internal_Move(Destination.x, Destination.y, OnMoveCompleteCallback);
    }

    //todo: maybe pass in a function as a parameter so you callback oncomplete instead of checking the move duration.???
    UnitController Internal_Move(int xDestination, int yDestination, Action OnMoveCompleteCallback)
    {
        anim.SetBool("isMove", true);

        Vector3 Destination = new Vector3(pg.cols[xDestination].position.x, pg.rows[yDestination].position.y, 0);
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
            
            OnMoveCompleteCallback?.Invoke();
        });

       
        //temp = x
        //x = y
        //y = temp
        UnitController secondaryUnit = pg.GridArray[xDestination, yDestination];
        pg.GridArray[xDestination, yDestination] = this;
        xPos = xDestination;
        yPos = yDestination;

        //todo: update grid array via event so that not every unit has reference to pg?

        return secondaryUnit;
    }

    public void RemoveUnit()
    {
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