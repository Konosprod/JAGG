using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class SceneListEntry : MonoBehaviour {

    public Text buttonName;
    public Text labelLevelName;
    public Image imagePreview;
    public Button buttonEntry;

    public LobbyControls lobbyControls;

	// Use this for initialization
	void Start() {
        buttonEntry.onClick.RemoveAllListeners();
        buttonEntry.onClick.AddListener(Selected);
	}
	

    public void SetUp(string name, Text labelLevelName, Image imagePreview, LobbyControls lobbyControls)
    {
        buttonName.text = name;
        this.labelLevelName = labelLevelName;
        this.lobbyControls = lobbyControls;
        this.imagePreview = imagePreview;
    }

    public void Selected()
    {
        //StopAllCoroutines();
        labelLevelName.text = buttonName.text;
        lobbyControls.selectedScene = buttonName.text;
        lobbyControls.levelName = buttonName.text;

        Regex r = new Regex(@"(\d+)_*");
        string id = r.Match(buttonName.text).Groups[1].Value;

        StartCoroutine(LoadMapPreview(id));
    }

    IEnumerator LoadMapPreview(string id)
    {
        WWW www = new WWW("https://jagg-api.konosprod.fr/thumbs/" + id + ".png");
        yield return www;
        imagePreview.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }
}
