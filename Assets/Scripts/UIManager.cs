using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UIElements;
using PlayFab.ClientModels;
using DG.Tweening;

public enum CurrentScene
{
    Main,
    Game,
    End
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; } // Singleton
    private VisualElement rootMM;
    private VisualElement rootG;
    private VisualElement rootS;
    public UIDocument mainMenuUIDocument;
    public UIDocument gameUIDocument;
    public UIDocument settingsUIDocument;
    public UIDocument endUIDocument;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Button exit;
    private GroupBox difficultyBox;
    private VisualElement statPanel;
    private CurrentScene currentScene = CurrentScene.Main;
    public GameObject carouselHandler;
    public VisualTreeAsset leaderboardEntryTemplate;

    private void Awake()
    {
        rootMM = mainMenuUIDocument.rootVisualElement;
        rootG = gameUIDocument.rootVisualElement;
        rootS = settingsUIDocument.rootVisualElement;
        difficultyBox = rootS.Q<GroupBox>("difficulty-box");
        musicSlider = rootS.Q<Slider>("music-slider");
        sfxSlider = rootS.Q<Slider>("sfx-slider");
        exit = rootS.Q<Button>("return-button");
        statPanel = rootMM.Q<VisualElement>("stat-panel");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
        statPanel.SetEnabled(false);
        Settings.OnDifficultyChange += OnDifficultyChanged;
        Settings.OnVolumeChange += OnVolumeChange;
        Settings.OnReturnPressed += OnReturnPressed;
        GameManager.OnLose += OnLose;
        GameManager.OnWin += OnWin;
        RegisterCallBacks();
    }



    private void RegisterCallBacks()
    {
        // MouseCaptureOutEvent is called when the user stops interacting with the slider
        musicSlider.RegisterValueChangedCallback(evt =>
        {
            Settings.VolumeChange(musicSlider, musicSlider.value);
        });

        sfxSlider.RegisterValueChangedCallback(evt =>
        {
            Settings.VolumeChange(sfxSlider, sfxSlider.value);
        });

        difficultyBox.RegisterCallback<ChangeEvent<bool>>(evt =>
        {
            if (evt.newValue)
            {
                Settings.DifficultyChange();
            }
        });

        exit.RegisterCallback<ClickEvent>(evt =>
        {
            AudioManager.SFXPressed("SFXButton");
            Settings.ReturnPressed(GetScene());
        });

        var leaderArrow = rootMM.Q<Button>("winrate-arrow");
        leaderArrow.RegisterCallback<ClickEvent>(evt =>
        {
            rootMM.Q<VisualElement>("solo-stats").DOMovePercent(Side.Left, 0, -100, 1f, Ease.InBack);
            rootMM.Q<VisualElement>("Leaderboard").DOMovePercent(Side.Left, 100, 0, 1f, Ease.InBack);
        });
        var statsArrow = rootMM.Q<Button>("stat-arrow");
        statsArrow.RegisterCallback<ClickEvent>(evt =>
        {
            rootMM.Q<VisualElement>("solo-stats").DOMovePercent(Side.Left, -100, 0, 1f, Ease.InBack);
            rootMM.Q<VisualElement>("Leaderboard").DOMovePercent(Side.Left, 0, 100, 1f, Ease.InBack);
        });

        var navigationButtons = rootMM.Query<Button>(className: "navigation-button").ToList();
        navigationButtons.AddRange(rootG.Query<Button>(className: "navigation-button").ToList());
        navigationButtons.AddRange(rootS.Query<Button>(className: "navigation-button").ToList());
        foreach (var button in navigationButtons)
        {
            button.RegisterCallback<ClickEvent>(evt =>
            {
                OnSceneChange((Button)evt.target, GetScene());
                AudioManager.SFXPressed("SFXButton");
            });
        }

        var categoryButtons = rootG.Query<Button>(className: "category").ToList();
        foreach (var button in categoryButtons)
        {
            button.RegisterCallback<ClickEvent>(evt =>
            {
                AudioManager.SFXPressed("SFXButton");
                OnCategoryPressed((Button)evt.target);
            });
        }

        var pauseButton = rootG.Q<Button>("pause-button");
        pauseButton.RegisterCallback<ClickEvent>(evt =>
        {
            AudioManager.SFXPressed("SFXButton");
            rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.Flex;
        });

        var statButton = rootMM.Q<Button>("stats-button");
        statButton.RegisterCallback<ClickEvent>(evt =>
        {
            AudioManager.SFXPressed("SFXButton");
            statPanel.SetEnabled(!statPanel.enabledSelf);
        });

        var restartButtons = rootG.Query<Button>("restart-button").ToList();
        foreach (var button in restartButtons)
            button.RegisterCallback<ClickEvent>(evt =>
        {
            AudioManager.SFXPressed("SFXButton");
            rootG.Q<VisualElement>("restart-popup").style.display = DisplayStyle.Flex;
            rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
        });

        var playButton = rootMM.Q<Button>("play-button");
        playButton.RegisterCallback<ClickEvent>(evt =>
        {
            AudioManager.SFXPressed("SFXButton");
            rootMM.Q<VisualElement>("play-popup").style.display = DisplayStyle.Flex;
        });


        var popups = rootG.Query<VisualElement>(className: "popup").ToList();
        popups.AddRange(rootMM.Query<VisualElement>(className: "popup").ToList());
        foreach (var popup in popups)
        {
            popup.RegisterCallback<PointerDownEvent>(evt =>
            {
                popup.style.display = DisplayStyle.None;
            });
        }

        rootMM.Q<Button>("confirm-user").RegisterCallback<ClickEvent>(evt =>
        {
            string displayName = rootMM.Q<TextField>("user-field").value;
            if (displayName != null)
            {
                PlayFabManager.UpdateDisplayName(displayName);
                rootMM.Q<VisualElement>("first-login").style.display = DisplayStyle.None;
                rootMM.Q<Label>("current-user").text = $"Current user: {displayName}";
            }
        });

    }

    private UIDocument GetScene()
    {
        UIDocument scene;
        switch (currentScene)
        {
            case CurrentScene.Main:
                scene = mainMenuUIDocument;
                break;
            case CurrentScene.Game:
                scene = gameUIDocument;
                break;
            case CurrentScene.End:
                scene = endUIDocument;
                break;
            default:
                scene = mainMenuUIDocument;
                break;
        }

        return scene;
    }

    private void Start()
    {
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat("Music", 0.5f);
    }

    private void OnVolumeChange(VisualElement target, float volume)
    {
        if (target == musicSlider)
        {
            AudioManager.Instance.MusicVolume = volume;
        }
        else if (target == sfxSlider)
        {
            AudioManager.Instance.SfxVolume = volume;
        }
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

    private void OnSceneChange(Button target, UIDocument current)
    {
        string targetScene = target.name.Split('-')[0];
        switch (targetScene)
        {
            case "main":
                mainMenuUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
                currentScene = CurrentScene.Main;
                AudioManager.MusicChange("BGMainMenu");
                carouselHandler.SetActive(true);
                break;
            case "mainend":
                mainMenuUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                rootG.Q<VisualElement>("end-panel").style.display = DisplayStyle.None;
                rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.Flex;
                rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
                currentScene = CurrentScene.Main;
                AudioManager.MusicChange("BGMainMenu");
                carouselHandler.SetActive(true);
                break;
            case "game":
                WordManager.Instance.SetNewWord();
                gameUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                statPanel.SetEnabled(false);
                rootMM.Q<VisualElement>("play-popup").style.display = DisplayStyle.None;
                AudioManager.MusicChange("BGGame");
                currentScene = CurrentScene.Game;
                carouselHandler.SetActive(false);
                break;
            case "settings":
                settingsUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                statPanel.SetEnabled(false);
                rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
                carouselHandler.SetActive(false);
                break;
            default:
                mainMenuUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                currentScene = CurrentScene.Main;
                break;
        }

    }

    private void OnCategoryPressed(Button target)
    {
        string category = target.name.Split('-')[0];

        switch (category)
        {
            case "all":
                WordManager.Instance.SetNewWord();
                break;
            case "flower":
                var cat = Category.Flower;
                WordManager.Instance.SetNewWord(cat);
                break;
            case "houseplant":
                cat = Category.Houseplant;
                WordManager.Instance.SetNewWord(cat);
                break;
            case "aromatic":
                cat = Category.Aromatic;
                WordManager.Instance.SetNewWord(cat);
                break;
            default:
                WordManager.Instance.SetNewWord();
                break;
        }
        rootG.Q<VisualElement>("end-panel").style.display = DisplayStyle.None;
        rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.Flex;
        rootG.Q<VisualElement>("restart-popup").style.display = DisplayStyle.None;
    }

    private void OnReturnPressed(UIDocument scene)
    {
        Debug.Log("Return pressed");
        scene.sortingOrder = 5;
        settingsUIDocument.sortingOrder = 0;
        if (scene = mainMenuUIDocument) carouselHandler.SetActive(true);
    }

    private void OnLose()
    {
        rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.None;
        rootG.Q<VisualElement>("end-panel").style.display = DisplayStyle.Flex;
        rootG.Q<Label>("end-message").text = "Better luck next time...";
        rootG.Q<Label>("total-win").text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";
        rootG.Q<Label>("win-rate").text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        rootG.Q<Label>("total-loss").text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
        rootG.Q<Label>("description-label").text = WordManager.Instance.wordSO.values.description;
        rootG.Q<Label>("display-end").text = GetLostWordDisplay(WordManager.Instance.wordToGuess, WordManager.Instance.wordDisplay);
    }

    private void OnWin(Difficulty difficulty)
    {
        rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.None;
        rootG.Q<VisualElement>("end-panel").style.display = DisplayStyle.Flex;
        rootG.Q<Label>("end-message").text = "You won!";
        rootG.Q<Label>("total-win").text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";
        rootG.Q<Label>("win-rate").text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        rootG.Q<Label>("total-loss").text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
        rootG.Q<Label>("display-end").text = new string(WordManager.Instance.wordToGuess);
        rootG.Q<Label>("description-label").text = WordManager.Instance.wordSO.values.description;
    }

    // Method to return a string with unguessed letters in red
    private string GetLostWordDisplay(Char[] wordToGuess, string wordDisplay)
    {
        string lostDisplay = "";
        for (int i = 0; i < wordToGuess.Length; i++)
        {
            if (wordDisplay[i] == '_')
            {
                lostDisplay += $"<color=red>{wordToGuess[i]}</color>";
            }
            else
            {
                lostDisplay += wordToGuess[i];
            }
        }
        return lostDisplay;
    }

    public void GameLoaded()
    {
        rootMM.Q<VisualElement>("loading-panel").style.display = DisplayStyle.None;
        if (PlayFabManager.hasName == true)
        {
            rootMM.Q<Label>("current-user").text = $"Current user: {PlayFabManager.currentUser}";
            rootMM.Q<VisualElement>("first-login").style.display = DisplayStyle.None;
        }
        PlayFabManager.GetLeaderBoard();
    }


    public void DisplayLeaderboard(List<PlayerLeaderboardEntry> leaderboard)
    {
        VisualElement leaderboardPanel = rootMM.Q<VisualElement>("Leaderboard");

        var oldEntries = leaderboardPanel.Query<VisualElement>(className: "leaderboard-entry").ToList();
        foreach (var oldEntry in oldEntries)
        {
            leaderboardPanel.Remove(oldEntry);
        }

        foreach (var entry in leaderboard)
        {
            VisualElement entryElement = leaderboardEntryTemplate.Instantiate();
            entryElement.AddToClassList("leaderboard-entry");
            entryElement.Q<Label>("position").text = (entry.Position + 1).ToString();
            entryElement.Q<Label>("name").text = entry.DisplayName;
            entryElement.Q<Label>("winrate").text = entry.StatValue.ToString() + "%";
            leaderboardPanel.Add(entryElement);
        }
    }

}