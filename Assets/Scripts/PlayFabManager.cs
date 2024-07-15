using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

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
}
