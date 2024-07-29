using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FlowerProject;

public class HangFlowerTests
{
    private UIDocument gameUIDocument;
    private WordManager wordManager;
    private VisualElement keyboard;
    private VisualElement rootG;
    private Word_SO wordSO;
    private bool isWon = false;
    private bool isLost = false;

    [UnitySetUp]
    public IEnumerator SetUpWord()
    {
        SceneManager.LoadScene(0); // Ensure the correct scene is loaded
        yield return null; // Wait for the scene to load
        
        gameUIDocument = GameObject.Find("UIGame").GetComponent<UIDocument>();
        wordManager = GameObject.Find("WordManager").GetComponent<WordManager>();

        Assert.IsNotNull(gameUIDocument, "gameUIDocument should not be null.");
        Assert.IsNotNull(wordManager, "wordManager should not be null.");
        // Set up a known word for testing
        wordSO = ScriptableObject.CreateInstance<Word_SO>();

        wordSO.values = new SerializableWord
        {
            id = "1234",
            common = "TEST",
            scientific = "TESTINGWORD",
            description = "A test word",
            hint = "This is a test",
            category = Category.Flower
        };

        // Assign the word to WordManager
        wordManager.wordSO = wordSO;
        wordManager.wordToGuess = wordSO.values.common.ToCharArray();
        wordManager.wordDisplay = "____";

        rootG = gameUIDocument.rootVisualElement;
        keyboard = rootG.Q<VisualElement>("virtual-keyboard");

        GameManager.OnWin += OnWinHandler;
        GameManager.OnLose += OnLoseHandler;

        // Check if the virtual keyboard and letter button exist
        Assert.IsNotNull(keyboard, "Virtual keyboard should not be null.");
    }
    

    private IEnumerator SimulateButtonPress(Button button)
    {
        using (var clicked = new NavigationSubmitEvent() { target = button })
        {
            button.SendEvent(clicked);
        }
        yield return null; 
    }

    [UnityTest]
    public IEnumerator CorrectLetterTest()
    {
        yield return SetUpWord();


        var letterButton = keyboard.Q<Button>("T");
        Assert.IsNotNull(letterButton, "Letter button T should not be null.");

        // Simulate pressing a correct letter
        yield return SimulateButtonPress(letterButton);

        // Check if the letter appears in the displayWord and displayWordLabel
        var displayWordLabel = rootG.Q<Label>("display-word");
        Assert.IsTrue(displayWordLabel.text.Contains("T"), "The letter 'T' should appear in the displayed word.");
        Assert.IsTrue(wordManager.wordDisplay == "T__T");

        // Check if the button is disabled
        Assert.AreEqual(PickingMode.Ignore, letterButton.pickingMode, "The button for 'T' should be disabled after being pressed.");

        
        Assert.AreEqual(7, wordManager.flower.Lives, "Lives should not decrease after a correct guess.");

        yield return SimulateButtonPress(keyboard.Q<Button>("E"));
        Assert.IsTrue(wordManager.wordDisplay == "TE_T");
        Assert.AreEqual(PickingMode.Ignore, keyboard.Q<Button>("E").pickingMode, "The button for 'E' should be disabled after being pressed.");

        yield return SimulateButtonPress(keyboard.Q<Button>("S"));
        Assert.IsTrue(wordManager.wordDisplay == wordSO.values.common, "When fully guessed the wordDisplay should be the same as the wordSO.value.*difficulty*");
        Assert.AreEqual(PickingMode.Ignore, keyboard.Q<Button>("S").pickingMode, "The button for 'S' should be disabled after being pressed.");
        yield return new WaitForSeconds(2f);
        Assert.IsTrue(isWon, "When the correct word is guessed the OnWin event should be triggered.");
    }  

    [UnityTest]
    public IEnumerator WrongLetterTest()
    {
        wordManager.wordToGuess = wordSO.values.common.ToCharArray();
        wordManager.wordDisplay = "____";

        var letterButton = keyboard.Q<Button>("Y");
        Assert.IsNotNull(letterButton, "Letter button Y should not be null.");

        // Simulate pressing a wrong letter
        yield return SimulateButtonPress(letterButton);


        // Check if the button is disabled
        Assert.AreEqual(PickingMode.Ignore, letterButton.pickingMode, "The button for 'Y' should be disabled after being pressed.");
        yield return new WaitForSeconds(1f);
        Assert.IsTrue(letterButton.ClassListContains("letter-wrong"),"The button 'Y' should have the class letter-wrong.");
        var displayWordLabel = rootG.Q<Label>("display-word");
        Assert.IsTrue(!displayWordLabel.text.Contains("Y"), "The letter 'Y' shouldn't appear in the displayed word.");
        Assert.AreEqual(6, wordManager.flower.Lives, "Lives should be 6 after a wrong guess.");

        yield return SimulateButtonPress(keyboard.Q<Button>("E"));
        Assert.IsTrue(wordManager.wordDisplay == "_E__");
        Assert.AreEqual(PickingMode.Ignore, keyboard.Q<Button>("E").pickingMode, "The button for 'E' should be disabled after being pressed.");

        wordManager.flower.Lives = 1;
        yield return SimulateButtonPress(keyboard.Q<Button>("U"));
        Assert.AreEqual(PickingMode.Ignore, keyboard.Q<Button>("U").pickingMode, "The button for 'U' should be disabled after being pressed.");
        yield return new WaitForSeconds(5f);
        Assert.IsTrue(keyboard.Q<Button>("U").ClassListContains("letter-wrong"),"The button 'U' should have the class letter-wrong.");
        Assert.IsTrue(isLost, "When flower.Lives is brought to 0 by a wrong guess, OnLost event should be triggered.");
    }


    private void OnWinHandler()
    {
        isWon = true;
    }

    private void OnLoseHandler()
    {
        isLost = true;
    }

}