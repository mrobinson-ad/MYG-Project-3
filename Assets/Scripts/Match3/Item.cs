using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public int value;

    public ItemType itemType;

}

public enum ItemType
{
    Consonant,
    Vowel,
    Power,
    Minus,
    Hinder,
    Null
}
