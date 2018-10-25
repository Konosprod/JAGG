using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationBetweenHole : MonoBehaviour {

    [Header("UI")]
    public Button retestHoleButton;
    public Button testNextHoleButton;
    public Text shotsText;
    public Text timeText;

    [Header("Inputs")]
    public InputField parInputLast;
    public InputField maxShotInputLast;
    public InputField timeInputLast;

    public InputField parInputNext;
    public InputField maxShotInputNext;
    public InputField timeInputNext;


    [Header("Other")]
    public EditorManager editorManager;

    void Start()
    {
        // Buttons callbacks
        retestHoleButton.onClick.AddListener(RetestHole);
        testNextHoleButton.onClick.AddListener(TestNextHole);
    }


    void OnEnable()
    {
        // Inputs and text
        LevelProperties lpLast = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
        int parLast = lpLast.par;
        int maxShotLast = lpLast.maxShot;
        float maxTimeLast = lpLast.maxTime;
        parInputLast.text = parLast.ToString();
        maxShotInputLast.text = maxShotLast.ToString();
        timeInputLast.text = maxTimeLast.ToString();

        LevelProperties lpNext = editorManager.GetNextHoleLevelProp().GetComponent<LevelProperties>();
        int parNext = lpNext.par;
        int maxShotNext = lpNext.maxShot;
        float maxTimeNext = lpNext.maxTime;
        parInputNext.text = parNext.ToString();
        maxShotInputNext.text = maxShotNext.ToString();
        timeInputNext.text = maxTimeNext.ToString();
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

    public void TestNextHole()
    {
        editorManager.testMode.ValidationGoToNextHole();
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

    public void UpdateNextHoleLevelProperties()
    {
        int par = 0;
        int.TryParse(parInputNext.text, out par);

        int maxshot = 0;
        int.TryParse(maxShotInputNext.text, out maxshot);

        int time = 0;
        int.TryParse(timeInputNext.text, out time);

        editorManager.UpdateLevelProperties(par, maxshot, time, editorManager.GetNextValidHole(editorManager.GetCurrentHoleNumber()));
    }
}
