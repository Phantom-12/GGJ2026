using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    public Animator startCanvasAnimator, infoCanvasAnimator, endCanvasAnimator, catAnimator;

    public Text scoreAddition;
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
        infoCanvasAnimator.SetTrigger("Show");
    }

    public void HideInfoCanvas()
    {
        infoCanvasAnimator.SetTrigger("Hide");
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

    public void ScoreAdditionAnimation(int addition)
    {
        scoreAddition.text = "+" + addition.ToString();
        StartCoroutine(ScoreAdditionRoutine(addition));
    }

    IEnumerator ScoreAdditionRoutine(int addition)
    {
        RectTransform scoreAdditionRect = scoreAddition.GetComponent<RectTransform>();
        scoreAdditionRect.anchoredPosition = new Vector3(0, -160, 0);
        CanvasGroup canvasGroup = scoreAddition.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f);
        
        while (canvasGroup.alpha > 0f && Vector3.Distance(scoreAdditionRect.anchoredPosition, Vector3.zero) > 0.01f)
        {
            scoreAdditionRect.anchoredPosition = Vector3.Lerp(scoreAdditionRect.anchoredPosition, Vector3.zero, Time.deltaTime * 2f);
            canvasGroup.alpha -= Time.deltaTime * 2f;
            yield return null;
        }

        GameManager.Instance.AddScore(addition);
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

    public void ChangeCatMood(int resultCode)
    {   
        catAnimator.SetBool("Working", false);
        catAnimator.SetInteger("Result", resultCode);
    }

    public void CatStartWorking()
    {
        catAnimator.SetTrigger("Idle");
        catAnimator.SetBool("Working", true);
    }
}