﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour {

    public Text textShots;
    public Text notificationText;
    public GameObject notificationPanel;

    [Header("Slider")]
    public Slider slider;
    public int minSliderVal = 10;
    public int maxSliderVal = 150;

    [Header("Score")]
    public GameObject panelScore;
    public GameObject holeEntry;
    public PlayerScoreEntry[] scorePlayers;
    public GameObject holes;

    [Header("Pause")]
    public GameObject panelPause;
    public Button buttonQuit;
    public Button buttonReturn;

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

    public void SetParList()
    {
        foreach(Transform hole in holes.transform)
        {
            int par = hole.gameObject.GetComponentInChildren<LevelProperties>().par;
            holeEntry.GetComponent<PlayerScoreEntry>().AddScore(par);
        }
    }

    public void ShowScores()
    {
        panelScore.SetActive(true);

        List<SyncListInt> scores = playerManager.GetPlayersScore();

        for(int i = 0; i < scores.Count; i++)
        {
            if (!scorePlayers[i].gameObject.activeSelf)
                scorePlayers[i].gameObject.SetActive(true);

            scorePlayers[i].CleanScores();

            int total = 0;

            for(int j = 0; j < scores[i].Count; j++)
            {
                total += scores[i][j];
                scorePlayers[i].AddScore(scores[i][j]);
            }

            scorePlayers[i].SetTotal(total);
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

        SettingsManager._instance.SetBackSettings(panelPause);

        buttonReturn.onClick.RemoveAllListeners();
        buttonReturn.onClick.AddListener(HidePauseMenu);
        buttonReturn.onClick.AddListener(returnCallback);

        buttonQuit.onClick.RemoveAllListeners();
        buttonQuit.onClick.AddListener(HidePauseMenu);
        buttonQuit.onClick.AddListener(quitCallback);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public IEnumerator ShowNotification(string message, float time, Action callback = null)
    {
        notificationPanel.SetActive(true);
        notificationText.text = message;
        notificationText.enabled = true;
        yield return new WaitForSeconds(time);
        notificationText.enabled = false;
        notificationPanel.SetActive(false);

        if (callback != null)
            callback();
    }
}
