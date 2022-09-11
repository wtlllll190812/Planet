using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class AudioManager : SerializedMonoBehaviour
{
    public static AudioManager instance;
    public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    public AudioSource audioSource;

    public void Awake()
    {
        instance = this;
    }

    public void Play(string clipName)
    {
        audioSource.clip = audioClips[clipName];
        audioSource.Play();
    }
}
