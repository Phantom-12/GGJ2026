using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject desktopButton;
    [SerializeField] private GameObject mobileButton;

    private void Awake()
    {
        ApplyPlatformButton();
    }

    public void ApplyPlatformButton()
    {
        mobileButton.SetActive(Application.isMobilePlatform);
        desktopButton.SetActive(!Application.isMobilePlatform);
    }
}
