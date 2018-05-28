using SimpleJSON;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AuthenticationManager : MonoBehaviour {

    public bool isAuthenticated;
    public LoadingOverlay loadingOverlay;

    public string sessionCookie = "";

    public static AuthenticationManager _instance;

    private bool test = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(GetAuthentication());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator GetAuthentication()
    {
        loadingOverlay.gameObject.SetActive(true);
        loadingOverlay.PlayAnimation();
        loadingOverlay.messageText.text = "Authentification...";

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

    IEnumerator Authenticate()
    {
        byte[] ticket = new byte[1024];
        uint ticketSize = 0;
        SteamUser.GetAuthSessionTicket(ticket, 1000, out ticketSize);

        loadingOverlay.messageText.text = "Authentification...";

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
            loadingOverlay.messageText.text = "Error, retrying in 3 secs";
            yield return new WaitForSeconds(3);
            StartCoroutine(Authenticate());
        }
        else
        {
            JSONNode node = JSON.Parse(www.downloadHandler.text);

            if (node["auth"].AsBool)
            {
                sessionCookie = www.GetResponseHeader("Set-Cookie");
                isAuthenticated = true;
                loadingOverlay.StopAnimation();
                loadingOverlay.gameObject.SetActive(false);
            }
            else
            {
                loadingOverlay.messageText.text = "Error, retrying in 3 secs";
                Debug.Log("error while authenticating");
                isAuthenticated = false;
                loadingOverlay.StopAnimation();
                loadingOverlay.gameObject.SetActive(false);
                yield return new WaitForSeconds(3);
                StartCoroutine(Authenticate());
            }
        }

    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }
}
