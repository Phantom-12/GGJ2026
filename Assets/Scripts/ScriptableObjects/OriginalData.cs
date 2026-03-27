using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/OriginalData", order = 1)]
public class OriginalData : ScriptableObject
{
    [Header("初始的移动速度")]
    public float startMoveSpeed = 4f;
    [Header("游戏限时")]
    public int totalTime = 90;
}