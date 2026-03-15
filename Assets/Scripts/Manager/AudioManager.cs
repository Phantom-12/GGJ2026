using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public readonly struct SfxHandle
    {
        private readonly AudioManager _manager;

        internal SfxHandle(AudioManager manager, AudioSource source, int playId)
        {
            _manager = manager;
            Source = source;
            PlayId = playId;
        }

        public bool IsValid => _manager != null && Source != null && PlayId != 0;

        internal AudioSource Source { get; }

        internal int PlayId { get; }

        public void Stop()
        {
            _manager?.StopSfx(this);
        }
    }

    public AudioLib audioLibrary;
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField] private string bgmVolumeParameter = "BGMVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    [Header("Audio Sources")] private AudioSource _bgmSource;
    private readonly List<AudioSource> _sfxSources = new();
    private readonly Dictionary<AudioSource, int> _sfxPlayIds = new();
    private int _nextSfxPlayId = 1;
    private CancellationTokenSource _bgmSpeedCts;

    [Header("Volume")] [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float bgmVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    [Header("Playback")] [Min(0.01f)] public float bgmSpeed = 1f;
    [Min(0.01f)] public float sfxSpeed = 1f;

    [Header("SFX Pool")] public int sfxPoolSize = 8;

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
        audioLibrary.Init();
        ApplyAudioSettings();
    }

    void InitAudioSources()
    {
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.outputAudioMixerGroup = sfxMixerGroup;
            _sfxSources.Add(sfx);
            _sfxPlayIds[sfx] = 0;
        }
    }

    public void PlayBGM(AudioType clipType, bool loop = true)
    {
        var clip = audioLibrary.GetAudio(clipType);
        if (_bgmSource.clip == clip && _bgmSource.isPlaying)
            return;

        _bgmSource.clip = clip;
        _bgmSource.loop = loop;
        _bgmSource.volume = GetSourceVolume(bgmVolumeParameter, bgmVolume);
        _bgmSource.pitch = bgmSpeed;
        _bgmSource.Play();
    }

    public void StopBGM()
    {
        CancelBGMSpeedTransition();
        _bgmSource.Stop();
    }

    public SfxHandle PlaySfx(AudioType clipType)
    {
        var clip = audioLibrary.GetAudio(clipType);
        if (!clip)
            return default;

        AudioSource source = GetAvailableSfxSource();
        if (!source)
            return default;

        source.clip = clip;
        source.volume = GetSourceVolume(sfxVolumeParameter, sfxVolume);
        source.pitch = sfxSpeed;
        source.Play();

        int playId = _nextSfxPlayId++;
        _sfxPlayIds[source] = playId;
        return new SfxHandle(this, source, playId);
    }

    public void StopSfx(SfxHandle handle)
    {
        if (!handle.IsValid)
            return;

        if (!_sfxPlayIds.TryGetValue(handle.Source, out int currentPlayId) || currentPlayId != handle.PlayId)
            return;

        handle.Source.Stop();
        handle.Source.clip = null;
        _sfxPlayIds[handle.Source] = 0;
    }

    public void StopAllSfx()
    {
        foreach (var sfx in _sfxSources)
        {
            sfx.Stop();
            sfx.clip = null;
            _sfxPlayIds[sfx] = 0;
        }
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

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        if (!TrySetMixerVolume(masterVolumeParameter, masterVolume))
            AudioListener.volume = masterVolume;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (_bgmSource)
            _bgmSource.volume = GetSourceVolume(bgmVolumeParameter, bgmVolume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (UsesMixerVolume(sfxVolumeParameter))
            return;

        foreach (var sfx in _sfxSources)
            sfx.volume = sfxVolume;
    }

    public void SetBGMSpeed(float speed, float duration = 0f)
    {
        float targetSpeed = Mathf.Max(0.01f, speed);
        float startSpeed = _bgmSource ? _bgmSource.pitch : bgmSpeed;
        CancelBGMSpeedTransition();

        if (!_bgmSource || duration <= 0f)
        {
            ApplyBGMSpeed(targetSpeed);
            return;
        }

        _bgmSpeedCts = new CancellationTokenSource();
        TweenBGMSpeedAsync(startSpeed, targetSpeed, duration, _bgmSpeedCts.Token).Forget();
    }

    public void SetSFXSpeed(float speed)
    {
        ApplySFXSpeed(Mathf.Max(0.01f, speed));
    }

    public void SetSfxSpeed(float speed)
    {
        SetSFXSpeed(speed);
    }

    private void ApplyBGMSpeed(float speed)
    {
        bgmSpeed = speed;
        if (_bgmSource)
            _bgmSource.pitch = speed;
    }

    private void ApplySFXSpeed(float speed)
    {
        sfxSpeed = speed;

        foreach (var sfx in _sfxSources)
            sfx.pitch = speed;
    }

    private void CancelBGMSpeedTransition()
    {
        _bgmSpeedCts?.Cancel();
        _bgmSpeedCts?.Dispose();
        _bgmSpeedCts = null;
    }

    private async UniTaskVoid TweenBGMSpeedAsync(float startSpeed, float targetSpeed, float duration, CancellationToken ct)
    {
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                elapsed += Time.unscaledDeltaTime;
                ApplyBGMSpeed(Mathf.Lerp(startSpeed, targetSpeed, Mathf.Clamp01(elapsed / duration)));
            }

            ApplyBGMSpeed(targetSpeed);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void PlayButtonClickSfx()
    {
        PlaySfx(AudioType.ButtonClickSfx);
    }

    private void ApplyAudioSettings()
    {
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSfxVolume(sfxVolume);
        ApplyBGMSpeed(Mathf.Max(0.01f, bgmSpeed));
        ApplySFXSpeed(Mathf.Max(0.01f, sfxSpeed));
    }

    private float GetSourceVolume(string parameterName, float fallbackVolume)
    {
        return UsesMixerVolume(parameterName) ? 1f : Mathf.Clamp01(fallbackVolume);
    }

    private bool UsesMixerVolume(string parameterName)
    {
        return TrySetMixerVolume(parameterName, GetVolumeValue(parameterName));
    }

    private float GetVolumeValue(string parameterName)
    {
        if (parameterName == masterVolumeParameter)
            return masterVolume;

        if (parameterName == bgmVolumeParameter)
            return bgmVolume;

        if (parameterName == sfxVolumeParameter)
            return sfxVolume;

        return 1f;
    }

    private bool TrySetMixerVolume(string parameterName, float linearVolume)
    {
        if (!audioMixer || string.IsNullOrWhiteSpace(parameterName))
            return false;

        float dbVolume = linearVolume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp(linearVolume, 0.0001f, 1f)) * 20f;
        return audioMixer.SetFloat(parameterName, dbVolume);
    }

    private void OnDestroy()
    {
        CancelBGMSpeedTransition();
    }
}
