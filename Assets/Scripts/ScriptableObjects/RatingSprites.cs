using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RatingSprites", order = 1)]
public class RatingSprites : ScriptableObject
{
    [Serializable]
    public struct RatingSpriteData
    {
        public RatingLevel ratingLevel;
        public Sprite sprite;
        public Sprite commentSprite;
        public Sprite endBgSprite;
    }
    public List<RatingSpriteData> data;
}