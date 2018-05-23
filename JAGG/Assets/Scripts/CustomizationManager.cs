using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour {

    public ParticleSystem trailParticle;
    public GameObject panelUI;
    public GameObject ball;
    public Slider slider;
    public ColorPicker colorPicker;

    private Color selectedColor;
    private bool isStarted = false;

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
        if(isStarted)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                panelUI.SetActive(true);
                isStarted = false;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;

                ball.SetActive(false);
                slider.gameObject.SetActive(false);
                Camera.main.GetComponent<BallCamera>().enabled = false;
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                //Save();
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    public void StartTest()
    {
        isStarted = true;
        panelUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ball.SetActive(true);
        slider.gameObject.SetActive(true);
        Camera.main.GetComponent<BallCamera>().enabled = true;
    }

    public void Save()
    {
        SettingsManager._instance.gameSettings.colorTrail = selectedColor;
        SettingsManager._instance.SaveSettings();
    }
}
