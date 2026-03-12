using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsSliders : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (bgmSlider)
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
    
        if (sfxSlider)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }
    
    private void OnDisable()
    {
        if (bgmSlider)
            bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
    
        if (sfxSlider)
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
    }

    private void Start()
    {
        SyncSliderValues();
    }

    private void SyncSliderValues()
    {
        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.bgmVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
    }

    private void OnBgmSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(value);
    }
}
