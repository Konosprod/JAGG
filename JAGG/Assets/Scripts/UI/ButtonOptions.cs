using UnityEngine.UI;
using UnityEngine;

public class ButtonOptions : MonoBehaviour {

    private SettingsManager settingsManager;
    public Button optionsButton;
    public GameObject panelReturn;

	// Use this for initialization
	void Start () {
        settingsManager = SettingsManager._instance;

        optionsButton.onClick.RemoveListener(ShowOptionsMenu);
        optionsButton.onClick.AddListener(ShowOptionsMenu);

        if (panelReturn)
            settingsManager.SetBackSettings(panelReturn);
        else
            settingsManager.SetBackSettings(null);
    }

    private void ShowOptionsMenu()
    {
        settingsManager.ShowOptionsMenu(true);
    }
}
