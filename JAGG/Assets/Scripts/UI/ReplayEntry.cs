using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReplayEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image backgroundImage;
    public Text mapNameEntry;
    public Text replayNameEntry;
    public Text dateEntry;

    [HideInInspector]
    public string mapName;
    [HideInInspector]
    public string replayName;
    [HideInInspector]
    public string date;
    /*[HideInInspector]
    public List<string> tags = new List<string>();*/
    [HideInInspector]
    public int mapId;

    public EventTrigger eventTrigger;

    public Color highlighted;
    public Color nonHighlighted;

    // Use this for initialization
    void Start()
    {
        mapNameEntry.text = mapName.Split('_')[1];
        replayNameEntry.text = replayName;
        dateEntry.text = date;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            /*mapStore.selectedMapEntry = this;
            mapStore.LoadInfo(thumbUrl);*/
        }
        if (eventData.clickCount == 2)
        {
            /*mapStore.selectedMapEntry = this;
            mapStore.StartDownload(downloadUrl);*/
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