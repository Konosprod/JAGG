using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FileSaver : MonoBehaviour {


    [Header("UI")]
    public Text directoryText;
    public GameObject content;
    public Button returnButton;
    public Button saveButton;
    public InputField inputFilename;
    public OverwriteMessageBox messageBox;
    public GameObject fileEntryPrefab;

    [HideInInspector]
    public string currentDirectory;

    void Awake()
    {
        currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar;

        directoryText.text = currentDirectory;
    }

    // Use this for initialization
    void Start()
    {
        saveButton.onClick.AddListener(delegate ()
        {
            if(inputFilename.text != "")
            {
                SaveObject(inputFilename.text);
            }
            else
            {
                Debug.Log("Empty filename");
            }
        });
    }

    void OnEnable()
    {
        BrowseDirectory();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void EnableButtons()
    {
        returnButton.interactable = true;
        saveButton.interactable = true;
    }

    public void DisableButtons()
    {
        returnButton.interactable = false;
        saveButton.interactable = false;
    }

    public void SaveObject(string path, bool overwrite = false)
    {

        if(File.Exists(currentDirectory + path) && !overwrite)
        {
            messageBox.path = currentDirectory + path;
            DisableButtons();
            messageBox.gameObject.SetActive(true);
            return;
        }

        throw new NotImplementedException();
    }

    public void ChangeDirectory(string path)
    {
        currentDirectory += path;
        directoryText.text = currentDirectory;
        BrowseDirectory();
    }

    public void ParentDirectory()
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
        parentFe.fileSaver = this;
        parentFe.fileEntryText.text = "..";


        //List all directories first
        string[] directories = null;
        try
        {
            directories = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch (UnauthorizedAccessException e)
        {
            Debug.Log(e);
        }

        if (directories != null)
        {
            foreach (string s in directories)
            {
                GameObject fileEntry = Instantiate(fileEntryPrefab.gameObject, content.transform);
                fileEntry.GetComponent<FileEntry>().fileSaver = this;
                fileEntry.GetComponent<Text>().text = Path.GetFileName(s) + System.IO.Path.DirectorySeparatorChar;
            }
        }

    }
}
