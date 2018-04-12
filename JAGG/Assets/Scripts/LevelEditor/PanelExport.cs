using System.Collections;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Ionic.Zip;

public class PanelExport : MonoBehaviour {

    [Header("UI")]
    public Canvas canvasUi;
    public Image imagePreview;
    public InputField tagsInput;
    public InputField nameInput;
    public Button saveLocalButton;
    public Button uploadButton;
    public GameObject grid;
    public EditorManager editorManager;

    public ExportLevel levelExporter;

    [Header("DEBUG")]
    [Tooltip("Should be true on production")]
    public bool checkHoleValidity = false;

    private ZipFile mapFile;
    private MemoryStream ms;


    public string mapName = "";
    public string steamid = "";
    public int mapid = 0;

    private AuthenticationManager authenticationManager;


    // Use this for initialization
    void Start () {
        uploadButton.onClick.AddListener(UploadMap);
        saveLocalButton.onClick.AddListener(SaveLocal);

        authenticationManager = AuthenticationManager._instance;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable()
    {
        editorManager.canEdit = false;
        if(mapName != "")
        {
            nameInput.text = mapName;
            nameInput.readOnly = true;
            nameInput.interactable = false;
        }

        StartCoroutine(TakeScreenShot());
    }

    void OnDisable()
    {
        editorManager.canEdit = true;
    }

    public void UploadMap()
    {
        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Levels/local"));
        byte[] binary = levelExporter.CreateCustomLevel(nameInput.text, SteamFriends.GetPersonaName(), checkHoleValidity);

        if (binary != null)
        {
            ms = new MemoryStream(binary);
            StartCoroutine(Upload());
        }
        else
        {
            Debug.Log("All holes are not valid");
        }
    }

    IEnumerator Upload()
    {
        string filename = nameInput.text + ".map";
        WWWForm data = new WWWForm();

        mapFile = ZipFile.Read(ms);

        ZipEntry jsonFile = mapFile["level.json"];
        MemoryStream s = new MemoryStream();
        jsonFile.Extract(s);
        s.Seek(0, SeekOrigin.Begin);
        JObject json;

        using (BsonReader br = new BsonReader(s))
        {
            json = (JObject)JToken.ReadFrom(br);
        }

        if(mapid != 0 && steamid != "")
        {
            json.Add("mapid", mapid);
            json.Add("steamid", steamid);
        }

        s = new MemoryStream();
        BsonWriter bw = new BsonWriter(s);
        json.WriteTo(bw);
        s.Flush();
        s.Seek(0, SeekOrigin.Begin);
        mapFile.UpdateEntry("level.json", s.ToArray());

        ms = new MemoryStream();
        mapFile.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);

        data.AddBinaryData("map", ms.ToArray(), filename);
        data.AddBinaryData("thumb", imagePreview.sprite.texture.EncodeToJPG());
        if(tagsInput.text != "")
            data.AddField("tags", tagsInput.text);
        data.AddField("steamid", Steamworks.SteamUser.GetSteamID().m_SteamID.ToString());
        data.AddField("name", SteamFriends.GetPersonaName());

        UnityWebRequest uwr = null;

        if (mapid == 0)
            uwr = UnityWebRequest.Post("https://jagg.konosprod.fr/api/maps", data);
        else
            uwr = UnityWebRequest.Post("https://jagg.konosprod.fr/api/maps/" + mapid.ToString(), data);

        uwr.SetRequestHeader("Cookie", authenticationManager.sessionCookie);
        uwr.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        AsyncOperation request = uwr.SendWebRequest();

        while (!request.isDone)
        {
            yield return null;
        }


        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
            Debug.Log(uwr.responseCode);
            Debug.Log(uwr.downloadHandler.text);
        }
        else
        {
            JSONNode n = JSON.Parse(uwr.downloadHandler.text);
            string mapId = n["id"];
            mapFile = ZipFile.Read(ms);

            int.TryParse(mapId, out mapid);
            steamid = n["author"]["steamid"];

            if (mapid != 0 && json["mapid"] == null)
                json.Add("mapid", mapid);
            if(steamid != "" && json["steamid"] == null)
                json.Add("steamid", steamid);

            Debug.Log(json);

            s = new MemoryStream();
            bw = new BsonWriter(s);
            json.WriteTo(bw);

            mapFile.UpdateEntry("level.json", s.ToArray());

            mapFile.Save(Path.Combine(Application.persistentDataPath, "Levels") + "/" + mapId + "_" + filename);

            File.Copy(Path.Combine(Application.persistentDataPath, "Levels") + "/" + mapId + "_" + filename, 
                Path.Combine(Application.persistentDataPath, "Levels/local") + "/" + filename, true);

            this.gameObject.SetActive(false);
        }
    }

    IEnumerator TakeScreenShot()
    {
        yield return null;
        canvasUi.enabled = false;
        grid.SetActive(false);
        yield return new WaitForEndOfFrame();

        Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();

        texture.Apply();
        imagePreview.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0f, 0f));
        grid.SetActive(true);
        canvasUi.enabled = true;
    }

    public void SaveLocal()
    {
        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "Levels/local"));
        string filename = nameInput.text + ".map";

        byte[] binary = levelExporter.CreateCustomLevel(nameInput.text, SteamFriends.GetPersonaName());
        ms = new MemoryStream(binary);
        mapFile = ZipFile.Read(ms);

        ZipEntry jsonFile = mapFile["level.json"];
        MemoryStream s = new MemoryStream();
        jsonFile.Extract(s);

        s.Seek(0, SeekOrigin.Begin);

        JObject json;

        using (BsonReader br = new BsonReader(s))
        {
            json = (JObject)JToken.ReadFrom(br);
        }

        if (mapid != 0 && json["mapid"] == null && steamid != "" && json["steamid"] == null)
        {
            json.Add("mapid", mapid);
            json.Add("steamid", steamid);
        }
        else
        {
            if (json["mapid"] != null)
                json["mapid"] = 0;
            else
                json.Add("mapid", 0);

            if (json["steamid"] != null)
                json["steamid"] = "";
            else
                json.Add("steamid", "");
        }

        s = new MemoryStream();
        BsonWriter bw = new BsonWriter(s);
        json.WriteTo(bw);
        mapFile.UpdateEntry("level.json", s.ToArray());

        ms = new MemoryStream();
        mapFile.Save(Path.Combine(Application.persistentDataPath, "Levels/local") + "/" + filename);

        this.gameObject.SetActive(false);
    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
}
