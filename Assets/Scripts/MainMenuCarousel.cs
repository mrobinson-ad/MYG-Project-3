using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuCarousel : MonoBehaviour
{
    private VisualElement root;
    public UIDocument mainMenuUIDocument;
    private VisualElement carousel;
    public EaseTypeWrapper carouselEase;
    float startPos = 500f;
    float endPos = 400f;
    private void Start()
    {
        root = mainMenuUIDocument.rootVisualElement;
        carousel = root.Q<VisualElement>("carousel");
        StartCarousel();
            
    }

    private void StartCarousel()
    {
        // Create a sequence
        var sequence = DOTween.Sequence()
            .SetLink(this.gameObject, LinkBehaviour.PauseOnDisablePlayOnEnable)
            .AppendInterval(3);

        sequence.Append(carousel.DOMovePercent(Side.Bottom, startPos, endPos, 2f, carouselEase.easeType))
            .OnComplete(() =>
            {
                startPos -= 100f;
                endPos -= 100f;

                if (endPos < 0f)
                {
                    startPos = 0f;
                    endPos = 500f;
                    RestartCarousel();
                }
                else
                {
                    StartCarousel();
                }
            });
    }

    private void RestartCarousel()
    {
        DOTween.Sequence()
            .AppendInterval(3)
            .Append(carousel.DOMovePercent(Side.Bottom, startPos, endPos, 3f, carouselEase.easeType))
            .OnComplete(() =>
            {
                startPos = 500f;
                endPos = 400f;
                StartCarousel();
            });
    }
}
