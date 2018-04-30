using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickGo : MonoBehaviour {

    public GizmoRotateScript rotationGizmo;
    public GizmoScaleScript scaleGizmo;
    public GizmoTranslateScript translateGizmo;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "cube")
                {
                    if(scaleGizmo!= null)
                    {
                        scaleGizmo.scaleTarget = hit.transform.gameObject;
                        scaleGizmo.gameObject.SetActive(true);
                    }
                    if (rotationGizmo != null)
                    {
                        rotationGizmo.rotateTarget = hit.transform.gameObject;
                        rotationGizmo.gameObject.SetActive(true);
                    }
                    if(translateGizmo != null)
                    {
                        translateGizmo.translateTarget = hit.transform.gameObject;
                        translateGizmo.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                if(scaleGizmo != null)
                {
                    Debug.Log(scaleGizmo.origin);
                    Debug.Log(scaleGizmo.end);
                    scaleGizmo.gameObject.SetActive(false);
                }
                if (rotationGizmo != null)
                {
                    Debug.Log(rotationGizmo.origin);
                    Debug.Log(rotationGizmo.end);
                    rotationGizmo.gameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                    rotationGizmo.gameObject.SetActive(false);
                }
                if(translateGizmo != null)
                {
                    Debug.Log(translateGizmo.origin);
                    Debug.Log(translateGizmo.end);
                    translateGizmo.gameObject.SetActive(false);
                }
            }
        }
	}
}
