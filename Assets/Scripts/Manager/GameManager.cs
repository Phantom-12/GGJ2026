using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CustomerQueue queue;
    public int totalTime = 600;
    public Text timeText, scoreText;
    public Text finalScoreText;
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

    public void GameOver()
    {
        StopAllCoroutines();
        finalScoreText.text = $"Final Score\n{score}";
        AnimationManager.Instance.GameOverAnimation();
    }

    public void StartFirstRound()
    {
        string itemName = queue.GetNameOfFirstCustomer();
        ItemManager.Instance.SetItemName(itemName);

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

        string itemName = queue.GetNameOfFirstCustomer();
        ItemManager.Instance.SetItemName(itemName);

        GameObject itemObject = ItemManager.Instance.GetAvailableItemObject();
        Item itemScript = itemObject.GetComponent<Item>();
        itemScript.Init();
        
        AnimationManager.Instance.ItemSlideInAnimation();
    }

    /* TODO: 将膜放下，并计分的函数 */
    public void PutMaskDown()
    {
        
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
