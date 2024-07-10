using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton

    private int totalWins;
    private int commonWins;
    private int scientificWins;
    private int totalLosses;

    public delegate void winAction(Difficulty difficulty);
    public static event winAction OnWin;

    public delegate void loseAction();
    public static event loseAction OnLose;

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

    public static void Win(Difficulty difficulty)
    {
        OnWin?.Invoke(difficulty);
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
    }

    public static void Lose()
    {
        OnLose?.Invoke();
        GameManager.Instance.totalLosses++;
        Debug.Log($"You lost a total of {GameManager.Instance.totalLosses} times");
        PlayerPrefs.SetInt("TotalLosses", GameManager.Instance.totalLosses);
    }
}
