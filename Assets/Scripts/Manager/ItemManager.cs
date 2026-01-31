using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public GameObject item1, item2;
    public Dictionary<string, Sprite> specialItemSprites = new();
    public List<Sprite> commonItemSprites = new();

    public string currentItemName = "";

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
        Sprite[] loadedSpecialSprites = Resources.LoadAll<Sprite>("Sprites/Items/Special");
        foreach (Sprite sprite in loadedSpecialSprites)
        {
            specialItemSprites[sprite.name] = sprite;
        }

        Sprite[] loadedCommonSprites = Resources.LoadAll<Sprite>("Sprites/Items/Common");
        commonItemSprites.AddRange(loadedCommonSprites);
    }

    public GameObject GetAvailableItemObject()
    {
        if (!item1.activeSelf) return item1;
        if (!item2.activeSelf) return item2;
        return null;
    }

    public GameObject GetCurrentItemObject()
    {
        if (item1.activeSelf) return item1;
        if (item2.activeSelf) return item2;
        return null;
    }


    public void SetItemName(string name)
    {
        currentItemName = name;
    }

    public Sprite GetItemSprite()
    {
        if (specialItemSprites.ContainsKey(currentItemName))
        {
            return specialItemSprites[currentItemName];
        }
        return commonItemSprites[Random.Range(0, commonItemSprites.Count)];
    }
}