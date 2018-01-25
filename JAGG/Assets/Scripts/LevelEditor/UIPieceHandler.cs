using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPieceHandler : MonoBehaviour
    , IPointerClickHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{

    public EditorManager editorMan;

    private Image sprite;
    private Color target;
    private Color defaultColor;

    void Awake()
    {
        sprite = GetComponent<Image>();
        target = sprite.color;
        defaultColor = sprite.color;
    }

    void Update()
    {
        if (sprite)
            sprite.color = target;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log(Mouse click");
        target = Color.blue;
        editorMan.clickOnPiece(gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Mouse over");
        target = Color.green;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Mouse exit");
        target = defaultColor;
    }
}