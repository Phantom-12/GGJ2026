using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    public Animator startCanvasAnimator, endCanvasAnimator;
    public GameObject infoCanvas;
    public Vector3 originalPosition;
    public float itemAnimationDuration = 0.4f;
    private GameObject item;

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

    public void ShowInfoCanvas()
    {
        infoCanvas.SetActive(true);
    }

    public void HideInfoCanvas()
    {
        infoCanvas.SetActive(false);
    }

    public void ItemSlideInAnimation()
    {
        item = ItemManager.Instance.GetAvailableItemObject();
        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.SlideIn();
        }
    }

    public void ItemSlideOutAnimation()
    {
        item = ItemManager.Instance.GetCurrentItemObject();
        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.SlideOut();
        }
    }

    public void StartGameAnimation()
    {
        startCanvasAnimator.SetTrigger("Hide");
        ShowInfoCanvas();
    }

    public void GameOverAnimation()
    {
        endCanvasAnimator.SetTrigger("Show");
        HideInfoCanvas();
    }
}