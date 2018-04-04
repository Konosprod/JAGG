using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class LoadingEvent : UnityEvent<string>
{

}

public class FileBrowser : MonoBehaviour {

    [Header("UI")]
    public Text directoryText;
    public GameObject content;
    public Button returnButton;
    public GameObject fileEntryPrefab;
    public LoadingEvent loadingCallback;

    [HideInInspector]
    public string currentDirectory;

    void Awake()
    {
        currentDirectory = Path.Combine(Application.persistentDataPath, "Levels/local") + Path.DirectorySeparatorChar;

        directoryText.text = "Local levels";
    }

	// Use this for initialization
	void Start ()
    {

	}

    void OnEnable()
    {
        BrowseDirectory();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void LoadObject(string path)
    {
        if (loadingCallback != null)
        {
            loadingCallback.Invoke(currentDirectory + path);
            this.gameObject.SetActive(false);

        }
        else
        {
            Debug.Log("You must set a callback for loading function");
        }
    }

    public void ChangeDirectory(string path)
    {
        currentDirectory += path;
        directoryText.text = currentDirectory;
        BrowseDirectory();
    }

    public void ParentDirectory()
    {/*
        currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1);
        DirectoryInfo info = Directory.GetParent(currentDirectory);

        if (info != null)
        {
            currentDirectory = info.FullName + Path.DirectorySeparatorChar;
            directoryText.text = currentDirectory;
            BrowseDirectory();
        }*/
    }

    public void BrowseDirectory()
    {
        //Clear all previous entries if any
        foreach (Transform child in content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Create ".." to move inside
        GameObject parentDirectory = Instantiate(fileEntryPrefab.gameObject, content.transform);
        FileEntry parentFe = parentDirectory.GetComponent<FileEntry>();
        parentFe.fileBrowser = this;
        parentFe.fileEntryText.text = "..";


        //List all directories first
        string[] directories = null;
        try
        {
            directories = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch(UnauthorizedAccessException e)
        {
            Debug.Log(e);
        }

        if (directories != null)
        {
            foreach (string s in directories)
            {
                GameObject fileEntry = Instantiate(fileEntryPrefab.gameObject, content.transform);
                fileEntry.GetComponent<FileEntry>().fileBrowser = this;
                fileEntry.GetComponent<Text>().text = Path.GetFileName(s) + System.IO.Path.DirectorySeparatorChar;
            }
        }


        string[] files = null;

        try
        {
            files = Directory.GetFiles(currentDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch(UnauthorizedAccessException e)
        {
            Debug.Log(e);
        }

        if (files != null)
        {
            foreach (string s in files)
            {
                GameObject fileEntry = Instantiate(fileEntryPrefab.gameObject, content.transform);
                fileEntry.GetComponent<FileEntry>().fileBrowser = this;
                fileEntry.GetComponent<Text>().text = Path.GetFileName(s);
            }
        }
    }
}
