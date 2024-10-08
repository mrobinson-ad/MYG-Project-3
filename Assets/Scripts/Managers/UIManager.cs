using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using PlayFab.ClientModels;
using DG.Tweening;
using UnityEngine.SceneManagement;


namespace FlowerProject
{
    /// <summary>
    /// Used to define the UIDocument the user is currently on, mainly for navigation
    /// </summary>
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
        private VisualElement statPanel;
        private CurrentScene currentScene = CurrentScene.Main;
        public GameObject carouselHandler;
        public VisualTreeAsset leaderboardEntryTemplate;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            rootG = gameUIDocument.rootVisualElement;
            rootS = settingsUIDocument.rootVisualElement;
            rootMM = mainMenuUIDocument.rootVisualElement;
            difficultyBox = rootS.Q<GroupBox>("difficulty-box");
            musicSlider = rootS.Q<Slider>("music-slider");
            sfxSlider = rootS.Q<Slider>("sfx-slider");
            exit = rootS.Q<Button>("return-button");
            statPanel = rootMM.Q<VisualElement>("stat-panel");
            rootG.Q<VisualElement>("end-panel").SetEnabled(false);
            statPanel.SetEnabled(false);
            Settings.OnDifficultyChange += OnDifficultyChanged;
            Settings.OnVolumeChange += OnVolumeChange;
            Settings.OnReturnPressed += OnReturnPressed;
            GameManager.OnLose += OnLose;
            GameManager.OnWin += OnWin;
            RegisterCallBacks();
        }

        /// <summary>
        /// Returns the current scene UIDocument based on the currentScene enum
        /// </summary>
        /// <returns></returns>
        public UIDocument GetScene() 
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
                default:
                    scene = mainMenuUIDocument;
                    break;
            }

            return scene;
        }



        #region RegisterCallBacks
        private void RegisterCallBacks() // Forgive me for I have sinned
        {
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
                    StartCoroutine(OnCategoryPressed((Button)evt.target));
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
        #endregion


        private void Start()
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);
            musicSlider.value = PlayerPrefs.GetFloat("Music", 0.5f);
        }

        /// <summary>
        /// Changes the volume in AudioManager when the slider's value is changed
        /// </summary>
        /// <param name="target"></param>
        /// <param name="volume"></param>
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

        /// <summary>
        /// Sets WordManager Difficulty enum by parsing the label of the radio-difficulty RBs
        /// </summary>
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
        #region Scene Change

        /// <summary>
        /// Sets the target scene sortingOrder to the front and the current one to 0 and handles scene specific actions on change
        /// </summary>
        /// <param name="target"></param>
        /// <param name="current"></param>
        public void OnSceneChange(Button target, UIDocument current) 
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
                    rootG.Q<VisualElement>("end-panel").SetEnabled(false);
                    rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.Flex;
                    WordManager.Instance.wordSO = null;
                    Debug.Log("MainEnd triggered");
                    mainMenuUIDocument.sortingOrder = 5;
                    current.sortingOrder = 0;
                    rootG.Q<VisualElement>("pause-popup").style.display = DisplayStyle.None;
                    currentScene = CurrentScene.Main;
                    AudioManager.MusicChange("BGMainMenu");
                    carouselHandler.SetActive(true);
                    break;
                case "match":
                    statPanel.SetEnabled(false);
                    rootMM.Q<VisualElement>("play-popup").style.display = DisplayStyle.None;
                    AudioManager.MusicChange("BGGame");
                    currentScene = CurrentScene.Game;
                    carouselHandler.SetActive(false);
                    SceneManager.LoadScene("MatchFlower");
                    break;
                case "game":
                    if (WordManager.Instance.wordSO == null)
                    {
                        WordManager.Instance.SetNewWord();
                        rootG.Q<VisualElement>("sakura").transform.scale = new Vector3(0, 0, 1);
                    }
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
        #endregion

        #region Restart

        /// <summary>
        /// Method that handles UI changes and animations when restarting from the end screen, also sets a new word depending on the category chosen
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private IEnumerator OnCategoryPressed(Button target) 
        {
            var endStats = rootG.Q<VisualElement>("end-stats-panel");
            endStats.RemoveFromClassList("win-stats-panels");
            endStats.RemoveFromClassList("lose-stats-panels");
            rootG.Q<VisualElement>("restart-popup").style.display = DisplayStyle.None;
            string category = target.name.Split('-')[0];
            var rightCurtain = rootG.Q<VisualElement>("curtain-right");
            var leftCurtain = rootG.Q<VisualElement>("curtain-left");
            RemoveAddUSSClass(rightCurtain, "curtain-right-off", "curtain-right");
            RemoveAddUSSClass(leftCurtain, "curtain-left-off", "curtain-left");
            yield return new WaitForSeconds(1.5f);
            rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.Flex;
            rootG.Q<VisualElement>("end-panel").SetEnabled(false);
            rootG.Q<VisualElement>("sakura").transform.scale = new Vector3(0, 0, 1);
            yield return new WaitForSeconds(1.5f);
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
            RemoveAddUSSClass(rightCurtain, "curtain-right", "curtain-right-off");
            RemoveAddUSSClass(leftCurtain, "curtain-left", "curtain-left-off");
            AudioManager.MusicChange("BGGame");
        }

        #endregion
        #region Event Listeners

        /// <summary>
        /// Listens to the ReturnPressed Event and brings the previous screen forward and settings screen back
        /// </summary>
        /// <param name="scene"></param>
        private void OnReturnPressed(UIDocument scene) 
        {
            Debug.Log("Return pressed");
            scene.sortingOrder = 5;
            settingsUIDocument.sortingOrder = 0;
            if (scene = mainMenuUIDocument) carouselHandler.SetActive(true);
        }

        /// <summary>
        /// Triggers with the Lose event and applies the relevant UI changes and stats changes
        /// </summary>
        private void OnLose()
        {
            AudioManager.MusicChange("BGLose");
            rootG.Q<VisualElement>("sakura").transform.scale = new Vector3(0, 0, 1);
            rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.None;
            rootG.Q<VisualElement>("end-panel").SetEnabled(true);
            RemoveAddUSSClass(rootG.Q<VisualElement>("end-stats-panel"), "win-stats-panel", "lose-stats-panel");
            rootG.Q<Label>("end-message").text = "Better luck next time...";
            rootG.Q<Label>("total-win").text = $"Total wins: {GameManager.Instance.GetTotalWin()}";
            rootG.Q<Label>("win-rate").text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
            rootG.Q<Label>("total-loss").text = $"Total Losses: {GameManager.Instance.TotalLosses}";
            rootG.Q<Label>("description-label").text = WordManager.Instance.wordSO.values.description;
            rootG.Q<Label>("display-end").text = GetLostWordDisplay(WordManager.Instance.wordToGuess, WordManager.Instance.wordDisplay);
        }
        /// <summary>
        /// Triggers with the Win event and applies the relevant UI changes and stats changes
        /// </summary>
        private void OnWin()
        {
            AudioManager.MusicChange("BGWin");
            rootG.Q<VisualElement>("display-panel").style.display = DisplayStyle.None;
            RemoveAddUSSClass(rootG.Q<VisualElement>("end-stats-panel"), "lose-stats-panel", "win-stats-panel");
            DOTween.Sequence().PrependInterval(1.5f).Append(rootG.Q<VisualElement>("sakura").DOScale(1, 2).SetEase(Ease.InExpo)).Play();
            rootG.Q<VisualElement>("end-panel").SetEnabled(true);
            rootG.Q<Label>("end-message").text = "You won!";
            rootG.Q<Label>("total-win").text = $"Total wins: {GameManager.Instance.GetTotalWin()}";
            rootG.Q<Label>("win-rate").text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
            rootG.Q<Label>("total-loss").text = $"Total Losses: {GameManager.Instance.TotalLosses}";
            rootG.Q<Label>("display-end").text = new string(WordManager.Instance.wordToGuess);
            rootG.Q<Label>("description-label").text = WordManager.Instance.wordSO.values.description;
        }
        #endregion

        /// <summary>
        /// Method to return a string with unguessed letters in red
        /// </summary>
        /// <param name="wordToGuess"></param>
        /// <param name="wordDisplay"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Hides loading panel and disables the first login panel if user has already registered a name
        /// </summary>
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

        /// <summary>
        /// Displays the leaderboard using a template and the data from playfab's leaderboard
        /// </summary>
        /// <param name="leaderboard"></param>
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

        /// <summary>
        /// Util to remove and add a class to a Visual Element in one line
        /// </summary>
        /// <param name="target"></param>
        /// <param name="classToRemove"></param>
        /// <param name="classToAdd"></param>
        private void RemoveAddUSSClass(VisualElement target, string classToRemove, string classToAdd) 
        {
            target.RemoveFromClassList(classToRemove);
            target.AddToClassList(classToAdd);
        }

    }
}