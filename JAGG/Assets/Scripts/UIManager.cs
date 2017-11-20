using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private bool slideUp = false;

    void Start()
    {
        slider.minValue = minSliderVal;
        slider.maxValue = maxSliderVal;
    }

    public void UpdateSlider()
    {
        if (slideUp)
            slider.value += 2;
        else
            slider.value -= 2;


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

    public IEnumerator ShowScores(float time, Action callback = null)
    {
        panelScore.SetActive(true);

        List<List<int>> scores = PlayerManager.Instance.GetPlayersScore();

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

        yield return new WaitForSeconds(time);
        panelScore.SetActive(false);


        if (callback != null)
            callback();
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
