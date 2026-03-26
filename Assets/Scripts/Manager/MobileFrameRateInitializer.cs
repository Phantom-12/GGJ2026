using UnityEngine;

public static class MobileFrameRateInitializer
{
    private const int MobileDefaultFrameRate = 60;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
#if UNITY_ANDROID || UNITY_IOS
        Application.targetFrameRate = MobileDefaultFrameRate;
#endif
    }
}
