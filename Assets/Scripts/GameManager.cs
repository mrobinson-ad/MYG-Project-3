using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton
    #region Win/Lose events
    private int totalWins;
    private int commonWins;
    private int scientificWins;
    private int totalLosses;

    public delegate void winAction(Difficulty difficulty);
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
        totalWins = PlayerPrefs.GetInt("TotalWins", 0);
        commonWins = PlayerPrefs.GetInt("CommonWins", 0);
        scientificWins = PlayerPrefs.GetInt("ScientificWins", 0);
        totalLosses = PlayerPrefs.GetInt("TotalLosses", 0);

    }
    #region Win Event
    public static void Win(Difficulty difficulty) // updates the total and category specific wins and saves them to PlayerPrefs
    {
        GameManager.Instance.totalWins++;
        Debug.Log($"You won a total of {GameManager.Instance.totalWins} times");
        PlayerPrefs.SetInt("TotalWins", GameManager.Instance.totalWins);
        if (difficulty == Difficulty.Common)
        {
            GameManager.Instance.commonWins++;
            PlayerPrefs.SetInt("CommonWins", GameManager.Instance.commonWins);
        } else if (difficulty == Difficulty.Scientific)
        {
            GameManager.Instance.scientificWins++;
            PlayerPrefs.SetInt("ScientificWins", GameManager.Instance.scientificWins);
        }
        OnWin?.Invoke(difficulty);
    }
    #endregion

    #region Lose Event
    public static void Lose() // updates the losses and saves them to PlayerPrefs
    {
        GameManager.Instance.totalLosses++;
        Debug.Log($"You lost a total of {GameManager.Instance.totalLosses} times");
        PlayerPrefs.SetInt("TotalLosses", GameManager.Instance.totalLosses);
        OnLose?.Invoke();
    }
    #endregion 
}
