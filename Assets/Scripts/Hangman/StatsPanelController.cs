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
        totalWins.text = $"Total wins: {GameManager.Instance.GetTotalWin()}";

        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";

        commonWins.text = $"Common wins: {GameManager.Instance.CommonWins}";
        
        scientificWins.text = $"Scientific wins: {GameManager.Instance.ScientificWins}";

        totalLosses.text = $"Total Losses: {GameManager.Instance.TotalLosses}";
    }

    private void UpdateWins()
    {
        totalWins.text = $"Total wins: {GameManager.Instance.GetTotalWin()}";
        
        if (WordManager.Instance.difficulty == Difficulty.Common)
        {
            commonWins.text = $"Common wins: {GameManager.Instance.CommonWins}";
        }
        else
        {
            scientificWins.text = $"Scientific wins: {GameManager.Instance.ScientificWins}";
        }
        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        PlayFabManager.UpdateWinRate(GameManager.Instance.GetWinRate());    
    }

    private void UpdateLoss()
    {
        totalLosses.text = $"Total Losses: {GameManager.Instance.TotalLosses}";
        winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";
        PlayFabManager.UpdateWinRate(GameManager.Instance.GetWinRate());
    }
}