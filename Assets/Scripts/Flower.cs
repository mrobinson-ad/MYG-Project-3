using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

// Handles lives, lose state, and flower animations in the future
public class Flower : MonoBehaviour
{
    private VisualElement root;
    public UIDocument gameUIDocument;
    private Label livesDisplay;
    private List<VisualElement> petals;
    private List<Vector2> petalPos;
    public AnimationCurve easePetal;

    private int lives = 7;
    public int Lives // Lives property updates the lives display on change and triggers the Lose event when = 0
    {
        get => lives;
        set
        {
            if (lives > value)
                LoseRandomPetal();
            lives = Mathf.Clamp(value, 0, 7);
            livesDisplay.text = lives.ToString();
            if (lives == 7)
                ResetPetals();
            if (lives <= 0)
            {
                GameManager.Lose();
                Debug.Log("Game Over");
            }
        }
    }

    private void Awake()
    {
        root = gameUIDocument.rootVisualElement;
        livesDisplay = root.Q<Label>("lives");
        petals = new List<VisualElement>();
        petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
        petalPos = new List<Vector2>();
        StartCoroutine(CaptureInitialPositions());
    }

    private IEnumerator CaptureInitialPositions()
    {
        yield return new WaitForEndOfFrame();

        foreach (var petal in petals)
        {
            petalPos.Add(new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top));
        }
    }

    private void LoseRandomPetal()
    {
        int randomIndex = Random.Range(0, petals.Count);
        VisualElement petal = petals[randomIndex];
        Vector2 startPos = new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top);
        Vector3[] endValues = new[]
        {
            new Vector3(startPos.x - 100, startPos.y + 100),
            new Vector3(startPos.x +100, startPos.y + 200),
             new Vector3(startPos.x -100, startPos.y + 300),
            new Vector3(startPos.x , startPos.y + 400)
        };

        float[] durations = new[] { 1.5f, 1.5f, 1.5f, 1.5f };

        Tween t = DOTween.ToArray(
            () => new Vector2(petal.resolvedStyle.left, petal.resolvedStyle.top),
            x =>
            {
                petal.style.left = x.x;
                petal.style.top = x.y;
            },
            endValues,
            durations
        );
        t.SetEase(easePetal);
        petals.RemoveAt(randomIndex);
    }

    private void ResetPetals()
    {
        petals.Clear();
        petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
        int i = 0;
        foreach (var petal in petals)
        {
            petal.style.left = petalPos[i].x;
            petal.style.top = petalPos[i].y;
            i++;
        }
    }
}