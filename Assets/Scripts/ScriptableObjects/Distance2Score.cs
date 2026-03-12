using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Distance2Score", order = 1)]
public class Distance2Score : ScriptableObject
{
    [Serializable]
    public struct Distance2ScoreData
    {
        public float distance;
        public int score;
    }
    public List<Distance2ScoreData> data;
}