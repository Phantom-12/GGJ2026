using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public GameObject item1, item2;
    public Dictionary<string, Sprite> specialItemSprites = new();
    public List<Sprite> commonItemSprites = new();
    public Dictionary<string, Sprite> itemMembranes = new();
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

        Sprite[] loadedMembranes = Resources.LoadAll<Sprite>("Sprites/Membranes");
        foreach (Sprite sprite in loadedMembranes)
        {
            itemMembranes[sprite.name] = sprite;
        }
    }

    public void SetCurrentItemName(string itemName)
    {
        currentItemName = itemName;
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

    public void ResetItems()
    {
        item1.SetActive(false);
        item2.SetActive(false);
    }

    public Sprite GetItemSprite()
    {
        if (specialItemSprites.ContainsKey(currentItemName))
        {
            return specialItemSprites[currentItemName];
        }
        int index = Random.Range(0, commonItemSprites.Count);
        Sprite sprite = commonItemSprites[index];
        if(currentItemName == "") currentItemName = sprite.name;
        return sprite;
    }

    public Sprite GetItemMembrane()
    {
        if (itemMembranes.ContainsKey(currentItemName))
        {
            return itemMembranes[currentItemName];
        }
        return null;
    }
}