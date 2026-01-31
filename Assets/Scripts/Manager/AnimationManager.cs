using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    public GameObject item;
    public Vector3 originalPosition;
    public float itemAnimationDuration = 0.4f;

    // Start is called before the first frame update
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
    }

    public void StartItemAnimation()
    {
        Item itemScript = item.GetComponent<Item>();
        itemScript.Init(item.GetComponent<SpriteRenderer>().sprite);
        if (itemScript != null)
        {
            itemScript.SlideIn();
        }
    }
}