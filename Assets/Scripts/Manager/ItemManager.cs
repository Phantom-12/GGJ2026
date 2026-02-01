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
    int activeItemCode = 1;

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

    public void InitActiveItem()
    {
        Item itemScript = item1.GetComponent<Item>();
        itemScript.Init();
    }

    public void SwitchActiveItem()
    {
        activeItemCode = activeItemCode == 1 ? 2 : 1;
    }

    public GameObject GetAvailableItemObject()
    {
        if (activeItemCode == 1 && !item2.activeSelf){
            return item2;
        }
        if (activeItemCode == 2 && !item1.activeSelf)
        {
            return item1;
        }
        return null;
    }

    public GameObject GetCurrentItemObject()
    {
        return activeItemCode == 1 ? item1 : item2;
    }

    public void ResetItems()
    {
        activeItemCode = 1;
    }

    public Sprite GetItemSprite()
    {
        if (specialItemSprites.ContainsKey(currentItemName))
        {
            return specialItemSprites[currentItemName];
        }
        int index = Random.Range(0, commonItemSprites.Count);
        Sprite sprite = commonItemSprites[index];
        currentItemName = sprite.name;
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