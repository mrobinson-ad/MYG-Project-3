using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "BGMusic", menuName = "Scriptable Objects/BackgroundMusic")]
public class BGMusic_SO : ScriptableObject
{
    public SerializableBGMusic values;


    [System.Serializable]
    public class SerializableBGMusic
    {
        public string name;
        public AudioClip audioClip;
        public AudioMixerGroup output;
        [Range(0f,1f)]
        public float volume = 1;
        public bool isLoop;
        [Range(-3f,3f)]
        public float pitch = 1;
    }
}
