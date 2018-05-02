using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{	
	private Vector3 screenPoint;
    private Vector3 offset;

    public bool isActivated = true;

    public List<Collider> colliders;

    void OnMouseDown()
    {
        if (isActivated)
        {
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
            foreach(Collider c in colliders)
            {
                c.isTrigger = false;
            }
        }
    }

    void OnMouseDrag()
    {
        if (isActivated)
        {
            Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
            transform.position = cursorPosition;
        }
    }

    void OnMouseUp()
    {
        foreach (Collider c in colliders)
        {
            c.isTrigger = true;
            c.GetComponent<SnappingPoint>().isSnapped = false;
        }
        isActivated = true;
    }
}
