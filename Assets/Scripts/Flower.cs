using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

//Handles lives, lose state and flower animations in the future
public class Flower : MonoBehaviour
{
    private VisualElement root;
    public UIDocument gameUIDocument;
    private Label livesDisplay;
    private List<VisualElement> petals;
    private List<Vector2> petalPos;
    public EaseTypeWrapper petalEase;


    private int lives = 7;
    public int Lives //Lives property updates the lives display on change and triggers the Lose event when = 0
    {   
        get => lives;
        set {
            if (lives > value)
            LoseRandomPetal();
            lives = Mathf.Clamp(value, 0, 7);
            livesDisplay.text = lives.ToString();
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
        foreach (var petal in petals)
        {
            petalPos.Add(petal.layout.position);
        }
    }

    private void LoseRandomPetal()
    {
        int randomIndex = Random.Range(0, petals.Count);
        VisualElement petal = petals[randomIndex];
        float startPos = petal.layout.position.y;
        petal.DOMove(Side.Top, startPos, 400, 3f, petalEase.easeType);
        petals.RemoveAt(randomIndex);
    }

    private void ResetPetals()
    {
        petals.Clear();
        petals.AddRange(root.Query<VisualElement>(className: "petal").ToList());
        int i = 0;
        foreach (var petal in petals)
        {
            petals[i].style.left = petalPos[i].x;
            petals[i].style.top = petalPos[i].y;
            i++;
        }
    }
}
