using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleOnOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Enter")]
    public float scaleXEnter = 1.0f;
    public float scaleYEnter = 1.0f;
    [Header("Exit")]
    public float scaleXExit = 1.0f;
    public float scaleYExit = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(scaleXEnter, scaleYEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(scaleXExit, scaleYExit);
    }
}
