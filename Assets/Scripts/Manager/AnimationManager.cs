using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    [SerializeField] private Indicator indicator;
    [SerializeField] private Bubble bubble;
    public Animator startCanvasAnimator, infoCanvasAnimator, endCanvasAnimator, catAnimator;
    [SerializeField] public List<Sprite> commentSprites = new();
    [SerializeField] public List<Sprite> ratingSprites = new();
    [SerializeField] public List<Sprite> endBgSprites = new();

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
        GameManager.Instance.canPutDown = false;
        startCanvasAnimator.SetTrigger("Hide");
        infoCanvasAnimator.SetTrigger("Show");
        indicator.Show();
    }

    public void GameOverAnimation()
    {
        int ratingIndex = GameManager.Instance.GetRatingIndex();
        ratingSprite = ratingSprites[Mathf.Clamp(ratingIndex, 0, ratingSprites.Count - 1)];
        ratingImage.sprite = ratingSprite;
        commentImage.sprite = commentSprites[Mathf.Clamp(ratingIndex, 0, commentSprites.Count - 1)];
        Sprite endBg = endBgSprites[Mathf.Clamp(ratingIndex, 0, endBgSprites.Count - 1)];
        if (endBg != null)
        {
            endBackgroundImage.color = Color.white;
            endBackgroundImage.sprite = endBg;
        } else
        {
            endBackgroundImage.color = new Color(0, 0, 0, 0.4f);
        }
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
        if(Random.value < 0.5f)
        {
            catAnimator.SetLayerWeight(1, 1f);
            catAnimator.SetLayerWeight(0, 0f);
        }
        else
        {
            catAnimator.SetLayerWeight(1, 0f);
            catAnimator.SetLayerWeight(0, 1f);
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
        bubble.ShowBubbleAnimation(level);
    }
}