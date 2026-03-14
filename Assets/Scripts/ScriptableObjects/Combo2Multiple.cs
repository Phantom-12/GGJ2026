using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Combo2Multiple", order = 1)]
public class Combo2Multiple : ScriptableObject
{
    [Header("允许连击的最低评分等级")] public int comboLevel = 3; 
    [Header("允许连击的时间窗口(单位秒)")] public float comboTimeWindow = 5;

    [Serializable]
    public struct C2MData
    {
        public Color color;
        public int comboTime;
        public float moveDuration;
        public float scoreMultiple;
        [Min(0.01f)]
        public float bgmSpeedMultiple;
    }
    public List<C2MData> data;
}
