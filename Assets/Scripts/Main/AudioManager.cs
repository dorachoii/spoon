using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BGMType
{
    Intro,
    Game,
    Boss
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ChangeBGM(BGMType.Intro);
    }

    public void ChangeBGM(BGMType type)
    {
        AudioClip selected = null;
        selected = bgmClips[(int)type];
        if (bgmSource.clip == selected) return;

        bgmSource.Stop();
        bgmSource.clip = selected;
        bgmSource.Play();
    }
}
