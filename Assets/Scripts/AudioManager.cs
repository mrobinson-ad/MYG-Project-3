using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set;} // Singleton
    // The Volume properties use log(i)*20 to get a value from -80 to 0 in order to set the attenuation in the mixer and saves the pre log value to PlayerPrefs
    public float MusicVolume
    {   
        get => musicVolume;
        set {
            musicVolume = Mathf.Log(value)*20; 
            PlayerPrefs.SetFloat("Music", value);
            audioMixer.SetFloat("Music",musicVolume);
            }
    }
    public float SfxVolume
    {   
        get => sfxVolume;
        set {
            sfxVolume = Mathf.Log(value)*20;
            PlayerPrefs.SetFloat("SFX", value);
            audioMixer.SetFloat("SFX",sfxVolume);
            }
    }
    public AudioMixer audioMixer;
    private AudioClip audioClip;
    private GameObject audioPoolObject;

    private float musicVolume;
    private float sfxVolume;

    private void Awake() // Gets the saved volume settings from playerprefs with a 0.5f default
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.5f); 
       
        SfxVolume = PlayerPrefs.GetFloat("SFX", 0.5f);
        
    }

    private void Start() // Sets the audioMixer volume to the saved volume settings
    {
        audioMixer.SetFloat("Music",MusicVolume);
        Debug.Log("the music volume is" + MusicVolume);
        audioMixer.SetFloat("SFX",SfxVolume);
        Debug.Log("the SFX volume is" + SfxVolume);
    }
}
