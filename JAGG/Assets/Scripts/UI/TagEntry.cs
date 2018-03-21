using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Image backgroundImage;
    public Text tagEntry;

    [HideInInspector]
    public string tagName;
    [HideInInspector]
    public MapStore mapStore;

    public EventTrigger eventTrigger;

    public Color highlighted;
    public Color nonHighlighted;

    // Use this for initialization
    void Start()
    {
        tagEntry.text = tagName;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            mapStore.searchTypeDropdown.value = 1;
            mapStore.searchInput.text = tagName;
            mapStore.Search("");
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
