using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.IO;
using System;
using PlayFab.MultiplayerModels;
using Unity.VisualScripting;

public class PlayFabManager : MonoBehaviour
{

    public delegate void updateDisplayName(string displayName);
    public static event updateDisplayName OnUpdateDisplayName;

    public static bool hasName;
    public static string currentUser;

   
    private void Awake()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams{
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
            hasName = true;
        }
        else
            hasName = false;
        GetWordData();
        UIManager.Instance.GameLoaded();
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
        Debug.Log($"{currentUser} has submitted a new win rate of {updateResult}");
        UIManager.Instance.StartCoroutine(WaitAndUpdateLeaderboard(3f));
    }

    public static void GetLeaderBoard()
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
        {
            StatisticName = "WinRate",
            StartPosition = 0,
            MaxResultsCount = 3
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
    // if WordData.json doesn't exist or is different from the WordData we get from TitleData, create wordData.json
    private void OnGetWordData(GetTitleDataResult result)
    {
        if (result.Data == null || !result.Data.ContainsKey("WordData"))
        {
            Debug.Log("No word data found");
            return;
        }
        if (!File.Exists(Application.persistentDataPath + "/WordData.json")||File.ReadAllText(Application.persistentDataPath + "/WordData.json") != result.Data["WordData"])
        {
            string wordjson = result.Data["WordData"];
            File.WriteAllText(Application.persistentDataPath + "/WordData.json", wordjson);
        }
        GameManager.Instance.DeserializeJson(); // starts deserialization so the data can be used
    }

}