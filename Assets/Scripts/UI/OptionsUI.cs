using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the options menu...
/// </summary>
public class OptionsUI : UIComponent
{
    public const string SFX_VOLUME_PREF_KEY = "SFXVolume";
    public const string MUSIC_VOLUME_PREF_KEY = "MusicVolume";
    public const string WINDOW_SHAKE_PREF_KEY = "WindowShakeEnabled";
    public const string CAMERA_SHAKE_PREF_KEY = "CameraShakeEnabled";

    public Canvas UICanvas;

    [SerializeField] private TextMeshProUGUI optionsText;

    [SerializeField] private TextMeshProUGUI cameraShakeText;
    [SerializeField] private Toggle cameraShakeToggle;

    [SerializeField] private TextMeshProUGUI windowShakeText;
    [SerializeField] private Toggle windowShakeToggle;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI sfxAmountText;

    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI musicAmountText;

    private string inactiveColor;
    private string activeColor;

    private SFXManager sfxManager;
    private MusicManager musicManager;

    public override void SetupUI()
    {
        LoadJsonSettings();
        InitializeUI();
        Show(false);
    }

    private void LoadJsonSettings()
    {
        inactiveColor = (string)JsonData["options"]["inactiveColor"];
        activeColor = (string)JsonData["options"]["activeColor"];
    }

    private void InitializeUI()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
        musicManager = FindFirstObjectByType<MusicManager>();

        optionsText.text = LocalizationManager.GetMessage("optionsLabel", UIJsonIdentifier);
        cameraShakeText.text = LocalizationManager.GetMessage("cameraShakeLabel", UIJsonIdentifier);
        windowShakeText.text = LocalizationManager.GetMessage("windowShakeLabel", UIJsonIdentifier);
        sfxText.text = LocalizationManager.GetMessage("sfxLabel", UIJsonIdentifier);
        musicText.text = LocalizationManager.GetMessage("musicLabel", UIJsonIdentifier);

        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        cameraShakeToggle.onValueChanged.AddListener(OnCameraShakeToggleChanged);
        windowShakeToggle.onValueChanged.AddListener(OnWindowShakeToggleChanged);

        InitializeCameraShakeSetting();
        InitializeWindowShakeSetting();

        ApplyInitialColors();
        LoadSliderValues();
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        if(!showUI) { return; }
        LoadSliderValues();
    }

    private void InitializeWindowShakeSetting()
    {
        bool windowShakeEnabled = PlayerPrefs.GetInt(WINDOW_SHAKE_PREF_KEY, 1) == 1;
        windowShakeToggle.isOn = windowShakeEnabled;
        ApplyWindowShakeSettings(windowShakeEnabled);
    }

    private void InitializeCameraShakeSetting()
    {
        bool cameraShakeEnabled = PlayerPrefs.GetInt(CAMERA_SHAKE_PREF_KEY, 1) == 1;
        cameraShakeToggle.isOn = cameraShakeEnabled;
        ApplyCameraShakeSettings(cameraShakeEnabled);
    }

    private void LoadSliderValues()
    {
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_PREF_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF_KEY, 1f);

        sfxSlider.value = Mathf.Clamp(Mathf.Pow(10, sfxVolume / 20f) * 100f, 0f, 100f);
        musicSlider.value = Mathf.Clamp(Mathf.Pow(10, musicVolume / 20f) * 100f, 0f, 100f);

        OnSFXSliderChanged(sfxSlider.value);
        OnMusicSliderChanged(musicSlider.value);
    }

    private void OnSFXSliderChanged(float value)
    {
        float actualValue = Mathf.Log10(Mathf.Lerp(0.0001f, 1f, value / 100f)) * 20f;
        ApplyTextColor(sfxSlider, "percentageLabel", sfxAmountText);
        sfxAmountText.text = sfxAmountText.text.Replace("%percentageAmount%", Mathf.RoundToInt(value).ToString());
        ApplySFXSettings(actualValue);
        ApplyTextColor(sfxSlider, "sfxLabel", sfxText);
        PlayerPrefs.SetFloat(SFX_VOLUME_PREF_KEY, actualValue);
    }

    private void OnMusicSliderChanged(float value)
    {
        float actualValue = Mathf.Log10(Mathf.Lerp(0.0001f, 1f, value / 100f)) * 20f;
        ApplyTextColor(musicSlider, "percentageLabel", musicAmountText);
        musicAmountText.text = musicAmountText.text.Replace("%percentageAmount%", Mathf.RoundToInt(value).ToString());
        ApplyMusicSettings(actualValue);
        ApplyTextColor(musicSlider, "musicLabel", musicText);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF_KEY, actualValue);
    }

    private void OnCameraShakeToggleChanged(bool isOn)
    {
        ApplyCameraShakeSettings(isOn);
        ApplyTextColor(cameraShakeToggle, "cameraShakeLabel", cameraShakeText);
        PlayerPrefs.SetInt(WINDOW_SHAKE_PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnWindowShakeToggleChanged(bool isOn)
    {
        ApplyWindowShakeSettings(isOn);
        ApplyTextColor(windowShakeToggle, "windowShakeLabel", windowShakeText);
        PlayerPrefs.SetInt(WINDOW_SHAKE_PREF_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplySFXSettings(float value)
    {
        sfxManager.UpdateMasterVolume(value);
        Debug.Log($"Applying SFX settings: {value}");
    }

    private void ApplyMusicSettings(float value)
    {
        musicManager.UpdateMasterVolume(value);
        Debug.Log($"Applying music settings: {value}");
    }

    private void ApplyCameraShakeSettings(bool isOn)
    {
        CameraShaker.ShakeEnabled(isOn);
        Debug.Log($"Applying camera shake settings: {isOn}");
    }

    private void ApplyWindowShakeSettings(bool isOn)
    {
        WindowShaker.SetWindowShakeEnabled(isOn);
        Debug.Log($"Applying window shake settings: {isOn}");
    }

    private void ApplyTextColor(Slider slider, string messageKey, TextMeshProUGUI labelText)
    {
        labelText.text = GetFormattedMessage(messageKey, slider.value == 0 ? inactiveColor : activeColor);
    }

    private string GetFormattedMessage(string messageKey, string color)
    {
        string message = LocalizationManager.GetMessage(messageKey, UIJsonIdentifier);
        return message.Replace("%color%", color);
    }

    private void ApplyTextColor(Toggle toggle, string messageKey, TextMeshProUGUI labelText)
    {
        labelText.text = GetFormattedMessage(messageKey, !toggle.isOn ? inactiveColor : activeColor);
    }

    private void ApplyInitialColors()
    {
        ApplyTextColor(sfxSlider, "sfxLabel", sfxText);
        ApplyTextColor(musicSlider, "musicLabel", musicText);
        ApplyTextColor(cameraShakeToggle, "cameraShakeLabel", cameraShakeText);
        ApplyTextColor(windowShakeToggle, "windowShakeLabel", windowShakeText);
    }

    public override string UIJsonIdentifier => "options_ui";
}
