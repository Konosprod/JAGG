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
    public LoadingEvent mapLoadingCallback;
    public LoadingEvent objLoadingCallback;

    [HideInInspector]
    public string currentDirectory;

    private bool mapContext = true;

    void Awake()
    {

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
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            this.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void BrowseMap()
    {
        currentDirectory = Path.Combine(Application.persistentDataPath, "Levels/local") + Path.DirectorySeparatorChar;
        directoryText.text = "Local levels";

        mapContext = true;

        this.gameObject.SetActive(true);
    }

    public void BrowseObjects()
    {
        currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar;
        directoryText.text = "My Documents";

        mapContext = false;

        this.gameObject.SetActive(true);
    }

    public void LoadObject(string path)
    {
        if (mapContext)
        {
            if (mapLoadingCallback != null)
            {
                mapLoadingCallback.Invoke(currentDirectory + path);
                this.gameObject.SetActive(false);

            }
            else
            {
                Debug.Log("You must set a callback for map loading function");
            }
        }
        else
        {
            if(objLoadingCallback != null)
            {
                objLoadingCallback.Invoke(currentDirectory + path);
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("You must set a callback for obj loading function");
            }
        }
    }

    public void ChangeDirectory(string path)
    {
        currentDirectory += path;
        directoryText.text = currentDirectory;
        BrowseDirectory();
    }

    public void ParentDirectory()
    {
        if (!mapContext)
        {
            currentDirectory = currentDirectory.Remove(currentDirectory.Length - 1);
            DirectoryInfo info = Directory.GetParent(currentDirectory);

            if (info != null)
            {
                currentDirectory = info.FullName + Path.DirectorySeparatorChar;
                directoryText.text = currentDirectory;
                BrowseDirectory();
            }
        }
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

        //Reset scrollview position
        Vector3 pos = content.transform.position;
        pos.y = 0;
        content.transform.position = pos;
    }
}
