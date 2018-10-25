using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationHole : MonoBehaviour {

    [Header("UI")]
    public Text validationMenuText;
    public Button testHoleButton;

    [Header("Inputs")]
    public InputField parInput;
    public InputField maxShotInput;
    public InputField timeInput;


    [Header("Other")]
    public EditorManager editorManager;

    void Start()
    {
        // Buttons callbacks
        testHoleButton.onClick.AddListener(TestHole);
    }


    void OnEnable()
    {
        // Inputs and text
        LevelProperties lp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
        int par = lp.par;
        int maxShot = lp.maxShot;
        float maxTime = lp.maxTime;
        parInput.text = par.ToString();
        maxShotInput.text = maxShot.ToString();
        timeInput.text = maxTime.ToString();

        validationMenuText.text = validationMenuText.text + editorManager.GetCurrentHoleNumber().ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TestHole()
    {
        editorManager.testMode.TestHole(true, true);
        this.gameObject.SetActive(false);
    }

    public void UpdateLevelProperties()
    {
        int par = 0;
        int.TryParse(parInput.text, out par);

        int maxshot = 0;
        int.TryParse(maxShotInput.text, out maxshot);

        int time = 0;
        int.TryParse(timeInput.text, out time);

        editorManager.UpdateLevelProperties(par, maxshot, time, editorManager.GetNextValidHole(-1));
    }
}
