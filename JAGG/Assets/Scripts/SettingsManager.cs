using System;
using UnityEngine.Events;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour {

    public GameObject optionsPanel;

    public Toggle fullscreenToggle;
    public Dropdown resolutionsDropdown;
    public Dropdown textureQualityDropdown;
    public Dropdown antialiasingDropdown;
    public Dropdown vsyncDropdown;
    public Dropdown accurateModeDropdown;
    public Slider BGMvolumeSlider;
    public Slider SFXvolumeSlider;
    public Slider SensibilitySlider;
    public Slider AccurateSensibilitySlider;

    public Button backButton;
    private GameObject returnPanel;
    private UnityAction backCallback;

    private Resolution[] resolutions;
    public GameSettings gameSettings;

    private string SettingsPath;

    public static SettingsManager _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialise()
    {
        if (_instance == null)
        {
            Instantiate(Resources.Load("Prefabs/SettingsManager"));
        }
    }

    void Start()
    {
        resolutions = Screen.resolutions;

        SettingsPath = Application.persistentDataPath + "/gamesettings.json";

        gameSettings = new GameSettings();

        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);

        resolutionsDropdown.onValueChanged.RemoveAllListeners();
        resolutionsDropdown.onValueChanged.AddListener(OnResolutionChange);

        textureQualityDropdown.onValueChanged.RemoveAllListeners();
        textureQualityDropdown.onValueChanged.AddListener(OnTextureQualityChange);

        antialiasingDropdown.onValueChanged.RemoveAllListeners();
        antialiasingDropdown.onValueChanged.AddListener(OnAntialiasingChange);

        vsyncDropdown.onValueChanged.RemoveAllListeners();
        vsyncDropdown.onValueChanged.AddListener(OnVsyncChange);

        BGMvolumeSlider.onValueChanged.RemoveAllListeners();
        BGMvolumeSlider.onValueChanged.AddListener(OnBGMVolumeChange);

        SFXvolumeSlider.onValueChanged.RemoveAllListeners();
        SFXvolumeSlider.onValueChanged.AddListener(OnSFXVolumeChange);

        SensibilitySlider.onValueChanged.RemoveAllListeners();
        SensibilitySlider.onValueChanged.AddListener(OnSensibilityChanged);

        AccurateSensibilitySlider.onValueChanged.RemoveAllListeners();
        AccurateSensibilitySlider.onValueChanged.AddListener(OnAccurateSensibilityChanged);

        accurateModeDropdown.onValueChanged.RemoveAllListeners();
        accurateModeDropdown.onValueChanged.AddListener(OnAccurateModeChanged);

        resolutionsDropdown.ClearOptions();

        foreach (Resolution r in resolutions)
        {
            resolutionsDropdown.options.Add(new Dropdown.OptionData(r.ToString()));
        }

        LoadSettings();

        resolutionsDropdown.RefreshShownValue();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ShowReturnPanel();
        }
    }

    public void OnAccurateModeChanged(int newMode)
    {
        gameSettings.AccurateMode = newMode;
    }

    public void OnFullscreenToggle(bool newFullscreen)
    {
        gameSettings.Fullscreen = Screen.fullScreen = newFullscreen;
    }

    public void OnResolutionChange(int newResolution)
    {
        Screen.SetResolution(resolutions[newResolution].width, resolutions[newResolution].height, Screen.fullScreen);
        gameSettings.Resolution = newResolution;
    }

    public void OnTextureQualityChange(int newTextureQuality)
    {
        gameSettings.TextureQuality = QualitySettings.masterTextureLimit = newTextureQuality;
    }

    public void OnAntialiasingChange(int newAntialiasing)
    {
        gameSettings.Antialiasing = QualitySettings.antiAliasing = (int) Mathf.Pow(2, newAntialiasing);
    }

    public void OnVsyncChange(int newVsync)
    {
        gameSettings.VSync = QualitySettings.vSyncCount = newVsync;
    }

    public void OnBGMVolumeChange(float newVolume)
    {
        gameSettings.BGMAudioVolume = newVolume;
        SoundManager._instance.SetBGMVolume(newVolume);

        if (newVolume == BGMvolumeSlider.minValue)
            SoundManager._instance.MuteBGM();
    }

    public void OnSFXVolumeChange(float newVolume)
    {
        gameSettings.SFXAudioVolume = newVolume;
        SoundManager._instance.SetSFXVolume(newVolume);

        if (newVolume == SFXvolumeSlider.minValue)
            SoundManager._instance.MuteSFX();
    }

    public void OnSensibilityChanged(float newSensibility)
    {
        gameSettings.Sensibility = newSensibility;
    }

    public void OnAccurateSensibilityChanged(float newAccurateSensibility)
    {
        gameSettings.AccurateSensibility = newAccurateSensibility;
    }


    public void LoadSettings()
    {

        try
        {
            string[] jsons = File.ReadAllLines(SettingsPath);
            gameSettings = JsonUtility.FromJson<GameSettings>(jsons[0]);

            fullscreenToggle.isOn = gameSettings.Fullscreen;
            resolutionsDropdown.value = gameSettings.Resolution;
            textureQualityDropdown.value = gameSettings.TextureQuality;
            antialiasingDropdown.value = gameSettings.Antialiasing;
            vsyncDropdown.value = gameSettings.VSync;
            BGMvolumeSlider.value = gameSettings.BGMAudioVolume;
            SFXvolumeSlider.value = gameSettings.SFXAudioVolume;
            SensibilitySlider.value = gameSettings.Sensibility;
            AccurateSensibilitySlider.value = gameSettings.AccurateSensibility;
            accurateModeDropdown.value = gameSettings.AccurateMode;

            gameSettings.Keys = JsonConvert.DeserializeObject<Dictionary<KeyAction, KeyCode>>(jsons[1]);
        }
        catch(Exception e)
        {
            fullscreenToggle.isOn = false;
            resolutionsDropdown.value = 0;
            textureQualityDropdown.value = 0;
            antialiasingDropdown.value = 0;
            vsyncDropdown.value = 0;
            BGMvolumeSlider.value = 1f;
            SFXvolumeSlider.value = 1f;
            SensibilitySlider.value = 100f;
            AccurateSensibilitySlider.value = 100f;
            accurateModeDropdown.value = 0;

            Debug.Log(e);
        }
    }

    public void SaveSettings()
    {
        File.WriteAllText(SettingsPath, JsonUtility.ToJson(gameSettings, false)+"\n");
        File.AppendAllText(SettingsPath, JsonConvert.SerializeObject(gameSettings.Keys));
    }

    public void ShowOptionsMenu(bool show)
    {
        optionsPanel.SetActive(show);
    }

    public void SetBackSettings(GameObject returnPanel, UnityAction callback = null)
    {
        this.returnPanel = returnPanel;
        this.backCallback = callback;

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(ShowReturnPanel);
    }

    private void ShowReturnPanel()
    {
        optionsPanel.SetActive(false);

        if (backCallback != null)
            backCallback.Invoke();

        if (returnPanel != null)
            returnPanel.SetActive(true);
    }

    public void ShowMapDirectory()
    {
        string path = Path.Combine(Application.persistentDataPath, "Levels/");
        path = path.Replace(@"/", @"\");   // explorer doesn't like front slashes
        Debug.Log(path);
        System.Diagnostics.Process.Start("explorer.exe", "" + path);
    }
}
