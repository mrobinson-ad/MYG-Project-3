using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Settings : MonoBehaviour
{
    public delegate void volumeChangeAction(VisualElement target, float volume);
    public static event volumeChangeAction OnVolumeChange;

    public delegate void difficultyChangeAction();
    public static event difficultyChangeAction OnDifficultyChange;

    public delegate void returnPressedAction(VisualElement target);
    public static event returnPressedAction OnReturnPressed;


    public static void VolumeChange(VisualElement target, float volume)
    {
        OnVolumeChange?.Invoke(target, volume);
    }

    public static void DifficultyChange()
    {
        OnDifficultyChange?.Invoke();
    }
    

    public static void ReturnPressed(VisualElement target)
    {
        OnReturnPressed?.Invoke(target);
    }
}
