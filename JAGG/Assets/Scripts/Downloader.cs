using System;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class Downloader : MonoBehaviour {

    private WebClient client = new WebClient();

    public int progressPercent;
    public bool isDone = false;
    public long totalBytes;
    public long currentBytes;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void DownloadFile(string url, string path)
    {
        client.DownloadProgressChanged += ProgressChanged;
        client.DownloadFileCompleted += DownloadFinished;

        try
        {
            client.DownloadFileAsync(new Uri(url), path);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private void DownloadFinished(object sender, AsyncCompletedEventArgs e)
    {
        isDone = true;
    }

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        totalBytes = e.TotalBytesToReceive;
        currentBytes = e.BytesReceived;
        progressPercent = e.ProgressPercentage;
    }
}
