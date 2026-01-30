using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score = 0;

    public int timeLimit = 300;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
