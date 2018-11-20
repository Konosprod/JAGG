using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationFailHole : MonoBehaviour {

    [Header("UI")]
    public Button retestHoleButton;
    public Text shotsText;
    public Text timeText;

    [Header("Inputs")]
    public InputField parInputLast;
    public InputField maxShotInputLast;
    public InputField timeInputLast;

    [Header("Other")]
    public EditorManager editorManager;

    void Start()
    {
        // Buttons callbacks
        retestHoleButton.onClick.AddListener(RetestHole);
    }


    void OnEnable()
    {
        // Inputs and text
        LevelProperties lpLast = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
        int par = lpLast.par;
        int maxShot = lpLast.maxShot;
        float maxTime = lpLast.maxTime;
        parInputLast.text = par.ToString();
        maxShotInputLast.text = maxShot.ToString();
        timeInputLast.text = maxTime.ToString();
    }

    public void SetTestResults(int shots, float time)
    {
        shotsText.text = shotsText.text.Split(':')[0] + ": " + shots;
        timeText.text = timeText.text.Split(':')[0] + ": " + time.ToString("0.##") + "s";
    }

    public void RetestHole()
    {
        editorManager.testMode.ValidationReplayHole();
        this.gameObject.SetActive(false);
    }

    public void UpdateLevelProperties()
    {
        int par = 0;
        int.TryParse(parInputLast.text, out par);

        int maxshot = 0;
        int.TryParse(maxShotInputLast.text, out maxshot);

        int time = 0;
        int.TryParse(timeInputLast.text, out time);

        editorManager.UpdateLevelProperties(par, maxshot, time);
    }
}
