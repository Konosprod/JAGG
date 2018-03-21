using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InterractableScrollRect : ScrollRect
{
    public bool canInterract = true;

    public override void OnScroll(PointerEventData data)
    {
        if (canInterract)
        {
            base.OnScroll(data);
        }
    }
    public override void OnBeginDrag(PointerEventData eventData)
    {
    }
    public override void OnDrag(PointerEventData eventData)
    {

    }
    public override void OnEndDrag(PointerEventData eventData)
    {
    }
}
