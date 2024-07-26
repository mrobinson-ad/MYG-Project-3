using UnityEngine;
using UnityEngine.UIElements;

public class StatsPanelController : MonoBehaviour // Initially sets the win/lose stats and updates them when OnWin or OnLose are called
{
    
    Label totalWins;
    Label commonWins;
    Label scientificWins;
    Label totalLosses;
    Label winRate;
  
    private void Awake() 
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        totalWins = root.Q<Label>("total-win");
        commonWins = root.Q<Label>("common-win");
        scientificWins = root.Q<Label>("scientific-win");
        totalLosses = root.Q<Label>("total-loss");
        winRate = root.Q<Label>("win-rate");
        GameManager.OnWin += UpdateWins;
        GameManager.OnLose += UpdateLoss; 
    }
    
    void Start()
    {        
        totalWins.text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";

        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";

        commonWins.text = $"Common wins: {PlayerPrefs.GetInt("CommonWins", 0)}";
        
        scientificWins.text = $"Scientific wins: {PlayerPrefs.GetInt("ScientificWins", 0)}";

        totalLosses.text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
    }

    private void UpdateWins()
    {
        totalWins.text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";
        
        if (WordManager.Instance.difficulty == Difficulty.Common)
        {
            commonWins.text = $"Common wins: {PlayerPrefs.GetInt("CommonWins", 0)}";
        }
        else
        {
            scientificWins.text = $"Scientific wins: {PlayerPrefs.GetInt("ScientificWins", 0)}";
        }
        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        PlayFabManager.UpdateWinRate(GameManager.Instance.GetWinRate());    
    }

    private void UpdateLoss()
    {
        totalLosses.text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        PlayFabManager.UpdateWinRate(GameManager.Instance.GetWinRate());
    }
}