using UnityEngine.UI;
using UnityEngine;

public class ButtonOptions : MonoBehaviour {

    private SettingsManager settingsManager;
    public Button optionsButton;

	// Use this for initialization
	void Start () {
        settingsManager = SettingsManager._instance;

        optionsButton.onClick.RemoveListener(ShowOptionsMenu);
        optionsButton.onClick.AddListener(ShowOptionsMenu);

        settingsManager.SetBackSettings(optionsButton.transform.parent.gameObject);
    }

    private void ShowOptionsMenu()
    {
        settingsManager.ShowOptionsMenu(true);
    }
}
