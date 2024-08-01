using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace FlowerProject
{
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to the WordList Scriptable object that WordManager uses in order to populate it when Deserializing WordData
        /// </summary>
        public WordList_SO wordList;
        public static GameManager Instance { get; private set; } // Singleton

        public int CommonWins
        {
            get => commonWins;
            set
            {
                commonWins = value;
                totalWins = commonWins + scientificWins;
                UpdateStats("Common");
            }
        }

        public int ScientificWins
        {
            get => scientificWins;
            set
            {
                scientificWins = value;
                totalWins = commonWins + scientificWins;
                UpdateStats("Scientific");
            }
        }

        public int TotalLosses
        {
            get => totalLosses;
            set
            {
                totalLosses = value;
                UpdateStats("Losses");
            }
        }



        private int totalWins;
        private int commonWins;
        private int scientificWins;
        private int totalLosses;
        private int winRate;
        #region Win/Lose events
        public delegate void winAction();
        public static event winAction OnWin;

        public delegate void loseAction();
        public static event loseAction OnLose;

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Win Event

        /// <summary>
        /// Updates the total and category specific wins and saves them to PlayFab
        /// </summary>
        public static void Win() 
        {
            GameManager.Instance.totalWins++;
            Debug.Log($"You won a total of {GameManager.Instance.totalWins} times");
            if (WordManager.Instance.difficulty == Difficulty.Common)
            {
                GameManager.Instance.CommonWins++;
                GameManager.Instance.UpdateStats("Common");
            }
            else if (WordManager.Instance.difficulty == Difficulty.Scientific)
            {
                GameManager.Instance.ScientificWins++;
                GameManager.Instance.UpdateStats("Scientific");
            }
            OnWin?.Invoke();
        }
        #endregion

        #region Lose Event

        /// <summary>
        /// Updates the losses and saves them to PlayFab
        /// </summary>
        public static void Lose() 
        {
            GameManager.Instance.TotalLosses++;
            Debug.Log($"You lost a total of {GameManager.Instance.TotalLosses} times");
            GameManager.Instance.UpdateStats("Losses");
            OnLose?.Invoke();
        }
        #endregion
        /// <summary>
        /// Gets win ratio from totalWins and totalLosses
        /// </summary>
        /// <returns></returns>
        public int GetWinRate()
        {
            float winRatio = (float)totalWins / (totalWins + totalLosses) * 100;
            Debug.Log($"Win Rate: {winRate}");
            winRate = (int)winRatio;
            return winRate;
        }
        
        /// <summary>
        /// Gets total wins from commonWins and ScientificWins
        /// </summary>
        /// <returns></returns>
        public int GetTotalWin()
        {
            totalWins = CommonWins + ScientificWins;
            return totalWins;
        }

        /// <summary>
        /// Deserialize WordData.json into Word_SO scriptable objects and feeds them into wordList
        /// </summary>
        public void DeserializeJson() 
        {
            string filePath = Path.Combine(Application.persistentDataPath, "WordData.json");

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                WordSOContainer wordSOContainer = JsonUtility.FromJson<WordSOContainer>(json);

                // Clear existing list
                wordList.allWords.Clear();

                // Create Word_SO instances and add to WordList_SO
                foreach (var wordData in wordSOContainer.values)
                {
                    Word_SO wordSO = ScriptableObject.CreateInstance<Word_SO>();
                    wordSO.values = wordData;
                    wordSO.name = wordData.common;
                    wordSO.values.UpdateCommonLength();
                    wordSO.values.UpdateScientificLength();
                    wordSO.values.id = System.Guid.NewGuid().ToString();

                    wordList.allWords.Add(wordSO);
                }
                wordList.BuildCategoryLists();  // When all the instances of Word_SO are created, build the lists by category
            }
            else
            {
                Debug.LogError("WordData.json not found at: " + filePath);
            }
        }

        /// <summary>
        /// Updates PlayFabManager PlayerData by giving a single KVP
        /// </summary>
        /// <param name="key"></param>
        private void UpdateStats(string key) 
        {
            var data = new Dictionary<string, string>();
            switch (key)
            {
                case "Common":
                    data["Common"] = commonWins.ToString();
                    PlayFabManager.Instance.SetUserData(data);
                    break;
                case "Scientific":
                    data["Scientific"] = scientificWins.ToString();
                    PlayFabManager.Instance.SetUserData(data);
                    break;
                case "Losses":
                    data["Losses"] = totalLosses.ToString();
                    PlayFabManager.Instance.SetUserData(data);
                    break;
            }
        }
    }
}