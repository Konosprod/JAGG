using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FileEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    public Text fileEntryText;
    public EventTrigger eventTrigger;

    [HideInInspector]
    public FileBrowser fileBrowser;
    public FileSaver fileSaver;

    // Use this for initialization
    void Start() {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            //Browsing context
            if (fileBrowser != null)
            {
                if (fileEntryText.text == "..")
                {
                    //Parent
                    fileBrowser.ParentDirectory();
                }
                else if (fileEntryText.text.EndsWith("" + System.IO.Path.DirectorySeparatorChar))
                {
                    fileBrowser.ChangeDirectory(fileEntryText.text);
                }
                else
                {
                    //Load an object
                    fileBrowser.LoadObject(fileEntryText.text);
                }
            }
            //Saving context
            else if (fileSaver != null)
            {
                if (fileEntryText.text == "..")
                {
                    //Parent
                    fileSaver.ParentDirectory();
                }
                else if (fileEntryText.text.EndsWith("" + System.IO.Path.DirectorySeparatorChar))
                {
                    fileSaver.ChangeDirectory(fileEntryText.text);
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnPointerExit(PointerEventData eventData)
    {
        fileEntryText.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        fileEntryText.color = Color.red;
    }
}
