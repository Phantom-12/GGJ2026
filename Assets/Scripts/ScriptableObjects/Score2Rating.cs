using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Score2Rating", order = 1)]
public class Score2Rating : ScriptableObject
{
    [Serializable]
    public struct RatingData
    {
        public int score;
        public Sprite ratingSprite;
        public Sprite commentSprite;
        public Sprite endBgSprite;
    }
    public List<RatingData> data;
}