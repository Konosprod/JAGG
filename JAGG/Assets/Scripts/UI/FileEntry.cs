using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FileEntry : MonoBehaviour {

    public Text fileEntryText;
    public EventTrigger eventTrigger;

    [HideInInspector]
    public FileBrowser fileBrowser;

    // Use this for initialization
    void Start() {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        entry.callback.AddListener((data) => { DoubleClicked((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    public void DoubleClicked(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
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
    }

    // Update is called once per frame
    void Update () {
		
	}
}
