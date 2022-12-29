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
    int unitStrength;

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
            swappable = !value;
            anim.SetBool("isIdle", !value);
            anim.SetBool("isAttack", value);
            anim.SetBool("isShield", !value);
            unitStrength = data.baseAttackStrength;

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
			swappable = !value;

            unitStrength = data.baseShieldStrength;

			//todo: I'm trying to make the shield set to a speicific frame based on combo.
			if (value)
            {
                anim.SetBool("isShield", value);
                float clipLength = anim.GetCurrentAnimatorClipInfo(0).Length;
                //print(anim.GetCurrentAnimatorClipInfo(0));
                if (clipLength > 0)
				{
                    anim.Play("Shield", 0, 0.5f);
					// anim Animation["Shield"].Length???
				}
            }
            else 

            anim.SetBool("isIdle", !value);
            anim.SetBool("isAttack", !value);

        }
    }


    IEnumerator DelayValueChange<T> (T changedVariable, T valueToChangeTo)
	{
        yield return new WaitForSeconds(GameManager._.unitMoveSpeed);

	}
    [HideInInspector] public int xPos = 0;
    [HideInInspector] public int yPos = 0;
    [HideInInspector] public Vector2Int Pos
	{
        get { return new Vector2Int(xPos,yPos); }
        set { xPos = value.x; yPos = value.y; }
	}

    [HideInInspector] public bool swappable;
    [HideInInspector] public bool movable;

    public float remainingShieldTime
	{
        get { return anim.GetFloat("remainingShieldTime"); }
        set { anim.SetFloat("remainingShieldTime", value); }
	}

    public bool queuedToBeMoved = false;

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
        movable = true;
        isIdle = true;

    }

    private unitColors _color;

    void LoadData ()
	{
        
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
        UpdateName();

    }

    public void Move(Action OnMoveCompleteCallback)
    {
        Internal_Move(xPos, yPos, OnMoveCompleteCallback, null, -1);
    }
    public void Move()
    {
        queuedToBeMoved = false;
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

        //broken: switching this on and off is still kind of broken. Like it doesn't let you buffer moves
        swappable = false;

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
        if (xTarget < xPos)
        {
            xflip *= -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y); //flip when going back
        }
        transform.DOMove(Destination, GameManager._.unitMoveSpeed * speedMofifier).OnComplete(() =>
        {
            anim.SetBool("isMove", false);

            if (xflip < 0) xflip *= -1;
            this.transform.localScale = new Vector2(xflip, transform.localScale.y);

            //broken: switching this on and off is still kind of broken. Like it doesn't let you buffer moves
            swappable = true;

            UpdateName();

            DefaultAction?.Invoke();
            AttackAction?.Invoke(i);
        });

        //todo: update grid array via event so that not every unit has reference to pg?
    }

    void UpdateName () => this.gameObject.name =  "(" + xPos + ", " + yPos + ") " + data.name;

    public void DeleteUnit()
    {
        if (pg.GridArray[xPos, yPos] != null)
        {
            pg.StartCoroutine(pg.PauseMove(0.2f));
            this.RemoveFromGrid();
            pg.MoveRow(yPos, xPos - 1, xPos);
            DOTween.Kill(this.transform);
            Destroy(this.gameObject);
        }
        else
            Debug.LogError("deleting an empty space");
    }
    public enum ActionType
	{
        Attack, Shield
	}
    public GameObject attackObject;
    //need to learn: what is this exactly?
    public void Release(ActionType at)
    {
        if (at == ActionType.Attack)
		{
            GameObject aO = Instantiate(attackObject);
            aO.transform.position = this.transform.position;
            //todo: attackObjectInfo constructor to determine attack unitStrength
            //for now just add a default value
            
            //aO unitStrength assign
            GameManager._.AttackRelease(this);
		}
        else if (at == ActionType.Shield)
            GameManager._.ShieldRelease(this);
        else
            Debug.LogError("error"); //error

        DeleteUnit();
	}

    //optimization: seems messy to have two versions of very similar functions... maybe consolidate

    public void RemoveFromGrid() => pg.GridArray[xPos, yPos] = null;

    void UpdateStrength ()
	{

	}
	private void OnTriggerEnter2D(Collider2D collision)
	{
        GameObject hitGO = collision.gameObject;

        if (hitGO.CompareTag("Attack"))
		{

            //takes damage... for now just dies
            //attack vs unit unitStrength 

            int enemyAttackStrength = hitGO.GetComponent<AttackTravel>().attackStrength;
            if (unitStrength >= enemyAttackStrength)
            {
                unitStrength -= enemyAttackStrength;
                Destroy(hitGO);
			}
			else
            {
                enemyAttackStrength -= unitStrength;
                DeleteUnit();
            }
		}
	}
}

[Serializable]
public class example
{
    public float mana;
}