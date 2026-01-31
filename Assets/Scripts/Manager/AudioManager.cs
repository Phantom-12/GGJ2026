using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource _bgmSource;
    private List<AudioSource> _sfxSources = new List<AudioSource>();

    [Header("Volume")]
    [Range(0, 1)] public float bgmVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    [Header("SFX Pool")]
    public int sfxPoolSize = 8;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
    }

    void InitAudioSources()
    {
        // BGM
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;

        // SFX Pool
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            _sfxSources.Add(sfx);
        }
    }

    // ================= BGM =================

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            return;

        _bgmSource.clip = clip;
        _bgmSource.loop = loop;
        _bgmSource.volume = bgmVolume;
        _bgmSource.Play();
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    // ================= SFX =================

    public void PlaySfx(AudioClip clip)
    {
        AudioSource source = GetAvailableSfxSource();
        if (source == null)
        {
            Debug.LogWarning("No available SFX AudioSource!");
            return;
        }

        source.clip = clip;
        source.volume = sfxVolume;
        source.Play();
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (var sfx in _sfxSources)
        {
            if (!sfx.isPlaying)
                return sfx;
        }
        return null;
    }

    // ================= Volume =================

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        _bgmSource.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
    }
}
