using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Leader")]
public class PlayerStats : ScriptableObject
{
    //chain multiplier scaling values
    //p1 ability
    public string minorAbilityName;
    public Sprite minorAbilityPortrait;
     
    public string ultimateAbilityName;
    public Sprite ultimateAbilityPortraitPortrait;

    public string characterName;
    public Sprite characterPortrait;

    public int startingHealth;
    public float maxMana;
}
