using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace FlowerProject
{
    /// <summary>
    /// Class in charge of displaying the underscores for the word at the start of the game and revealing letters
    /// </summary>
    public class WordRevealer : MonoBehaviour
    {
        public static WordRevealer Instance { get; private set; }

        public string wordToFind;

        public char[] charsToFind;

        private char[] wordDisplay;

        public TMP_Text displayedWord;


        private void Awake()
        {
            Instance = this;
            wordDisplay = SetWord(wordToFind);
            displayedWord.text = new string(wordDisplay);
        }

        /// <summary>
        /// Returns a char array of underscores equal to the length of the word to find
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reveals the next consonant in the word array that hasn't been revealed yet
        /// </summary>
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


        /// <summary>
        /// Reveals next vowel in the word array that hasn't been revealed yet
        /// </summary>
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
}