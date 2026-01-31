using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CustomerQueue queue;
    public int totalTime = 600;
    public Text timeText, scoreText;
    public List<int> ratingScoreList = new() { 70, 100, 125, 150, 175 };
    [SerializeField]
    private Object operableObject;
    int time = 0, score = 0;
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
            Instance.Start();
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void StartGame()
    {
        time = 0;
        score = 0;
        timeText.text = "Time 00:00:00";
        scoreText.text = "Score 0";
        StopAllCoroutines();
        StartCoroutine(Timer());
        queue.Init();
    }

    public void RestartGame()
    {
        AnimationManager.Instance.RestartGameAnimation();
        StartGame();
    }

    public void ExitToMainMenu()
    {
        AnimationManager.Instance.ReturnToStartAnimation();
    }

    public void GameOver()
    {
        StopAllCoroutines();
        ItemManager.Instance.ResetItems();
        queue.ClearQueue();
        AnimationManager.Instance.GameOverAnimation();
    }

    public void StartFirstRound()
    {
        if(queue.IsFirstCustomerSpecial())
        {
            ItemManager.Instance.SetCurrentItemName(queue.GetNameOfFirstCustomer());
        } else
        {
            ItemManager.Instance.SetCurrentItemName("");
        }

        GameObject itemObject = ItemManager.Instance.GetAvailableItemObject();
        Item itemScript = itemObject.GetComponent<Item>();
        itemScript.Init();
        
        AnimationManager.Instance.ItemSlideInAnimation();
    }

    // 完成贴膜后进行下一轮
    public void StartNextRound()
    {
        queue.CustomerLeave();

        AnimationManager.Instance.ItemSlideOutAnimation();

        if(queue.IsFirstCustomerSpecial())
        {
            ItemManager.Instance.SetCurrentItemName(queue.GetNameOfFirstCustomer());
        } else
        {
            ItemManager.Instance.SetCurrentItemName("");
        }

        GameObject itemObject = ItemManager.Instance.GetAvailableItemObject();
        Item itemScript = itemObject.GetComponent<Item>();
        itemScript.Init();
        
        AnimationManager.Instance.ItemSlideInAnimation();
    }

    public int GetRatingIndex()
    {
        int rating = 0;
        foreach (int threshold in ratingScoreList)
        {
            if (score >= threshold)
            {
                rating++;
            }
            else
            {
                break;
            }
        }
        return rating;
        
    }

    /* TODO: 将膜放下，并计分的函数 */
    public void PutMaskDown()
    {
        PutMaskDownInner().Forget();
    }

    private async UniTaskVoid PutMaskDownInner()
    {
        AnimationManager.Instance.HidePasteButton();
        var (curMood,curScore) = await operableObject.PutDown();
        AnimationManager.Instance.ScoreAdditionAnimation(curScore);
        AnimationManager.Instance.ChangeCatMood(curMood);
        StartNextRound();
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"Score {score}";
    }

    IEnumerator Timer()
    {
        time = totalTime;
        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time--;
            timeText.text = $"Time {time/3600:D2}:{time/60%60:D2}:{time%60:D2}";
        }
        GameOver();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
