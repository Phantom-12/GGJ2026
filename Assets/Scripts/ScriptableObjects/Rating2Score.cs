using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum RatingLevel
{
    F,
    C,
    B,
    A,
    S,
    S_plus,
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Rating2Score", order = 1)]
public class Rating2Score : ScriptableObject
{
    [Serializable]
    public struct R2SData
    {
        public RatingLevel ratingLevel;
        public int score;
    }
    public List<R2SData> data;

}