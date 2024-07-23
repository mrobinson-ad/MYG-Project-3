using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressCounter : MonoBehaviour
{
    public float maxConsonant = 100;
    public float maxVowel = 100;
    public float maxPower = 100;
    private float maxMinus = 5;

    private float currentConsonant = 0;
    private float currentVowel = 0;
    private float currentPower = 0;
    private float currentMinus = 5;

    [SerializeField] Slider sliderConsonant;
    [SerializeField] Slider sliderVowel;
    [SerializeField] Slider sliderPower;
    [SerializeField] Slider sliderMinus;

    public bool canUsePower;

    private const float TweenDuration = 0.5f;

    public float CurrentConsonant {
         get => currentConsonant;
         set
         {
            currentConsonant = value;
            sliderConsonant.DOValue(Remap.RemapScore(maxConsonant,currentConsonant), TweenDuration);
            if (currentConsonant >= maxVowel)
            {
                //Move to OnProgressFilled(new ItemType.Consonant)
                CurrentConsonant = currentConsonant - maxConsonant; //Add delay before this
                maxConsonant = Mathf.Clamp(maxConsonant * 1.25f,0,250);
                WordRevealer.Instance.RevealConsonant();
            }
         }}
    public float CurrentVowel {
         get => currentVowel;
         set
         {
            currentVowel = value;
            sliderVowel.DOValue(Remap.RemapScore(maxVowel,currentVowel), TweenDuration);
            if (currentVowel >= maxVowel)
            {
                //Move to OnProgressFilled(new ItemType.Vowel)
                CurrentVowel = currentVowel - maxVowel; //Add delay before this
                maxVowel = Mathf.Clamp(maxVowel * 1.25f, 0 ,250);
                WordRevealer.Instance.RevealVowel();
            }

         }}
    public float CurrentPower {
         get => currentPower;
         set
         {
            currentPower = value;
            sliderPower.DOValue(Remap.RemapScore(maxPower,currentPower), TweenDuration);
            if (currentPower >= maxPower)
                canUsePower = true;
            else
                canUsePower = false;
            // add animation
         }}
    public float CurrentMinus {
         get => currentMinus;
         set
         {
            currentMinus = value;
            sliderMinus.DOValue(Remap.RemapScore(maxMinus,currentMinus), TweenDuration);
            if (currentMinus <= 0)
            {
                CurrentMinus = maxMinus; //Add delay before this
                
                //LoseHealth function
            }
         }}

    private void Awake()
    {
        Board.OnIncreaseScore += OnIncreaseScore;
    }

    private void OnIncreaseScore(float score, ItemType type)
    {
        switch (type)
        {
            case ItemType.Consonant:
                CurrentConsonant += score;
                break;
            case ItemType.Vowel:
                CurrentVowel += score;
                break;
            case ItemType.Power:
                CurrentPower += score;
                break;
            case ItemType.Minus:
                CurrentMinus -= score;
                break;
            default:
                CurrentConsonant += score;
                CurrentVowel += score;
                CurrentPower += score/2;
                break;
        }
    }
}
