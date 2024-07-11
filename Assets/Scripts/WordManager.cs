using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using CustomAttributes;
using DG.Tweening;

public enum Difficulty
{
    Common,
    Scientific
}
public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; } // Singleton

    #region UI variables
    private VisualElement root;
    public UIDocument gameUIDocument;
    private VisualElement virtualKeyboard;
    private Label displayWord; // label where the underscores and guessed letters of the word to guess are displayed
    private Label hint;
    private Button[] letter; // array of Buttons containing each letter of the alphabet

    #endregion

    [ReadOnly]
    public string wordDisplay; // the word that the player needs to guess
    public WordList_SO wordListSO; // the scriptable object containing lists of wordSO separated by categories
    private Word_SO wordSO; // the scriptable object containing the word to guess and all its relevant data
    public char[] wordToGuess; // the word that the player has to guess converted to an array for easier comparison
    public Difficulty difficulty;

    public Flower flower;
    public int fontSize_c = 25;
    public int fontSize_s = 20;


    private void Awake()
    {
        root = gameUIDocument.rootVisualElement;
        virtualKeyboard = root.Q<VisualElement>("virtual-keyboard");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeVirtualKeyboard();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        displayWord = root.Q<Label>("display-word");
        hint = root.Q<Label>("hint");
    }
    // Gets a random wordSO in the specified category in the wordlistSO
    private Word_SO GetWord(List<Word_SO> wordList)
    {
        return wordList[Random.Range(0, wordList.Count)];
    }

    // Sets the display word to show an underscore for each letter in the word to guess 
    private void SetEmptyWord(char[] wordArray)
    {
        wordDisplay = "";
        for (int i = 0; i < wordArray.Length; i++)
        {
            if (wordArray[i] == ' ')
            {
                wordDisplay += " ";
            }
            else
            {
                wordDisplay += "_";
            }
        }
        displayWord.text = "<cspace=0.25em>" + wordDisplay + "</cspace>"; // use rich text format to space the letters
    }

    public void SetNewWord(Category category) // same as SetNewWord but gets a word from a specific category
    {
        flower.Lives = 7;
        ResetKeyboard();
        switch (category)
        {
            case Category.Flower:
                wordSO = GetWord(wordListSO.flowerList);
                break;
            case Category.Houseplant:
                wordSO = GetWord(wordListSO.houseplantList);
                break;
            case Category.Aromatic:
                wordSO = GetWord(wordListSO.aromaticList);
                break;
        }
        hint.text = wordSO.values.hint;
        switch (difficulty)
        {
            case Difficulty.Common:
            wordToGuess = wordSO.values.common.ToCharArray();
            displayWord.style.fontSize = fontSize_c;
            Debug.Log(wordSO.values.common);
                break;
            case Difficulty.Scientific:
            wordToGuess = wordSO.values.scientific.ToCharArray();
            displayWord.style.fontSize = fontSize_s;
            Debug.Log(wordSO.values.scientific);
                break;
        }
        SetEmptyWord(wordToGuess);
    }

    public void SetNewWord() // Resets lives, keyboard and gets a new random word depending on difficulty
    {
        flower.Lives = 7;
        ResetKeyboard();
        wordSO = GetWord(wordListSO.allWords);
        hint.text = wordSO.values.hint;
        switch (difficulty)
        {
            case Difficulty.Common:
            wordToGuess = wordSO.values.common.ToCharArray();
            displayWord.style.fontSize = fontSize_c;
            Debug.Log(wordSO.values.common);
                break;
            case Difficulty.Scientific:
            wordToGuess = wordSO.values.scientific.ToCharArray();
            displayWord.style.fontSize = fontSize_s;
            Debug.Log(wordSO.values.scientific);
                break;
        }
        SetEmptyWord(wordToGuess);
    }

    // Add each character button in virtual keyboard to the letters array
    private void InitializeVirtualKeyboard()
    {
        var buttons = virtualKeyboard.Query<Button>().ToList();
        letter = new Button[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            letter[i] = buttons[i];
            char character = char.Parse(letter[i].text);
            letter[i].clicked += () => OnLetterClicked(character);
        }
    }

    // Check if letter is part of the word and return the locations of each occurrence in order to update the displayed word
    private void OnLetterClicked(char c)
    {
        Debug.Log("You pressed " + c);

        bool found = false;
        int timesFound = 0;
        char[] displayArray = wordDisplay.ToCharArray();

        for (int i = 0; i < wordToGuess.Length; i++)
        {
            if (wordToGuess[i] == c)
            {
                displayArray[i] = c;
                found = true;
                timesFound += 1;
            }
        }

        if (found)
        {
            wordDisplay = new string(displayArray);
            displayWord.text = "<cspace=0.25em>" + wordDisplay + "</cspace>";
            Debug.Log(c + " appears " + timesFound + " times in the word");
            if (wordDisplay == new string(wordToGuess))
            {
                GameManager.Win(difficulty);
            }
            virtualKeyboard.Q<Button>(c.ToString()).AddToClassList("letter-correct");
        }
        else
        {
            Debug.Log(c + " is not part of the word.");
            virtualKeyboard.Q<Button>(c.ToString()).AddToClassList("letter-wrong");
            virtualKeyboard.Q<VisualElement>(c.ToString()).DOShake(5);
            flower.Lives--;
        }
        virtualKeyboard.Q<Button>(c.ToString()).pickingMode = PickingMode.Ignore; // disable the button after it's clicked
    }



    private void ResetKeyboard() //resets the buttons' style and removes the styles that color the buttons
    {
        var buttons = virtualKeyboard.Query<Button>().ToList();
        letter = new Button[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            letter[i] = buttons[i];
            letter[i].pickingMode = PickingMode.Position;
            letter[i].RemoveFromClassList("letter-correct");
            letter[i].RemoveFromClassList("letter-wrong");
        }
    }

    private void CheckWin()
    {
        if (wordDisplay == new string(wordToGuess))
        {
            GameManager.Win(difficulty);
        }
    }
}