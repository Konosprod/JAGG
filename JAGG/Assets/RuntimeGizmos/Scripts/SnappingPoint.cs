using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingPoint : MonoBehaviour {

    public Collider col;
    public bool isSnapped = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("SnapPoint") && !isSnapped && !other.GetComponent<SnappingPoint>().isSnapped)
            {
                //Find a mean to disable dragging
                //other.transform.parent.GetComponent<MouseDrag>().isActivated = false;
                Vector3 otherPos = other.transform.position;
                Vector3 distance = col.bounds.center - other.transform.position;
                other.transform.parent.transform.position += distance;
                other.GetComponent<SnappingPoint>().isSnapped = true;
            }
        }
    }
}
