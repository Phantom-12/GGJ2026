using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelecter : MonoBehaviour
{
    [SerializeField] private Toggle easyToggle, normalToggle, hardToggle;

    public void Start()
    {
        easyToggle.onValueChanged.AddListener((isOn =>
        {
            if (isOn) GameManager.Instance.SetDifficultyLevel(1);
        }));
        normalToggle.onValueChanged.AddListener((isOn =>
        {
            if (isOn) GameManager.Instance.SetDifficultyLevel(2);
        }));
        hardToggle.onValueChanged.AddListener((isOn =>
        {
            if (isOn) GameManager.Instance.SetDifficultyLevel(3);
        }));
        if (easyToggle.isOn)
        {
            GameManager.Instance.SetDifficultyLevel(1);
        }
        else if (normalToggle.isOn)
        {
            GameManager.Instance.SetDifficultyLevel(2);
        }
        else if (hardToggle.isOn)
        {
            GameManager.Instance.SetDifficultyLevel(3);
        }
    }
}