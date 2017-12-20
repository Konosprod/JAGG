using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

    public Toggle fullscreenToggle;
    public Dropdown resolutionsDropdown;
    public Dropdown textureQualityDropdown;
    public Dropdown antialiasingDropdown;
    public Dropdown vsyncDropdown;
    public Slider volumeSlider;

    private Resolution[] resolutions;
    private GameSettings gameSettings;

    void OnEnable()
    {
        resolutions = Screen.resolutions;

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

        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);

        resolutionsDropdown.ClearOptions();

        foreach(Resolution r in resolutions)
        {
            resolutionsDropdown.options.Add(new Dropdown.OptionData(r.ToString()));
        }

        LoadSettings();

        resolutionsDropdown.RefreshShownValue();
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

    public void OnVolumeChange(float newVolume)
    {
        //Set volume here
        //SoundManager.setMasterVolume();
        gameSettings.AudioVolume = newVolume;
    }

    public void LoadSettings()
    {
        try
        {
            gameSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + "/gamesettings.json"));

            fullscreenToggle.isOn = gameSettings.Fullscreen;
            resolutionsDropdown.value = gameSettings.Resolution;
            textureQualityDropdown.value = gameSettings.TextureQuality;
            antialiasingDropdown.value = gameSettings.Antialiasing;
            vsyncDropdown.value = gameSettings.VSync;
            volumeSlider.value = gameSettings.AudioVolume;
        }
        catch(Exception e)
        {
            Debug.Log(e);
            fullscreenToggle.isOn = false;
            resolutionsDropdown.value = 0;
            textureQualityDropdown.value = 0;
            antialiasingDropdown.value = 0;
            vsyncDropdown.value = 0;
            volumeSlider.value = 1f;

        }
    }

    public void SaveSettings()
    {
        File.WriteAllText(Application.persistentDataPath + "/gamesettings.json", JsonUtility.ToJson(gameSettings, true));
    }
}
