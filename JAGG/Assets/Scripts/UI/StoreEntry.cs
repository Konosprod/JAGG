using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Image backgroundImage;
    public Text mapNameEntry;
    public Text authorEntry;

    [HideInInspector]
    public string mapName;
    [HideInInspector]
    public string downloadUrl;
    [HideInInspector]
    public string author;
    [HideInInspector]
    public List<string> tags = new List<string>();
    [HideInInspector]
    public string thumbUrl;
    [HideInInspector]
    public MapStore mapStore;

    public EventTrigger eventTrigger;

    public Color highlighted;
    public Color nonHighlighted;

    // Use this for initialization
    void Start()
    {
        mapNameEntry.text = mapName;
        authorEntry.text = author;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.clickCount == 1)
        {
            mapStore.LoadInfo(tags, thumbUrl);
        }
        if (eventData.clickCount == 2)
        {
            mapStore.StartDownload(downloadUrl);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.color = nonHighlighted;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.color = highlighted;
    }
}