using UnityEngine;
using UnityEngine.UIElements;

public class StatsPanelController : MonoBehaviour // Initially sets the win/lose stats and updates them when OnWin or OnLose are called
{
    
    Label totalWins;
    Label commonWins;
    Label scientificWins;
    Label totalLosses;        
    private void Awake() 
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        totalWins = root.Q<Label>("total-win");
        commonWins = root.Q<Label>("common-win");
        scientificWins = root.Q<Label>("scientific-win");
        totalLosses = root.Q<Label>("total-loss");
        GameManager.OnWin += UpdateWins;
        GameManager.OnLose += UpdateLoss; 
    }
    
    void Start()
    {        
        totalWins.text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";

        commonWins.text = $"Common wins: {PlayerPrefs.GetInt("CommonWins", 0)}";
        
        scientificWins.text = $"Scientific wins: {PlayerPrefs.GetInt("ScientificWins", 0)}";

        totalLosses.text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
    }

    private void UpdateWins(Difficulty difficulty)
    {
        totalWins.text = $"Total wins: {PlayerPrefs.GetInt("TotalWins", 0)}";
        if (difficulty == Difficulty.Common)
        {
            commonWins.text = $"Common wins: {PlayerPrefs.GetInt("CommonWins", 0)}";
        }
        else
        {
            scientificWins.text = $"Scientific wins: {PlayerPrefs.GetInt("ScientificWins", 0)}";
        }
    }

    private void UpdateLoss()
    {
        totalLosses.text = $"Total Losses: {PlayerPrefs.GetInt("TotalLosses", 0)}";
    }
}