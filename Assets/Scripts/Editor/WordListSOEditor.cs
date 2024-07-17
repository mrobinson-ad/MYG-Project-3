using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WordList_SO))]
public class WordListSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WordList_SO wordList = (WordList_SO)target;

        DrawDefaultInspector();

        foreach (var wordSO in wordList.allWords)
        {
            EditorGUILayout.ObjectField("Word_SO", wordSO, typeof(Word_SO), false);
        }
    }
}