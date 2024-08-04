using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Item ScriptableObject holds the sprite, value and type of the items that the tiles can hold
/// </summary>
[CreateAssetMenu(menuName = "Match-3/Item")]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public int value;

    public ItemType itemType;

}

/// <summary>
/// Defines the type of an item for matching and their effects when matched
/// </summary>
public enum ItemType
{
    Consonant,
    Vowel,
    Power,
    Minus,
    Hinder,
    Null
}
