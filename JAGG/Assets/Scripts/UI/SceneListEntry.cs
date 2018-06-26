using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using Ionic.Zip;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

public class SceneListEntry : MonoBehaviour {

    public Text buttonName;
    public Text labelVersion;
    public Text labelLevelName;
    public Text labelAuthor;
    public Image imagePreview;
    public Button buttonEntry;

    public LobbyControls lobbyControls;

	// Use this for initialization
	void Start() {
        buttonEntry.onClick.RemoveAllListeners();
        buttonEntry.onClick.AddListener(Selected);
	}
	

    public void SetUp(string name, LobbyControls lobbyControls)
    {
        buttonName.text = name;
        this.lobbyControls = lobbyControls;
        this.labelLevelName = lobbyControls.labelLevelName;
        this.imagePreview = lobbyControls.imageScenePreview;
        this.labelVersion = lobbyControls.labelVersion;
        this.labelAuthor = lobbyControls.labelAuthor;
    }

    public void Selected()
    {
        //StopAllCoroutines();
        labelLevelName.text = buttonName.text;
        lobbyControls.selectedScene = buttonName.text;
        lobbyControls.levelName = buttonName.text;

        lobbyControls.levelInfo = GetLevelInfo();

        labelAuthor.text = "Author : " + lobbyControls.levelInfo.author;
        labelVersion.text = "Version : "  + lobbyControls.levelInfo.version;

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

    private CustomLevel GetLevelInfo()
    {
        JObject json = null;
        string filename = Application.persistentDataPath + "/levels/" + buttonName.text + ".map";

        using (ZipFile mapFile = ZipFile.Read(filename))
        {
            using (MemoryStream s = new MemoryStream())
            {
                ZipEntry e = mapFile["level.json"];
                e.Extract(s);

                s.Seek(0, SeekOrigin.Begin);

                using (BsonReader br = new BsonReader(s))
                {
                    json = (JObject)JToken.ReadFrom(br);
                }
            }
        }

        return JsonUtility.FromJson<CustomLevel>(json.ToString());
    }
}
