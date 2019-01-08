using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TagEntry : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Image backgroundImage;
    public Text tagEntry;
    public Button buttonDelete;

    [HideInInspector]
    public string tagName;
    [HideInInspector]
    public MapStore mapStore;

    public EventTrigger eventTrigger;

    public Color highlighted;
    public Color nonHighlighted;

    //Is the tag set by the author
    public bool mine = false;

    // Use this for initialization
    void Start()
    {
        tagEntry.text = tagName;

        if (mine)
            buttonDelete.gameObject.SetActive(true);

        buttonDelete.onClick.RemoveAllListeners();
        buttonDelete.onClick.AddListener(DeleteTag);
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

    public void DeleteTag()
    {
        mapStore.DeleteTag(tagName);
    }
}
