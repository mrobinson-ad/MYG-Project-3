using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace FlowerProject
{
    public class MatchManager : MonoBehaviour
    {
        public UIDocument uiDocGame;

        private VisualElement root;

        public delegate void restartMatch3();
        public static event restartMatch3 OnRestartMatch3;


        private void Awake()
        {
            root = uiDocGame.rootVisualElement;
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {

            var pauseButton = root.Q<Button>("pause-button");
            pauseButton.RegisterCallback<ClickEvent>(evt =>
            {
                AudioManager.SFXPressed("SFXButton");
                root.Q<VisualElement>("pause-popup").style.display = DisplayStyle.Flex;
                uiDocGame.panelSettings.sortingOrder = 5;
            });


            var popup = root.Q<VisualElement>(className: "popup");
            popup.RegisterCallback<PointerDownEvent>(evt =>
            {
                popup.style.display = DisplayStyle.None;
                uiDocGame.panelSettings.sortingOrder = 0;
            });

            var restartButton = root.Q<Button>("restart-button");
            restartButton.RegisterCallback<ClickEvent>(evt =>
            {
                popup.style.display = DisplayStyle.None;
                uiDocGame.panelSettings.sortingOrder = 0;
                OnRestartMatch3?.Invoke();
            }
            );

            var mainButton = root.Q<Button>("main-button");
            mainButton.RegisterCallback<ClickEvent>(evt =>
            {
                popup.style.display = DisplayStyle.None;
                uiDocGame.panelSettings.sortingOrder = 0;
                SceneManager.LoadScene("Hangflower");
            });
        }

    }
}