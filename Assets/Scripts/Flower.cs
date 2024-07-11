using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

//Handles lives, lose state and flower animations in the future
public class Flower : MonoBehaviour
{
    private VisualElement root;
    public UIDocument gameUIDocument;
    private Label livesDisplay;

    private int lives = 7;
    public int Lives //Lives property updates the lives display on change and triggers the Lose event when = 0
    {   
        get => lives;
        set {
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
    }
}
