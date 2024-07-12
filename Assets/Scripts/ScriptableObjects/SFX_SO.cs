using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFX", menuName = "Scriptable Objects/SoundEffect")]
public class SFX_SO : ScriptableObject
{
    public SerializableSFX values;


    [System.Serializable]
    public class SerializableSFX
    {
        public string name;
        public AudioClip audioClip;
        public AudioMixerGroup output;
        [Range(0f,1f)]
        public float volume = 1;
        public bool isLoop = false;
        [Range(-3f,3f)]
        public float pitch = 1;
    }
}