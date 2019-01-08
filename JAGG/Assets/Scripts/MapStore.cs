using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Steamworks;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum SearchType { Map = 0, Tag = 1, Author = 2 };


//Auto load with scroll rect normalized position

public class MapStore : MonoBehaviour
{
    private const int pageLength = 15;

    [Header("Progressbar")]
    public Slider progressBar;
    public Text percentageText;
    public Text progressSizeText;

    [Header("List")]
    public InterractableScrollRect scrollRect;
    public GameObject listMap;
    public GameObject MapEntryPrefab;

    [Header("Tags")]
    public InterractableScrollRect tagScrollRect;
    public GameObject listTag;
    public GameObject tagPrefab;
    public InputField inputTag;

    [Header("Search")]
    public InputField searchInput;
    public Dropdown orderbyDropdown;
    public Dropdown sortDropdown;
    public Dropdown searchTypeDropdown;

    [Header("Other UI")]
    public Image imageMap;
    public LoadingOverlay loadingImageOverlay;
    public Text notification;
    public Text queryResults;
    public LoadingOverlay loadingOverlay;

    private int offset = 0;
    private int offsetSearching = 0;
    private int lastResultCount = 0;
    private string lastSearch = "";
    private bool loading = false;
    private bool searching = false;
    private bool myMaps = false;

    [HideInInspector]
    public StoreEntry selectedMapEntry;

    [HideInInspector]
    public bool isDownloading = false;

    private static string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    private static string[] orderbyTerms = { "id", "last_update", "monthly_download_count", "download_count" };
    private static string[] sortTerms = { "ASC", "DESC" };

    private AuthenticationManager authenticationManager;

    // Use this for initialization
    void Start()
    {
        authenticationManager = AuthenticationManager._instance;

        authenticationManager.CheckAuth(GetMaps);

        //GetMaps();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FilterMyMaps()
    {
        if (!myMaps)
        {
            myMaps = true;
            searching = true;
            CleanList();
            offsetSearching = 0;
            StartCoroutine(SearchMaps());
        }
        else
        {
            myMaps = false;
            searching = false;
            CleanList();
            offsetSearching = 0;
            StartCoroutine(GetNextMaps());
        }
    }

    void FixedUpdate()
    {
        if (scrollRect.verticalNormalizedPosition <= 0.1  && scrollRect.verticalNormalizedPosition != 0 && !loading && lastResultCount != 0)
        {
            loading = true;

            if(myMaps)
            {
                StartCoroutine(SearchMaps());
            }
            else if (searching)
            {
                Search(searchInput.text);
            }
            else
            {
                NextMaps();
            }
        }
    }


    private void GetMaps()
    {
        if (searching)
        {
            StartCoroutine(SearchMaps());
        }
        else
        {
            StartCoroutine(GetNextMaps());
        }
    }

    IEnumerator GetNextMaps()
    {
        loading = true;
        loadingOverlay.gameObject.SetActive(true);
        loadingOverlay.PlayAnimation();
        loadingOverlay.messageText.text = "Loading maps...";

        string url = ConfigHandler.ApiUrl + "/maps";

        url += "?offset=" + offset.ToString();
        url += "&orderby=" + orderbyTerms[orderbyDropdown.value];
        url += "&sort=" + sortTerms[sortDropdown.value];

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Cookie", authenticationManager.sessionCookie);

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

            foreach(JSONNode n in node.Children)
            {
                GameObject mapEntry = Instantiate(MapEntryPrefab.gameObject, listMap.transform);
                StoreEntry storeEntry = mapEntry.GetComponent<StoreEntry>();

                storeEntry.mapName = n["name"];
                storeEntry.author = n["author"]["name"];
                storeEntry.downloadUrl = n["path"];
                storeEntry.thumbUrl = ConfigHandler.BaseUrl + "/thumbs/" + n["id"] + ".png";
                storeEntry.mapId = n["id"];
                JSONArray tags = n["tags"].AsArray;

                foreach(JSONNode tag in tags.Values)
                {
                    storeEntry.tags.Add(tag["tag"]);
                }

                storeEntry.mapStore = this;
            }

            lastResultCount = node.Count;

            queryResults.text = listMap.transform.childCount + " results";
        }

        loadingOverlay.StopAnimation();
        loadingOverlay.gameObject.SetActive(false);

        if (lastResultCount >= pageLength)
            scrollRect.verticalNormalizedPosition = 0.5f;

        scrollRect.canInterract = true;
        loading = false;
        offset += pageLength;
    }

    public void AddTag()
    {
        if (inputTag.text != "")
        {
            selectedMapEntry.tags.Add(inputTag.text);

            StartCoroutine(AddTagServer());

            CleanTagList();

            UpdateTagList();
        }
    }

    public void LoadInfo(string thumbUrl)
    {
        StartCoroutine(LoadMapPreview(thumbUrl));
        CleanTagList();
        UpdateTagList();
    }

    public void UpdateTagList()
    {
        foreach (string tag in selectedMapEntry.tags)
        {
            GameObject tagEntry = Instantiate(tagPrefab.gameObject, listTag.transform);
            TagEntry tagEntryScript = tagEntry.GetComponent<TagEntry>();

            tagEntryScript.mapStore = this;
            tagEntryScript.tagName = tag;
            if (myMaps)
                tagEntryScript.mine = true;
        }
    }

    public void StartDownload(string url)
    {
        if(!isDownloading)
        {
            StartCoroutine(DownloadFile(url));
        }
        else
        {
            Debug.Log("already downloading !");
        }
    }

    IEnumerator LoadMapPreview(string url)
    {
        loadingImageOverlay.gameObject.SetActive(true);
        loadingImageOverlay.PlayAnimation();

        WWW www = new WWW(url);
        yield return www;
        imageMap.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));

        loadingImageOverlay.StopAnimation();
        loadingImageOverlay.gameObject.SetActive(false);
    }

    IEnumerator DownloadFile(string url)
    {
        string path = Application.persistentDataPath + "/levels/" + url.Replace(ConfigHandler.BaseUrl + "/maps/", "");
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.SetRequestHeader("Cookie", authenticationManager.sessionCookie);
            uwr.SetRequestHeader("User-Agent", @"Mozilla / 5.0(Android 4.4; Mobile; rv: 41.0) Gecko / 41.0 Firefox / 41.0");

            AsyncOperation request = uwr.SendWebRequest();

            while (!request.isDone)
            {
                ulong totalSize;
                ulong.TryParse(uwr.GetResponseHeader("Content-Length"), out totalSize);
                progressBar.value = request.progress*100;
                percentageText.text = progressBar.value.ToString("0.##") + " %";
                progressSizeText.text = FormatSize(uwr.downloadedBytes) + " / " + FormatSize(totalSize);
                yield return null;
            }


            if (uwr.isNetworkError || uwr.isHttpError)
            {
                progressBar.value = 0;
                Debug.Log(uwr.error);
                Debug.Log(uwr.responseCode);
            }
            else
            {
                //progressBar.value = 0;
                percentageText.text = "";
                progressSizeText.text = "Done !";
                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
            }
        }
    }

    public void Search(string s)
    {
        if (searchInput.text != "")
        {
            if (lastSearch != searchInput.text)
            {
                lastSearch = searchInput.text;
                CleanList();
                offsetSearching = 0;
            }

            offset = 0;

            myMaps = false;
            searching = true;

            StartCoroutine(SearchMaps());
        }
        else
        {
            searching = false;
            CleanList();
            offsetSearching = 0;
            NextMaps();
        }
    }

    public void Sort()
    {
        offset = 0;
        offsetSearching = 0;
        CleanList();

        if(searchInput.text != "")
        {
            StartCoroutine(SearchMaps());
        }
        else
        {
            StartCoroutine(GetNextMaps());
        }
    }

    IEnumerator SearchMaps()
    {
        loading = true;
        loadingOverlay.gameObject.SetActive(true);
        loadingOverlay.PlayAnimation();
        loadingOverlay.messageText.text = "Loading maps...";

        string terms = searchInput.text;
        string url = "";

        if (!myMaps)
        {
            if (searchTypeDropdown.value == 0)
            {
                url = ConfigHandler.ApiUrl + "/maps/search/";
            }
            else if (searchTypeDropdown.value == 1)
            {
                url = ConfigHandler.ApiUrl + "/tags/";
            }
            else if (searchTypeDropdown.value == 2)
            {
                url = ConfigHandler.ApiUrl + "/authors/";
            }

            url += terms;

            url += "?orderby=" + orderbyTerms[orderbyDropdown.value];
            url += "&sort=" + sortTerms[sortDropdown.value];
        }
        else
        {
            url = ConfigHandler.ApiUrl + "/maps/search/";
            url += "bysid?steamid=" + SteamUser.GetSteamID().m_SteamID.ToString();
        }

        url += "&offset=" + offsetSearching.ToString();

        Debug.Log(url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Cookie", authenticationManager.sessionCookie);

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

            foreach (JSONNode n in node.Children)
            {
                GameObject mapEntry = Instantiate(MapEntryPrefab.gameObject, listMap.transform);
                StoreEntry storeEntry = mapEntry.GetComponent<StoreEntry>();

                storeEntry.mapName = n["name"];
                storeEntry.author = n["author"]["name"];
                storeEntry.downloadUrl = n["path"];
                storeEntry.thumbUrl = ConfigHandler.BaseUrl + "/thumbs/" + n["id"] + ".png";
                storeEntry.mapId = n["id"];
                JSONArray tags = n["tags"].AsArray;

                foreach (JSONNode tag in tags.Values)
                {
                    storeEntry.tags.Add(tag["tag"]);
                }


                storeEntry.mapStore = this;
            }

            lastResultCount = node.Count;

            queryResults.text = listMap.transform.childCount + " results";
        }

        loadingOverlay.StopAnimation();
        loadingOverlay.gameObject.SetActive(false);

        if (lastResultCount >= pageLength)
            scrollRect.verticalNormalizedPosition = 0.5f;

        scrollRect.canInterract = true;
        loading = false;
        offsetSearching += pageLength;
    }

    public void DeleteTag(string tagName)
    {
        selectedMapEntry.tags.Remove(tagName);
        CleanTagList();
        UpdateTagList();
        StartCoroutine(DeleteTagServer(tagName));
    }

    IEnumerator DeleteTagServer(string tagName)
    {
        string url = ConfigHandler.ApiUrl + "/tags/" + tagName + "/" + selectedMapEntry.mapId;

        Debug.Log(url);

        UnityWebRequest www = UnityWebRequest.Delete(url);
        www.SetRequestHeader("Cookie", authenticationManager.sessionCookie);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);
        }
    }

    IEnumerator AddTagServer()
    {
        string url = ConfigHandler.ApiUrl + "/tags/" + inputTag.text + "/" + selectedMapEntry.mapId;

        Debug.Log(url);

        UnityWebRequest www = UnityWebRequest.Post(url, "");
        www.SetRequestHeader("Cookie", authenticationManager.sessionCookie);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);
        }
    }

    private void CleanList()
    {
        foreach (Transform child in listMap.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void CleanTagList()
    {
        foreach (Transform child in listTag.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void NextMaps()
    {
        StartCoroutine(GetNextMaps());
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public string ByteArrayToString(byte[] ba)
    {
        string hex = BitConverter.ToString(ba);
        return hex.Replace("-", "");
    }

    public string FormatSize(ulong size)
    {
        double len = size;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return String.Format("{0:0.##} {1}", len, sizes[order]);
    }
}
