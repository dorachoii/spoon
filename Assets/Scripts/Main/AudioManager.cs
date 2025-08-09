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
    public AudioSource sfxSource; // 사운드 효과용 AudioSource
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
        AudioClip selected = bgmClips[(int)type];
        if (bgmSource.clip == selected) return;

        bgmSource.Stop();
        bgmSource.clip = selected;
        bgmSource.Play();
    }
    
    // 사운드 효과 재생 (One Shot)
    public void PlayOneShot(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
