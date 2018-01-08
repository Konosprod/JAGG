using UnityEngine.UI;
using UnityEngine;

public class SceneListEntry : MonoBehaviour {

    public Text labelName;
    public Button buttonEntry;

    public Text labelTitle;

    public LobbyControls lobbyControls;

	// Use this for initialization
	void Start() {
        buttonEntry.onClick.RemoveAllListeners();
        buttonEntry.onClick.AddListener(Selected);
	}
	

    public void SetUp(string name, Text levelName, LobbyControls lobbyControls)
    {
        labelName.text = name;
        labelTitle = levelName;
        this.lobbyControls = lobbyControls;
    }

    public void Selected()
    {
        labelTitle.text = labelName.text;

        lobbyControls.selectedScene = labelName.text;
    }
}
