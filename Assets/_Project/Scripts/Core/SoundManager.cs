using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviourSingleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopingSFXSource;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> bgmClips;
    [SerializeField] private List<AudioClip> sfxClips;
    [SerializeField] private float limmitPlaySfxTime = 0.1f;
    private Dictionary<string, AudioClip> bgmMap;
    private Dictionary<string, AudioClip> sfxMap;

    private Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void Start()
    {
        bgmMap = new Dictionary<string, AudioClip>();
        foreach (var clip in bgmClips)
        {
            bgmMap[clip.name] = clip;
        }

        sfxMap = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
        {
            sfxMap[clip.name] = clip;
        }
    }
    public void SetSpeedBackgroundMusic(float pitch)
    {
        bgmSource.pitch = pitch;
    }
    public void PlayBackgroundMusic(string name)
    {
        if (bgmMap.TryGetValue(name, out var clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{name}' not found.");
        }
    }

    public void StopBackgroundMusic()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(string name, float volumeScale = 1f)
    {
        if (sfxMap.TryGetValue(name, out var clip))
        {
            float currentTime = Time.time;
            if (lastPlayTimes.TryGetValue(name, out float lastTime))
            {
                if (currentTime - lastTime < limmitPlaySfxTime)
                    return;
            }
            lastPlayTimes[name] = currentTime;
            sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' not found.");
        }
    }

    public void PlayLoopingSFX(string name, float volumeScale = 1f)
    {
        if (sfxMap.TryGetValue(name, out var clip))
        {
            loopingSFXSource.clip = clip;
            loopingSFXSource.loop = true;
            loopingSFXSource.volume = volumeScale;
            if (!loopingSFXSource.isPlaying)
            {
                loopingSFXSource.Play();
            }
        }
    }

    public void StopLoopingSFX()
    {
        if (loopingSFXSource.isPlaying)
        {
            loopingSFXSource.Stop();
        }
    }
    private void OnSceneUnloaded(Scene scene)
    {
        StopBackgroundMusic();
        StopLoopingSFX();
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }
}
