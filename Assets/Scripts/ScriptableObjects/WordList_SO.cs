using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordList", menuName = "Scriptable Objects/WordList")]
public class WordList_SO : ScriptableObject
{
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

    // Finds and adds Word_SO scriptable objects from the WordData folder
    public void FindAndAddWordsFromFolder(string folderPath)
    {
        #if UNITY_EDITOR
        allWords.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Word_SO", new[] { folderPath });

        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Word_SO word = UnityEditor.AssetDatabase.LoadAssetAtPath<Word_SO>(assetPath);
            if (word != null)
            {
                allWords.Add(word);
            }
        }
        BuildCategoryLists();
        #endif
    }
}