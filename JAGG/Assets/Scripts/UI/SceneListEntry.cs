using UnityEngine.UI;
using UnityEngine;

public class SceneListEntry : MonoBehaviour {

    public Text buttonName;
    public Text labelLevelName;
    public Button buttonEntry;

    public LobbyControls lobbyControls;

	// Use this for initialization
	void Start() {
        buttonEntry.onClick.RemoveAllListeners();
        buttonEntry.onClick.AddListener(Selected);
	}
	

    public void SetUp(string name, Text labelLevelName, LobbyControls lobbyControls)
    {
        buttonName.text = name;
        this.labelLevelName = labelLevelName;
        this.lobbyControls = lobbyControls;
    }

    public void Selected()
    {
        labelLevelName.text = buttonName.text;
        lobbyControls.selectedScene = buttonName.text;
        lobbyControls.levelName = buttonName.text;
    }
}
