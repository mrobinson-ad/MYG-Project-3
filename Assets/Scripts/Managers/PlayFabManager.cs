using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.IO;
using System;
using Unity.VisualScripting;

namespace FlowerProject
{
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance { get; private set; }
        public delegate void updateDisplayName(string displayName);
        public static event updateDisplayName OnUpdateDisplayName;

        public static bool hasName;
        public static string currentUser;

        private static string currentID;


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

            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
        }

        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Login successful");
            if (result.InfoResultPayload.PlayerProfile != null)
            {
                currentUser = result.InfoResultPayload.PlayerProfile.DisplayName;
                currentID = result.PlayFabId;
                hasName = true;

            }
            else
                hasName = false;
            GetWordData();
            UIManager.Instance.GameLoaded();
            GetUserData(currentID);
        }

        private static void OnError(PlayFabError error)
        {
            Debug.Log("The following error has occured:");
            Debug.LogError(error.GenerateErrorReport());
        }

        public static void UpdateDisplayName(string displayName)
        {
            OnUpdateDisplayName?.Invoke(displayName);
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = displayName
            };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
        }

        private static void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log("You successfuly set the display name to " + result.DisplayName);
        }

        public static void UpdateWinRate(int winRate)
        {
            PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate> {
            new StatisticUpdate{
                StatisticName = "WinRate",
                Value = winRate
            }
        }
            }, result => OnStatisticUpdated(result), OnError);
        }

        private static void OnStatisticUpdated(UpdatePlayerStatisticsResult updateResult)
        {
            UIManager.Instance.StartCoroutine(WaitAndUpdateLeaderboard(3f));
        }

        public static void GetLeaderBoard()
        {
            PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
            {
                StatisticName = "WinRate",
                StartPosition = 0,
                MaxResultsCount = 5
            }, result => UIManager.Instance.DisplayLeaderboard(result.Leaderboard), OnError);
        }

        private static IEnumerator WaitAndUpdateLeaderboard(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            GetLeaderBoard();
        }

        private void GetWordData()
        {
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OnGetWordData, OnError);
        }

        /// <summary>
        /// If WordData.json doesn't exist or is different from the WordData we get from TitleData, create wordData.json
        /// </summary>
        /// <param name="result"></param>
        private void OnGetWordData(GetTitleDataResult result)
        {
            if (result.Data == null || !result.Data.ContainsKey("WordData"))
            {
                Debug.Log("No word data found");
                return;
            }
            if (!File.Exists(Application.persistentDataPath + "/WordData.json") || File.ReadAllText(Application.persistentDataPath + "/WordData.json") != result.Data["WordData"])
            {
                string wordjson = result.Data["WordData"];
                File.WriteAllText(Application.persistentDataPath + "/WordData.json", wordjson);
            }
            GameManager.Instance.DeserializeJson(); // Starts deserialization so the data can be used
        }

        public void SetUserData(Dictionary<string, string> data)
        {
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = data
            },
            result => Debug.Log("User data updated successfuly"), OnError); //Add event to update stats display
        }

        public void GetUserData(string playFabID)
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest()
            {
                PlayFabId = playFabID,
                Keys = null
            },
            result => LocalUserData(result), OnError);
        }

        /// <summary>
        /// Sets the user's win/loss data in GameManager with the values stored in their PlayFab PlayerData and sets them to 0 if the result is null
        /// </summary>
        /// <param name="result"></param>
        private void LocalUserData(GetUserDataResult result) 
        {
            if (result.Data == null)
            {
                GameManager.Instance.ScientificWins = 0;
                GameManager.Instance.CommonWins = 0;
                GameManager.Instance.TotalLosses = 0;
            }
            else
            {
                GameManager.Instance.ScientificWins = int.Parse(result.Data["Scientific"].Value);
                GameManager.Instance.CommonWins = int.Parse(result.Data["Common"].Value);
                GameManager.Instance.TotalLosses = int.Parse(result.Data["Losses"].Value);
            }

        }

    }
}