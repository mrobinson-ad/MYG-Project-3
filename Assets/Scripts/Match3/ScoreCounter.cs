
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    public static ScoreCounter Instance { get; private set; }

    private int _score;

    public int Score
    {
        get => _score;

        set
        {
            if (_score == value) return;

            _score = value;

            scoreText.text = ($"Score = {_score}");
        }
    }

    [SerializeField] private Text scoreText;
    private void Awake() => Instance = this;
}
