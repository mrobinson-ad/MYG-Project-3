using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    private Slider musicSlider;
    private Slider sfxSlider;
    private Button exit;
    private GroupBox difficultyBox;
    private CurrentScene currentScene = CurrentScene.Main;

    private void Awake()
    {
        rootMM = mainMenuUIDocument.rootVisualElement;
        rootG = gameUIDocument.rootVisualElement;
        rootS = settingsUIDocument.rootVisualElement;
        difficultyBox = rootS.Q<GroupBox>("difficulty-box");
        musicSlider = rootS.Q<Slider>("music-slider");
        sfxSlider = rootS.Q<Slider> ("sfx-slider");
        exit = rootS.Q<Button>("return-button");
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
        Settings.OnVolumeChange += OnVolumeChange;
        RegisterCallBacks();
    }

    private void RegisterCallBacks()
    {
        // MouseCaptureOutEvent is called when the user stops interacting with the slider
        musicSlider.RegisterCallback<MouseCaptureOutEvent>(evt =>
        {
            Settings.VolumeChange(musicSlider, musicSlider.value);
        });

        sfxSlider.RegisterCallback<MouseCaptureOutEvent>(evt =>
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
            
            Settings.ReturnPressed(GetScene());
        });

        var buttons = rootMM.Query<Button>(className:"navigation-button").ToList();
        buttons.AddRange(rootG.Query<Button>(className:"navigation-button").ToList());
        buttons.AddRange(rootS.Query<Button>(className:"navigation-button").ToList());
        foreach (var button in buttons)
        {
            button.RegisterCallback<ClickEvent>(evt => OnSceneChange((Button)evt.target, GetScene()));
        }

        var pauseButton = rootG.Q<Button>("pause-button");
        pauseButton.RegisterCallback<ClickEvent>(evt =>
        {
            rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.Flex;
        });

        var restartButton = rootG.Q<Button>("restart-button");
        restartButton.RegisterCallback<ClickEvent>(evt =>
        {
            rootG.Q<VisualElement>("restart-popup").style.display = DisplayStyle.Flex;
            rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
        });

        var playButton = rootMM.Q<Button>("play-button");
        playButton.RegisterCallback<ClickEvent>(evt =>
        {
            rootMM.Q<VisualElement>("play-popup").style.display = DisplayStyle.Flex;
        });

        var popups = rootG.Query<VisualElement>(className:"popup").ToList();
        popups.AddRange(rootMM.Query<VisualElement>(className: "popup").ToList());
        foreach (var popup in popups)
        {
            popup.RegisterCallback<PointerDownEvent>(evt =>
            {
                popup.style.display = DisplayStyle.None;
            });
        }

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
                scene = mainMenuUIDocument;
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
            Debug.Log($"Music volume set to: {volume}");
        }
        else if (target == sfxSlider)
        {
            AudioManager.Instance.SfxVolume = volume;
            Debug.Log($"SFX volume set to: {volume}");
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
                currentScene = CurrentScene.Main;
                break;
            case "game":
                WordManager.Instance.SetNewWord();
                gameUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                currentScene = CurrentScene.Game;
                break;
            case "settings":
                settingsUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                break;
            //case "end":
            default:
                mainMenuUIDocument.sortingOrder = 5;
                current.sortingOrder = 0;
                currentScene = CurrentScene.Main;
                break;
        }

    }

    private void OnReturnPressed(UIDocument scene)
    {
        scene.sortingOrder = 5;
        settingsUIDocument.sortingOrder = 0;
    }

}