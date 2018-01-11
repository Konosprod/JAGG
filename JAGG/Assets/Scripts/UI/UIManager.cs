using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

    public Text textShots;
    public Text notificationText;

    [Header("Slider")]
    public Slider slider;
    public int minSliderVal = 10;
    public int maxSliderVal = 150;

    [Header("Score")]
    public GameObject panelScore;
    public GameObject[] scorePlayers;

    [Header("Pause")]
    public GameObject panelPause;
    public Button buttonQuit;
    public Button buttonReturn;

    private UnityAction returnCallback;

    private bool slideUp = false;
    private PlayerManager playerManager;

    void Start()
    {
        slider.minValue = minSliderVal;
        slider.maxValue = maxSliderVal;

        playerManager = FindObjectOfType<PlayerManager>();
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            if (returnCallback != null)
                returnCallback.Invoke();

            HidePauseMenu();
        }
    }

    public void UpdateSlider()
    {
        if (slideUp)
            slider.value += 1;
        else
            slider.value -= 1;


        // Start moving the other way when we reach either end otherwise keep moving in the same direction
        slideUp = (slider.value >= maxSliderVal) ? false : (slider.value <= minSliderVal) ? true : slideUp;
    }

    public float GetSliderValue()
    {
        return slider.value;
    }

    public void ResetSlider()
    {
        slideUp = true;
        slider.value = minSliderVal;
    }

    public void SetTextShots(string text)
    {
        textShots.text = text;
    }

    public void HideScores()
    {
        panelScore.SetActive(false);
    }

    public void ShowScores()
    {
        panelScore.SetActive(true);

        List<SyncListInt> scores = playerManager.GetPlayersScore();

        for(int i = 0; i < scores.Count; i++)
        {
            if (!scorePlayers[i].activeSelf)
                scorePlayers[i].SetActive(true);

            string text = "";
            int total = 0;

            for(int j = 0; j < scores[i].Count; j++)
            {
                text += scores[i][j].ToString() + " ";
                total += scores[i][j];
            }

            scorePlayers[i].GetComponentsInChildren<Text>()[1].text = text;
            scorePlayers[i].GetComponentsInChildren<Text>()[2].text = total.ToString();
        }
    }

    public void HidePauseMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        panelPause.SetActive(false);
    }

    public void ShowPause(UnityAction returnCallback = null, UnityAction quitCallback = null)
    {
        panelPause.SetActive(true);
        this.returnCallback = returnCallback;

        SettingsManager._instance.SetBackSettings(panelPause);

        buttonReturn.onClick.RemoveAllListeners();
        buttonReturn.onClick.AddListener(returnCallback);
        buttonReturn.onClick.AddListener(HidePauseMenu);

        buttonQuit.onClick.RemoveAllListeners();
        buttonQuit.onClick.AddListener(HidePauseMenu);
        buttonQuit.onClick.AddListener(quitCallback);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public IEnumerator ShowNotification(string message, float time, Action callback = null)
    {
        notificationText.text = message;
        notificationText.enabled = true;
        yield return new WaitForSeconds(time);
        notificationText.enabled = false;

        if (callback != null)
            callback();
    }
}
