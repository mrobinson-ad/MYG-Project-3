using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "WordList", menuName = "Scriptable Objects/WordList")]
public class WordList_SO : ScriptableObject
{
    public string folderPath = "Assets/Resources/Words"; // Folder to save the assets
    public List<Word_SO> allWords = new List<Word_SO>();
    public List<Word_SO> flowerList = new List<Word_SO>();
    public List<Word_SO> houseplantList = new List<Word_SO>();
    public List<Word_SO> aromaticList = new List<Word_SO>();

    // Fills each list with Word_SO scriptable objects depending on their category
    public void BuildCategoryLists()
    {
        flowerList.Clear();
        houseplantList.Clear();
        aromaticList.Clear();

        foreach (Word_SO word in allWords)
        {
            switch (word.values.category)
            {
                case Category.Flower:
                    flowerList.Add(word);
                    break;
                case Category.Houseplant:
                    houseplantList.Add(word);
                    break;
                case Category.Aromatic:
                    aromaticList.Add(word);
                    break;
            }
        }
    }
}