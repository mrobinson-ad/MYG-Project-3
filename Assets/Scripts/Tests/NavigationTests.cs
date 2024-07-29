using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FlowerProject;
using PlayFab.ClientModels;

public class NavigationTests
{
    private UIDocument gameUIDocument;
    private Label displayWord;
    private char[] wordToGuess;
    private string wordDisplay;
    

    // A Test behaves as an ordinary method
    [UnityTest]
    public IEnumerator CorrectLetterTest()
    {
        SceneManager.LoadScene(0);
        yield return null;
        wordDisplay = "flower";
        var wordManager = new WordManager();
        wordManager.wordToGuess = wordDisplay.ToCharArray();
    }

}
