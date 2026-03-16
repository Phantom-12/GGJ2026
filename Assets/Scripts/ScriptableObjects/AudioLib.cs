using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType
{
    LevelMusic,
    StartPageMusic,
    ItemEnterSfx,
    BestAlignmentSfx,
    PerfectAlignmentSfx,
    NormalAlignmentSfx,
    FailAlignmentSfx,
    ButtonClickSfx,
    CatPerfectSfx,
    CatNormalSfx,
    CatBadSfx,
    SettlementPerfectSfx,
    SettlementNormalSfx,
    SettlementBadSfx,
}

[CreateAssetMenu(menuName = "Audio/Audio Library")]
public class AudioLib : ScriptableObject
{
    [System.Serializable]
    public struct AudioEntry
    {
        public AudioType type;
        public AudioClip clip;
    }

    public List<AudioEntry> audioList;

    private Dictionary<AudioType, AudioClip> _audioDict;

    public void Init()
    {
        _audioDict = new Dictionary<AudioType, AudioClip>();
        foreach (var e in audioList)
            _audioDict[e.type] = e.clip;
    }

    public AudioClip GetAudio(AudioType type)
    {
        return _audioDict[type];
    }
}