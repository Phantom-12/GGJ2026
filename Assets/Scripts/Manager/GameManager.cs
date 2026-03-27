using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CustomerQueue queue;
    public int totalTime = 600;
    public Text timeText, scoreText, multipleText, comboText;
    public Slider comboSlider;
    public int difficultyLevel = 1;
    [SerializeField] private OperableObject operableObject;
    [SerializeField] private Combo2MusicSpeed combo2MusicSpeed;
    private Combo2Multiple combo2Multiple;
    private Rating2Score rating2Score;
    private OriginalData originalData;
    private PutDownAutoAlignConfig putDownAutoAlignConfig;
    public bool canPutDown = true;
    int time = 0, comboCount = 0;
    private System.Threading.CancellationTokenSource _putDownCts;
    private Coroutine _comboTimerCoroutine;

    float score = 0, multiple = 1f;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Instance.Awake();
        }

        AudioManager.Instance.PlayBGM(AudioType.StartPageMusic);
    }

    public void SetDifficultyLevel(int level)
    {
        difficultyLevel = level;
        LoadDifficultyCfg();
    }

    public void LoadDifficultyCfg()
    {
        putDownAutoAlignConfig = Resources.Load<PutDownAutoAlignConfig>("Cfgs/PutDownAutoAlignConfig");

        switch (difficultyLevel)
        {
            case 1:
                combo2Multiple = Resources.Load<Combo2Multiple>("Cfgs/Easy/Combo2Multiple");
                rating2Score = Resources.Load<Rating2Score>("Cfgs/Easy/Rating2Score");
                originalData = Resources.Load<OriginalData>("Cfgs/Easy/OriginalParams");
                break;
            case 2:
                combo2Multiple = Resources.Load<Combo2Multiple>("Cfgs/Normal/Combo2Multiple");
                rating2Score = Resources.Load<Rating2Score>("Cfgs/Normal/Rating2Score");
                originalData = Resources.Load<OriginalData>("Cfgs/Normal/OriginalParams");
                break;
            case 3:
                combo2Multiple = Resources.Load<Combo2Multiple>("Cfgs/Hard/Combo2Multiple");
                rating2Score = Resources.Load<Rating2Score>("Cfgs/Hard/Rating2Score");
                originalData = Resources.Load<OriginalData>("Cfgs/Hard/OriginalParams");
                break;
        }
        totalTime = originalData.totalTime;
        operableObject.SetDefaultMoveSpeed(originalData.startMoveSpeed);
        operableObject.SetPutDownAutoAlignConfig(putDownAutoAlignConfig);
    }

    public int GetScore()
    {
        return (int)score;
    }

    public void StartGame()
    {
        time = 0;
        score = 0;
        canPutDown = false;
        comboCount = 0;
        multiple = 1f;
        timeText.text = "Time 00:00:00";
        scoreText.text = "Score 0";
        multipleText.text = "x1.0";
        comboText.text = "Combo 0";

        operableObject.ResetMoveSpeed();

        HideComboUI();
        if (_comboTimerCoroutine != null)
        {
            StopCoroutine(_comboTimerCoroutine);
            _comboTimerCoroutine = null;
        }

        StopAllCoroutines();
        StartCoroutine(Timer());
        StartCoroutine(PlayerPutDownListener());

        queue.Init();
        AnimationManager.Instance.SetCatIdle();
        AudioManager.Instance.PlayBGM(AudioType.LevelMusic);
        AudioManager.Instance.SetBGMSpeed(1f);
        AudioManager.Instance.StopAllSfx();
    }

    public void HideComboUI()
    {
        multipleText.gameObject.SetActive(false);
        comboSlider.gameObject.SetActive(false);
        comboText.gameObject.SetActive(false);
    }

    public void ShowComboUI()
    {
        multipleText.gameObject.SetActive(true);
        comboSlider.gameObject.SetActive(true);
        comboText.gameObject.SetActive(true);
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
        AudioManager.Instance.SetBGMSpeed(1f);
    }

    public void GameOver()
    {
        CancelPutDownTask();
        StopAllCoroutines();
        AnimationManager.Instance.ItemSlideOutAnimation();
        ItemManager.Instance.ResetItems();
        queue.ClearQueue();
        AnimationManager.Instance.GameOverAnimation();
        operableObject.Hide();
        int ratingIndex = GameManager.Instance.GetRatingIndex();
        switch (ratingIndex)
        {
            case 5:
            case 4:
                AudioManager.Instance.PlaySfx(AudioType.SettlementPerfectSfx);
                break;
            case 3:
            case 2:
                AudioManager.Instance.PlaySfx(AudioType.SettlementNormalSfx);
                break;
            case 1:
            case 0:
                AudioManager.Instance.PlaySfx(AudioType.SettlementBadSfx);
                break;
        }

        AudioManager.Instance.StopBGM();
        Debug.Log($"Game over. Final score: {GetScore()}");
    }

    public void StartFirstRound()
    {
        if (queue.IsFirstCustomerSpecial())
        {
            ItemManager.Instance.SetCurrentItemName(queue.GetNameOfFirstCustomer());
        }
        else
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

        ItemManager.Instance.SwitchActiveItem();

        if (queue.IsFirstCustomerSpecial())
        {
            ItemManager.Instance.SetCurrentItemName(queue.GetNameOfFirstCustomer());
        }
        else
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
        foreach (var data in rating2Score.data)
        {
            if (score >= data.score)
            {
                rating = rating2Score.data.IndexOf(data);
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
            if (canPutDown && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) ||
                               Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.J) ||
                               Input.GetKeyDown(KeyCode.N) || IsNonUiPointerDown()))
            {
                PutMaskDown();
            }
        }
    }

    private bool IsNonUiPointerDown()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && !IsPointerOverUi(touch.fingerId))
                {
                    return true;
                }
            }
        }

        return Input.GetMouseButtonDown(0) && !IsPointerOverUi();
    }

    private bool IsPointerOverUi(int pointerId = -1)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }

    /* TODO: 将膜放下，并计分的函数 */
    public void PutMaskDown()
    {
        CancelPutDownTask();
        _putDownCts = new System.Threading.CancellationTokenSource();
        PutMaskDownInner(_putDownCts.Token).Forget();
    }

    private void CancelPutDownTask()
    {
        _putDownCts?.Cancel();
        _putDownCts?.Dispose();
        _putDownCts = null;
    }

    private async UniTaskVoid PutMaskDownInner(System.Threading.CancellationToken ct)
    {
        canPutDown = false;
        var (level, curScore) = await operableObject.PutDown();
        if (ct.IsCancellationRequested) return;

        if (level >= combo2Multiple.comboLevel)
        {
            comboCount++;
            ShowComboUI();
            UpdateComboUI();
            ChangeObjectSpeed();
            if (_comboTimerCoroutine != null)
                StopCoroutine(_comboTimerCoroutine);
            _comboTimerCoroutine = StartCoroutine(ComboTimer());
        }
        else
        {
            comboCount = 0;
            multiple = 1f;
            HideComboUI();
            operableObject.ResetMoveSpeed();
            AudioManager.Instance.SetBGMSpeed(1f, 0.2f);
            if (_comboTimerCoroutine != null)
                StopCoroutine(_comboTimerCoroutine);
            _comboTimerCoroutine = null;
        }

        int finalScore = Mathf.RoundToInt(curScore * multiple);
        AddScore(finalScore);
        AnimationManager.Instance.ScoreAdditionAnimation(finalScore);
        AnimationManager.Instance.ChangeCatMood(level);
        AnimationManager.Instance.ShowBubble(level);
        StartNextRound();
    }

    private int GetComboDataIndex()
    {
        foreach (var data in combo2Multiple.data)
        {
            if (comboCount <= data.comboTime)
            {
                return combo2Multiple.data.IndexOf(data);
            }
        }

        return combo2Multiple.data.Count - 1;
    }

    private void UpdateComboUI()
    {
        if (combo2Multiple == null || combo2Multiple.data.Count == 0) return;
        int index = Mathf.Clamp(GetComboDataIndex(), 0, combo2Multiple.data.Count - 1);
        var data = combo2Multiple.data[index];

        multiple = data.scoreMultiple;
        multipleText.text = $"x{multiple:F1}";
        multipleText.color = data.color;
        comboText.text = $"Combo {comboCount}";
        comboText.color = data.color;
        comboSlider.fillRect.GetComponent<Image>().color = data.color;

        var audioData = combo2MusicSpeed.data[Mathf.Clamp(GetComboDataIndex(), 0, combo2MusicSpeed.data.Count - 1)];
        AudioManager.Instance.SetBGMSpeed(audioData.bgmSpeedMultiple, 0.2f);
    }

    private void ChangeObjectSpeed()
    {
        if (combo2Multiple == null || combo2Multiple.data.Count == 0) return;
        int index = Mathf.Clamp(GetComboDataIndex(), 0, combo2Multiple.data.Count - 1);
        var data = combo2Multiple.data[index];
        operableObject.SetMoveSpeed(data.moveSpeed);
    }

    IEnumerator ComboTimer()
    {
        comboSlider.value = 1f;
        float elapsed = 0f;
        while (elapsed < combo2Multiple.comboTimeWindow)
        {
            elapsed += Time.deltaTime;
            comboSlider.value = 1f - elapsed / combo2Multiple.comboTimeWindow;
            yield return null;
        }

        comboCount = 0;
        multiple = 1f;
        _comboTimerCoroutine = null;
        HideComboUI();
        operableObject.ResetMoveSpeed();
        AudioManager.Instance.SetBGMSpeed(1f, 0.2f);
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = $"Score {(int)score}";
    }

    IEnumerator Timer()
    {
        time = totalTime;
        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time--;
            timeText.text = $"Time {time / 3600:D2}:{time / 60 % 60:D2}:{time % 60:D2}";
        }

        GameOver();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
