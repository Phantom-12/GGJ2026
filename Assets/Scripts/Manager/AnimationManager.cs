using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    public Animator startCanvasAnimator, infoCanvasAnimator, endCanvasAnimator, catAnimator;
    public List<Sprite> ratingSprites = new();
    public Text scoreAddition, finalScoreText;
    public Image ratingImage;
    public Vector3 originalPosition;
    public float itemAnimationDuration = 0.4f;
    public GameObject pasteButton;
    private GameObject item;
    private Sprite ratingSprite;

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

    public void ShowPasteButton()
    {
        pasteButton.SetActive(true);
    }

    public void HidePasteButton()
    {
        pasteButton.SetActive(false);
    }

    public void StartGameAnimation()
    {
        pasteButton.SetActive(false);
        startCanvasAnimator.SetTrigger("Hide");
        infoCanvasAnimator.SetTrigger("Show");
    }

    public void GameOverAnimation()
    {
        int ratingIndex = GameManager.Instance.GetRatingIndex();
        ratingSprite = ratingSprites[Mathf.Clamp(ratingIndex, 0, ratingSprites.Count - 1)];
        ratingImage.sprite = ratingSprite;
        finalScoreText.text = $"Final Score\n {GameManager.Instance.GetScore()}";

        endCanvasAnimator.SetTrigger("Show");
        infoCanvasAnimator.SetTrigger("Hide");
    }

    public void RestartGameAnimation()
    {
        endCanvasAnimator.SetTrigger("Hide");
        infoCanvasAnimator.SetTrigger("Show");
    }

    public void ReturnToStartAnimation()
    {
        endCanvasAnimator.SetTrigger("Hide");
        startCanvasAnimator.SetTrigger("Show");
    }

    public void ChangeCatMood(int resultCode)
    {   
        catAnimator.SetBool("Working", false);
        catAnimator.SetInteger("Result", resultCode);
    }

    public void CatStartWorking()
    {
        catAnimator.SetInteger("Result", 0);
        catAnimator.SetTrigger("Idle");
        catAnimator.SetBool("Working", true);
    }
}