using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    [SerializeField] private Indicator indicator;
    [SerializeField] private Bubble bubble;
    [SerializeField] Score2Rating score2Rating;
    public Animator startCanvasAnimator, infoCanvasAnimator, endCanvasAnimator, catAnimator;

    public Image endBackgroundImage;
    public Text scoreAddition;
    public Image ratingImage, commentImage;
    public Vector3 originalPosition;
    public float itemAnimationDuration = 0.4f;
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
        item = ItemManager.Instance.GetCurrentItemObject();
        if (item == null) return;
        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.SlideIn();
        }
        AudioManager.Instance.PlaySfx(AudioType.ItemEnterSfx);
    }

    public void ItemSlideOutAnimation()
    {
        item = ItemManager.Instance.GetCurrentItemObject();
        if (item == null) return;
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
        scoreAdditionRect.anchoredPosition = new Vector3(-830, 0, 0);
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
        GameManager.Instance.canPutDown = false;
        startCanvasAnimator.SetTrigger("Hide");
        infoCanvasAnimator.SetTrigger("Show");
        indicator.Show();
    }

    public void GameOverAnimation()
    {
        int ratingIndex = GameManager.Instance.GetRatingIndex();
        ratingImage.sprite = score2Rating.data[ratingIndex].ratingSprite;
        commentImage.sprite = score2Rating.data[ratingIndex].commentSprite;
        endBackgroundImage.sprite = score2Rating.data[ratingIndex].endBgSprite;
        indicator.Hide();
        endCanvasAnimator.SetTrigger("Show");
        infoCanvasAnimator.SetTrigger("Hide");
    }

    public void RestartGameAnimation()
    {
        endCanvasAnimator.SetTrigger("Hide");
        infoCanvasAnimator.SetTrigger("Show");
        indicator.Show();
    }

    public void ReturnToStartAnimation()
    {
        endCanvasAnimator.SetTrigger("Hide");
        startCanvasAnimator.SetTrigger("Show");
    }

    public void ChangeCatMood(int resultCode)
    {
        int randomLayer = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            catAnimator.SetLayerWeight(i, i == randomLayer ? 1f : 0f);
        }
        catAnimator.SetInteger("Result", resultCode);
        catAnimator.SetTrigger("SwitchMood");
        switch (resultCode)
        {
            case 1:
                AudioManager.Instance.PlaySfx(AudioType.CatBadSfx);
                break;
            case 2:
                AudioManager.Instance.PlaySfx(AudioType.CatNormalSfx);
                break;
            case 3:
                AudioManager.Instance.PlaySfx(AudioType.CatPerfectSfx);
                break;
        }
    }

    public void CatStartWorking()
    {
        catAnimator.SetInteger("Result", 0);
        catAnimator.SetBool("Working", true);
        catAnimator.SetTrigger("Idle");
    }

    public void SetCatIdle()
    {
        catAnimator.SetInteger("Result", 0);
        catAnimator.SetBool("Working", false);
        catAnimator.SetTrigger("Idle");
    }

    public void ShowBubble(int level)
    {
        _ = bubble.ShowBubbleAnimation(level);
    }
}