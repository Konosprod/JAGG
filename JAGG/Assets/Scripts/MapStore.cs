using System;
using UnityEngine;
using UnityEngine.UI;

public class MapStore : MonoBehaviour
{
    public Downloader downloader;
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

    void FixedUpdate()
    {
        if(isDownloading)
        {
            progressBar.value = downloader.progressPercent;
            percentageText.text = progressBar.value.ToString() + " %";
            progressSizeText.text = FormatSize(downloader.currentBytes) + " / " + FormatSize(downloader.totalBytes);

            if(downloader.isDone)
            {
                isDownloading = false;
                progressBar.value = 0;

                //What to do when done
            }
        }
    }

    public void StartDownload()
    {
        isDownloading = true;
        //On pourra pas use du HTTPS avec Let's Encrypt, mono ne reconnait pas l'authoritée de certif
        //downloader.DownloadFile("http://compile.konosprod.fr/1.mp4", @"C:\Users\Kono\Desktop\dled.mp4");
    }

    public string FormatSize(long size)
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
