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
    public bool canPutDown = true;
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
        AudioManager.Instance.PlayBGM(AudioType.StartPageMusic);
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
        StartCoroutine(PlayerPutDownListener());
        queue.Init();
        AnimationManager.Instance.SetCatIdle();
        AudioManager.Instance.PlayBGM(AudioType.LevelMusic);
    }

    public void RestartGame()
    {
        AnimationManager.Instance.RestartGameAnimation();
        ItemManager.Instance.ResetItems();
        StartGame();
    }

    public void ExitToMainMenu()
    {
        AnimationManager.Instance.ReturnToStartAnimation();
        AudioManager.Instance.PlayBGM(AudioType.StartPageMusic);
    }

    public void GameOver()
    {
        StopAllCoroutines();
        ItemManager.Instance.ResetItems();
        queue.ClearQueue();
        AnimationManager.Instance.GameOverAnimation();
        operableObject.Hide();
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

        ItemManager.Instance.InitActiveItem();
        AnimationManager.Instance.ItemSlideInAnimation();
    }

    // 完成贴膜后进行下一轮
    public void StartNextRound()
    {
        queue.CustomerLeave();

        AnimationManager.Instance.ItemSlideOutAnimation();
        ItemManager.Instance.SwitchActiveItem();

        if(queue.IsFirstCustomerSpecial())
        {
            ItemManager.Instance.SetCurrentItemName(queue.GetNameOfFirstCustomer());
        } else
        {
            ItemManager.Instance.SetCurrentItemName("");
        }

        GameObject itemObject = ItemManager.Instance.GetCurrentItemObject();
        Item itemScript = itemObject.GetComponent<Item>();
        itemScript.Init();
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

    IEnumerator PlayerPutDownListener()
    {
        while (true)
        {
            yield return null;
            if (canPutDown && Input.GetKeyDown(KeyCode.Space))
            {
                PutMaskDown();
            }
        }
    }

    /* TODO: 将膜放下，并计分的函数 */
    public void PutMaskDown()
    {
        PutMaskDownInner().Forget();
    }

    private async UniTaskVoid PutMaskDownInner()
    {
        canPutDown = false;
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
