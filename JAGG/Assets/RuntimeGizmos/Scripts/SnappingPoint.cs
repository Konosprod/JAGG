using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingPoint : MonoBehaviour {

    public bool shouldSnap = false;
    public Collider col;
    public bool isSnapped = false;
    public float sphereRadius;
    public float maxDistance;
    public LayerMask layerMask;

    private Vector3 direction;
    private Vector3 origin;
    //private float currentHitDistance;

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (shouldSnap)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                origin = transform.position;
                direction = transform.forward;
                RaycastHit hit;

                if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, layerMask))
                {
                    //currentHitDistance = hit.distance;
                    isSnapped = true;
                    col.isTrigger = false;
                    shouldSnap = false;
                    transform.parent.transform.position += hit.transform.position - col.bounds.center;
                }
                else
                {
                    //currentHitDistance = maxDistance;
                }
            }
        }

    }
    /*
    private void OnDrawGizmosSelected()
    {
        if (shouldSnap)
        {
            Gizmos.color = Color.red;

            Debug.DrawLine(origin, origin + direction * currentHitDistance);
            Gizmos.DrawWireSphere(origin + direction * currentHitDistance, sphereRadius);
        }
    }*/

    /*
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
    }*/
}
