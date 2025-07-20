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

    private Dictionary<string, AudioClip> bgmMap;
    private Dictionary<string, AudioClip> sfxMap;

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
            sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' not found.");
        }
    }
    public void PlaySFXAt(string name, Vector3 position)
    {
        if (sfxMap.TryGetValue(name, out var clip))
        {
            AudioSource.PlayClipAtPoint(clip, position);
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
