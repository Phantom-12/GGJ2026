using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Combo2MusicSpeed", order = 1)]
public class Combo2MusicSpeed : ScriptableObject
{
    [Serializable]
    public struct C2MSData
    {
        [Range(1.0f, 2.0f)]
        public float bgmSpeedMultiple;
    }
    public List<C2MSData> data;
}
