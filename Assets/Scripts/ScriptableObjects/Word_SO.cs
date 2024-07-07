using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public enum Category 
{
    Flower,
    Houseplant,
    Aromatic
}


[CreateAssetMenu(fileName = "Word", menuName = "Scriptable Objects/Word")]
public class Word_SO : ScriptableObject
{
    public SerializableWord values;
    private void OnValidate() // makes sure the Length is updated when changing the value the word
    {
        values.UpdateCommonLength();
        values.UpdateScientificLength();
    }
}

[System.Serializable]
public class SerializableWord 
{
    public int id;
    public string common;
    public string scientific;
    public string description;
    public string hint;
    public Category category;
    
    [ReadOnly]
    public int common_length;

    public void UpdateCommonLength()
    {
        common_length = common.Length;
    }
    [ReadOnly]
    public int scientific_length;

    public void UpdateScientificLength()
    {
        scientific_length = scientific.Length;
    }
}