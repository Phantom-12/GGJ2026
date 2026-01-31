using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    public Animator startCanvasAnimator, endCanvasAnimator;
    public GameObject infoCanvas;
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

    public void ItemSlideInAnimation()
    {
        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.SlideIn();
        }
    }

    public void StartGameAnimation()
    {
        startCanvasAnimator.SetTrigger("Hide");
        infoCanvas.SetActive(true);
    }

    public void GameOverAnimation()
    {
        infoCanvas.SetActive(false);
        endCanvasAnimator.SetTrigger("Show");
    }
}