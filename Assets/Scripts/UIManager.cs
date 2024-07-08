using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // Singleton
    private VisualElement rootMM;
    private VisualElement rootG;
    private VisualElement rootS;
    public UIDocument mainMenuUIDocument;
    public UIDocument gameUIDocument;
    public UIDocument settingsUIDocument;
    private GroupBox difficultyBox;
    private GroupBox volumeBox;

    private void Awake()
    {
        rootMM = mainMenuUIDocument.rootVisualElement;
        rootG = gameUIDocument.rootVisualElement;
        rootS = settingsUIDocument.rootVisualElement;
        difficultyBox = rootS.Q<GroupBox>("difficulty-box");
        volumeBox = rootS.Q<GroupBox>("volume-box");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
           
        }
        else
        {
            Destroy(gameObject);
        }
        Settings.OnDifficultyChange += OnDifficultyChanged;

        difficultyBox.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (evt.newValue)
            {
                Settings.DifficultyChange();
            }
        });
    }

    private void OnDifficultyChanged()
    {
        var radioButtons = difficultyBox.Query<RadioButton>().Class("radio-difficulty").ToList();

        foreach (var radioButton in radioButtons)
        {
            if (radioButton.value)
            {
            
            string difficultyText = radioButton.label.Split()[0];
            Difficulty newDifficulty;
            
            if (Enum.TryParse(difficultyText, true, out newDifficulty))
            {

                WordManager.Instance.difficulty = newDifficulty;
                Debug.Log($"WordManager difficulty set to: {WordManager.Instance.difficulty}");
            }            
            break;            
            }
        }
    }

}