using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsSliders : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void OnEnable()
    {
        if (masterSlider)
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);

        if (bgmSlider)
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);

        if (sfxSlider)
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    private void OnDisable()
    {
        if (masterSlider)
            masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);

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
        if (AudioManager.Instance == null)
            return;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(AudioManager.Instance.masterVolume);

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(AudioManager.Instance.bgmVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
    }

    private void OnMasterSliderChanged(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
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
