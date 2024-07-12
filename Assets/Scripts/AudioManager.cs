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
    public BGMusic_SO[] backgroundMusics;
    public SFX_SO[] soundEffects;
    public GameObject BGMPrefab;
    public GameObject SFXPrefab;

    private GameObject currentBGMusic;

    private float musicVolume;
    private float sfxVolume;

    public delegate void musicChangeAction(string name);
    public static event musicChangeAction OnMusicChange;
    public delegate void sfxPressedAction(string name);
    public static event sfxPressedAction OnSFXPressed;
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
        MusicChange("BGMainMenu");
    }

    public static void MusicChange(string name)
    {
        if (AudioManager.Instance.currentBGMusic != null)
        {
            AudioSource currentAudioSource = AudioManager.Instance.currentBGMusic.GetComponent<AudioSource>();
            currentAudioSource.Pause();
        }

        AudioSource audioSource;
        GameObject BGMusic = GameObject.Find(name);
        if (BGMusic == null)
        {
            BGMusic = Instantiate(AudioManager.Instance.BGMPrefab);
            BGMusic.name = name;
            audioSource = BGMusic.GetComponent<AudioSource>();
            BGMusic_SO bgm = System.Array.Find(AudioManager.Instance.backgroundMusics, bgm => bgm.name == name);
            if (bgm != null)
            {
                audioSource.clip = bgm.values.audioClip;
                audioSource.outputAudioMixerGroup = bgm.values.output;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Music with name " + name + " not found!");
            }
        }
        else
        {
            audioSource = BGMusic.GetComponent<AudioSource>();
            audioSource.Play();
        }

        AudioManager.Instance.currentBGMusic = BGMusic;

        OnMusicChange?.Invoke(name);
    }

    public static void SFXPressed(string name)
    {
        AudioSource audioSource;
        GameObject mySFX = GameObject.Find(name);
        if (mySFX == null)
        {
            mySFX = Instantiate(AudioManager.Instance.SFXPrefab);
            mySFX.name = name;
            audioSource = mySFX.GetComponent<AudioSource>();
            SFX_SO sfx = System.Array.Find(AudioManager.Instance.soundEffects, sfx => sfx.name == name);
            audioSource.clip = sfx.values.audioClip;
            audioSource.outputAudioMixerGroup = sfx.values.output;
            audioSource.Play();
        }
        else
        {
            audioSource = mySFX.GetComponent<AudioSource>();
            audioSource.Play();
        }
        OnSFXPressed?.Invoke(name);
    }

    
}
