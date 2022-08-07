using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Leader")]
public class PlayerStats : ScriptableObject
{
    #region Public Fields
    #endregion

    #region Private Fields
    #endregion

    //chain multiplier scaling values
    //P1 Score
    //P1 character
    //p1 ability
    public string characterName;

    public int health;
    public float mana;
    public Sprite characterPortrait;
}
