using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public TextMeshProUGUI timeText, scoreText;
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
        scoreText.text = "Score: 0";
        StopAllCoroutines();
        StartCoroutine(Timer());
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"Score: {score}";
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
}
