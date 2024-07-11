using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public enum Side // Enum to choose which side's value to Tween
{
    Bottom,
    Top,
    Left,
    Right
}

public static class DOTweenExtensions
{
    // Fade to the given value over a duration of time in seconds
    public static Tweener DOFade(this VisualElement target, float endValue, float duration)
    {
        return DOTween.To(() => (float)target.style.opacity.value, x => target.style.opacity  = new StyleFloat(x), endValue, duration);
    }

    // Shake the visual element over a duration of time in seconds, the shaking effects settings can be set on declaration or take the default values defined here
    public static Tweener DOShake(this VisualElement target, float duration, float strength  = 10f, int vibrato  = 10, float randomness  = 90f)
    {
        Vector3 originalPosition  = target.transform.position;

        return DOTween.Shake(() => target.transform.position, 
                             x => target.transform.position  = x, 
                             duration, 
                             strength, 
                             vibrato, 
                             randomness)
                       .OnComplete(() => target.transform.position  = originalPosition); // Ensure the element returns to its original position
    }

    // Move a visual element by a given amount (from startValue to endValue), over the course of duration seconds using the easeType
    public static Tweener DOMovePercent(this VisualElement ve, Side side, float startValue, float endValue, float duration, Ease easeType) 
    {
        return DOTween.To(() => startValue, x  =>
        {
            startValue = x;
            switch (side) 
            {
                case Side.Bottom:
                    ve.style.bottom  = new Length(x, LengthUnit.Percent);
                    break;
                case Side.Top:
                    ve.style.top  = new Length(x, LengthUnit.Percent);
                    break;
                case Side.Left:
                    ve.style.left  = new Length(x, LengthUnit.Percent);
                    break;
                case Side.Right:
                    ve.style.right  = new Length(x, LengthUnit.Percent);
                    break;
            }
        }, endValue, duration).SetEase(easeType);
    }

}
