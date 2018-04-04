using System.Collections;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System;
using System.IO;

public class PanelExport : MonoBehaviour {

    public Canvas canvasUi;
    public Image imagePreview;
    public InputField tagsInput;
    public InputField nameInput;
    public Button saveLocalButton;
    public Button uploadButton;
    public GameObject grid;

    public ExportLevel levelExporter;

    private bool isAuthenticated = false;
    private string sessionCookie = "";

    // Use this for initialization
    void Start () {
        uploadButton.onClick.AddListener(UploadMap);
        saveLocalButton.onClick.AddListener(SaveLocal);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable()
    {
        StartCoroutine(TakeScreenShot());
        StartCoroutine(GetAuthentication());
    }

    public void UploadMap()
    {
        levelExporter.CreateCustomLevel(nameInput.text, SteamFriends.GetPersonaName(), Path.Combine(Application.persistentDataPath, "Levels"));
        StartCoroutine(Upload());
    }

    IEnumerator Authenticate()
    {
        byte[] ticket = new byte[1024];
        uint ticketSize = 0;
        SteamUser.GetAuthSessionTicket(ticket, 1000, out ticketSize);

        WWWForm form = new WWWForm();
        form.AddField("ticket", ByteArrayToString(ticket));
        form.AddField("steamid", (SteamUser.GetSteamID().m_SteamID).ToString());

        UnityWebRequest www = UnityWebRequest.Post("https://jagg.konosprod.fr/api/auth", form);
        www.SetRequestHeader("Cookie", sessionCookie);
        www.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            JSONNode node = JSON.Parse(www.downloadHandler.text);

            if (node["auth"].AsBool)
            {
                sessionCookie = www.GetResponseHeader("Set-Cookie");
                isAuthenticated = true;
            }
            else
            {
                Debug.Log("error while authenticating");
                isAuthenticated = false;
            }
        }
    }


    IEnumerator GetAuthentication()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://jagg.konosprod.fr/api/auth");
        request.SetRequestHeader("Cookie", sessionCookie);
        request.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            Debug.Log(request.responseCode);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            JSONNode node = JSON.Parse(request.downloadHandler.text);
            isAuthenticated = node["auth"].AsBool;

            if (!isAuthenticated)
            {
                StartCoroutine(Authenticate());
            }
            else
            {

            }
        }
    }

    IEnumerator Upload()
    {
        string filename = nameInput.text + ".map";
        WWWForm data = new WWWForm();

        byte[] fileData = System.IO.File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "Levels") + "/" + filename);

        data.AddBinaryData("map", fileData, filename);
        data.AddBinaryData("thumb", imagePreview.sprite.texture.EncodeToJPG());
        data.AddField("tags", tagsInput.text);
        data.AddField("steamid", Steamworks.SteamUser.GetSteamID().m_SteamID.ToString());
        data.AddField("name", SteamFriends.GetPersonaName());

        UnityWebRequest uwr = UnityWebRequest.Post("https://jagg.konosprod.fr/api/maps", data);

        uwr.SetRequestHeader("Cookie", sessionCookie);
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
        }
        else
        {
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
        this.gameObject.SetActive(false);
    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
}
