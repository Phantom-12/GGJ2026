using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Progress progress;
    public Item item;
    public Text timeText, scoreText;
    public Text finalScoreText;
    public int customerCount = 5;
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
        progress.InitProgress(customerCount);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"Score {score}";
    }

    IEnumerator Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            time++;
            timeText.text = $"Time {time/3600:D2}:{(time/60)%60:D2}:{time%60:D2}";
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
