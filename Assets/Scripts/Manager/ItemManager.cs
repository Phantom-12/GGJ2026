using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    public Dictionary<string, Sprite> specialItemSprites = new();
    public List<Sprite> commonItemSprites = new();

    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadSprites();
    }

    void LoadSprites()
    {
        // Load special item sprites
        Sprite[] loadedSpecialSprites = Resources.LoadAll<Sprite>("Sprites/Items/Special");
        foreach (Sprite sprite in loadedSpecialSprites)
        {
            specialItemSprites[sprite.name] = sprite;
        }

        // Load common item sprites
        Sprite[] loadedCommonSprites = Resources.LoadAll<Sprite>("Sprites/Items/Common");
        commonItemSprites.AddRange(loadedCommonSprites);
    }

    public Sprite GetRandomCommonItemSprite()
    {
        if (commonItemSprites.Count == 0) return null;
        int index = Random.Range(0, commonItemSprites.Count);
        return commonItemSprites[index];
    }

    public Sprite GetSpecialItemSprite(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        if (specialItemSprites.ContainsKey(itemName))
        {
            return specialItemSprites[itemName];
        }
        return null;
    }
}