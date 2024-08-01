using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlowerProject
{
    /// <summary>
    /// Initially sets the win/lose stats and updates them when OnWin or OnLose are called
    /// </summary>
    public class StatsPanelController : MonoBehaviour 
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
            StartCoroutine(SetInitialStats());
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

        /// <summary>
        /// Delays the InitialStats update to allow time for the playfab data to be fetched
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetInitialStats()
        {
            yield return new WaitForSeconds(4);
            totalWins.text = $"Total wins: {GameManager.Instance.GetTotalWin()}";

            winRate.text = $"Win rate: {GameManager.Instance.GetWinRate()}%";

            commonWins.text = $"Common wins: {GameManager.Instance.CommonWins}";

            scientificWins.text = $"Scientific wins: {GameManager.Instance.ScientificWins}";

            totalLosses.text = $"Total Losses: {GameManager.Instance.TotalLosses}";
        }
    }
}