using System.Collections;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class PanelExport : MonoBehaviour {

    public Image imagePreview;
    public InputField tagsInput;
    public InputField nameInput;
    public Button saveLocalButton;
    public Button uploadButton;

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
        StartCoroutine(GetAuthentication());
    }

    public void UploadMap()
    {
        StartCoroutine(Upload());
        //this.gameObject.SetActive(false);
    }

    IEnumerator Authenticate()
    {
        Debug.Log("Authenticate");
        byte[] ticket = new byte[1024];
        uint ticketSize = 0;
        SteamUser.GetAuthSessionTicket(ticket, 1000, out ticketSize);

        WWWForm form = new WWWForm();
        form.AddField("ticket", ByteArrayToString(ticket));
        form.AddField("steamid", (SteamUser.GetSteamID().m_SteamID).ToString());

        UnityWebRequest www = UnityWebRequest.Post("https://jagg.konosprod.fr/api/auth", form);
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
                Debug.Log("Authenticated");
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
        Debug.Log("GetAuthentication");
        UnityWebRequest request = UnityWebRequest.Get("https://jagg.konosprod.fr/api/auth");

        yield return request.SendWebRequest();


        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            Debug.Log(request.responseCode);
        }
        else
        {
            JSONNode node = JSON.Parse(request.downloadHandler.text);
            isAuthenticated = node["auth"].AsBool;

            if (!isAuthenticated)
            {
                Debug.Log("Not Authenticated");
                StartCoroutine(Authenticate());
            }
            else
            {
                Debug.Log("authenticated");
            }
        }
    }

    IEnumerator Upload()
    {
        WWWForm data = new WWWForm();

        byte[] fileData = System.IO.File.ReadAllBytes(@"C:\Users\Kono\Desktop\TestLevel.map");

        data.AddBinaryData("map", fileData, nameInput.text+".map");
        data.AddBinaryData("thumb", imagePreview.sprite.texture.EncodeToJPG());
        data.AddField("tag", tagsInput.text);
        data.AddField("steamid", Steamworks.SteamUser.GetSteamID().m_SteamID.ToString());
        data.AddField("name", "Kono");

        UnityWebRequest uwr = UnityWebRequest.Post("https://jagg.konosprod.fr/api/maps", data);

        uwr.SetRequestHeader("Cookie", sessionCookie);
        //uwr.chunkedTransfer = false;
        uwr.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

        AsyncOperation request = uwr.SendWebRequest();

        while (!request.isDone)
        {
            Debug.Log(request.progress * 100);
            yield return null;
        }


        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
            Debug.Log(uwr.responseCode);
        }
        else
        {
            Debug.Log("done");
        }
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
