using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{

    public ParticleSystem trailParticle;
    public GameObject panelUI;
    public ColorPicker colorPicker;

    private Color selectedColor;

    private void Start()
    {
        selectedColor = SettingsManager._instance.gameSettings.colorTrail;
        colorPicker.CurrentColor = selectedColor;
    }

    public void OnColorChanged(Color newColor)
    {
        selectedColor = newColor;
        trailParticle.GetComponent<Renderer>().material.SetColor("_TintColor", newColor);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Save();
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void Save()
    {
        SettingsManager._instance.gameSettings.colorTrail = selectedColor;
        SettingsManager._instance.SaveSettings();
    }
}
