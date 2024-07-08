using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PreviousScene
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
    private GroupBox difficultyBox;

    private void Awake()
    {
        rootMM = mainMenuUIDocument.rootVisualElement;
        rootG = gameUIDocument.rootVisualElement;
        rootS = settingsUIDocument.rootVisualElement;
        difficultyBox = rootS.Q<GroupBox>("difficulty-box");
        musicSlider = rootS.Q<Slider>("music-slider");
        sfxSlider = rootS.Q<Slider> ("sfx-slider");
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

}