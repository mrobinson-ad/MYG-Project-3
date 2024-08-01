using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Enum to choose which side's value to Tween
/// </summary>
public enum Side 
{
    Bottom,
    Top,
    Left,
    Right
}

public static class DOTweenExtensions
{
    /// <summary>
    /// Fade to the given value over a duration of time in seconds
    /// </summary>
    /// <param name="target"></param>
    /// <param name="endValue"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static Tweener DOFade(this VisualElement target, float endValue, float duration)
    {
        return DOTween.To(() => (float)target.style.opacity.value, x => target.style.opacity  = new StyleFloat(x), endValue, duration);
    }


    /// <summary>
    /// Shake the visual element over a duration of time in seconds, the shaking effects settings can be set on declaration or take the default values defined here
    /// </summary>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="strength"></param>
    /// <param name="vibrato"></param>
    /// <param name="randomness"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Move a visual element by a given amount (from startValue to endValue), over the course of duration seconds using the easeType
    /// </summary>
    /// <param name="ve"></param>
    /// <param name="side"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="duration"></param>
    /// <param name="easeType"></param>
    /// <returns></returns>
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

        public static Tweener DOMove(this VisualElement ve, Side side, float startValue, float endValue, float duration, Ease easeType) 
    {
        return DOTween.To(() => startValue, x  =>
        {
            startValue = x;
            switch (side) 
            {
                case Side.Bottom:
                    ve.style.bottom  = new Length(x, LengthUnit.Pixel);
                    break;
                case Side.Top:
                    ve.style.top  = new Length(x, LengthUnit.Pixel);
                    break;
                case Side.Left:
                    ve.style.left  = new Length(x, LengthUnit.Pixel);
                    break;
                case Side.Right:
                    ve.style.right  = new Length(x, LengthUnit.Pixel);
                    break;
            }
        }, endValue, duration).SetEase(easeType);
    }

    public static Tweener DOAlpha(this VisualElement ve, float startValue, float endValue, float duration)
    {
        return DOTween.To(() => startValue, x =>
        {
            startValue = x;
            ve.style.unityBackgroundImageTintColor = new Color(1, 1, 1, x);
        }, endValue, duration);
    }

    public static Tweener DORotate(this VisualElement ve, float startValue, float endValue, float duration)
    {
        return DOTween.To(() => startValue, x =>
        {
            startValue = x;
            ve.transform.rotation = Quaternion.Euler(0, 0, x);
        }, endValue, duration);
    }


    public static Tweener DOScale(this VisualElement ve, float endValue, float duration)
    {
        return DOTween.To(() => ve.transform.scale.x, 
                          x => ve.transform.scale = new Vector3(x, x, 1), 
                          endValue, 
                          duration);
    }

}
