using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapStore : MonoBehaviour
{
    public Slider progressBar;
    public Text percentageText;
    public Text progressSizeText;

    [HideInInspector]
    public bool isDownloading = false;

    private static string[] sizes = { "B", "KB", "MB", "GB", "TB" };

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    public void StartDownload()
    {
        //downloader.DownloadFile("https://img.20mn.fr/ef4UGBD6S5aECsvZcFzisQ/478x190-0.21x13.52-100_deux-loups-semi-sauvages-parc-angles-sud-france-juin-2015", @"C:\Users\Kono\Desktop\img.jpg");

        StartCoroutine(DownloadFile("https://img.20mn.fr/ef4UGBD6S5aECsvZcFzisQ/478x190-0.21x13.52-100_deux-loups-semi-sauvages-parc-angles-sud-france-juin-2015", @"C:\Users\Kono\Desktop\img.jpg"));
    }

    IEnumerator DownloadFile(string url, string path)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
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
                progressBar.value = 0;
                percentageText.text = "";
                progressSizeText.text = "";
                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
            }
        }
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

        // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
        // show a single decimal place, and no space.
        return String.Format("{0:0.##} {1}", len, sizes[order]);
    }
}
