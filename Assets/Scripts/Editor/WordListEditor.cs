using UnityEditor;
using UnityEngine;


// Creates an editor button for WordList_SO to fetch the Word_SO scriptable objects in the WordData folder
[CustomEditor(typeof(WordList_SO))]
public class WordListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WordList_SO wordList = (WordList_SO)target;

        if (GUILayout.Button("Find and Add Words from Folder"))
        {
            string folderPath = "Assets/WordData";
            wordList.FindAndAddWordsFromFolder(folderPath);
            EditorUtility.SetDirty(wordList);
        }
    }
}