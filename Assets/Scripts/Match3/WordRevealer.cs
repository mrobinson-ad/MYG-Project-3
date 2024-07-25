using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WordRevealer : MonoBehaviour
{
    public static WordRevealer Instance { get; private set; }

    public string wordToFind;
    public WordList_SO wordList;

    public char[] charsToFind;

    private char[] wordDisplay;

    public TMP_Text displayedWord;


    private void Awake()
    {
        Instance = this;
        wordDisplay = SetWord(wordToFind);
        displayedWord.text = new string(wordDisplay);
    }


    private char[] SetWord(string word)
    {
        // Initialize the wordDisplay array with underscores representing hidden characters
        char[] x = new char[word.Length];
        for (int i = 0; i < word.Length; i++)
        {
            x[i] = '_';
        }
        return x;
    }

    public void RevealConsonant()
    {
        // Find the first consonant in wordToFind that is not already revealed in wordDisplay
        char x = wordToFind
                    .FirstOrDefault(c => "bcdfghjklmnpqrstvwxyz".Contains(char.ToLower(c)) && !wordDisplay.Contains(c));

        if (x != '\0') // If a consonant was found
        {
            // Replace all occurrences of x in wordDisplay
            for (int i = 0; i < wordDisplay.Length; i++)
            {
                if (wordToFind[i] == x)
                {
                    wordDisplay[i] = x;
                }
            }
        }

        displayedWord.text = new string(wordDisplay);
    }



    public void RevealVowel()
    {
        // Find the first vowel in wordToFind that is not already revealed in wordDisplay
        char x = wordToFind
                    .FirstOrDefault(c => "aeiou".Contains(char.ToLower(c)) && !wordDisplay.Contains(c));

        if (x != '\0') // If a vowel was found
        {
            // Replace all occurrences of x in wordDisplay
            for (int i = 0; i < wordDisplay.Length; i++)
            {
                if (wordToFind[i] == x)
                {
                    wordDisplay[i] = x;
                }
            }
        }

        displayedWord.text = new string(wordDisplay);
    }

}
