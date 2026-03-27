using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PutDownAutoAlignConfig", order = 1)]
public class PutDownAutoAlignConfig : ScriptableObject
{
    [Header("放下时自动完全对齐的距离阈值")]
    [Min(0f)]
    public float autoAlignDistance = 0.03f;
}
