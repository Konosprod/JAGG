using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingPoint : MonoBehaviour
{

    public bool shouldSnap = false;
    public Collider col;
    public bool isSnapped = false;
    public float sphereRadius;
    public float maxDistance;
    public LayerMask layerMask;
    public MeshRenderer sphere;

    public Color selectedColor;
    public Color unselectedColor;

    private Vector3 direction;
    private Vector3 origin;
    //private float currentHitDistance;

    // Use this for initialization
    void Start()
    {
        shouldSnap = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            sphere.enabled = true;
            if (shouldSnap)
            {
                //Debug.Log("We snap");

                origin = transform.position;
                /*direction = transform.forward;
                RaycastHit hit;

                //Debug.DrawRay(origin, direction, Color.red, 5f);

                if (Physics.SphereCast(origin, sphereRadius, direction, out hit, maxDistance, layerMask))
                {*/


                // Overlap Shpere will also find the collider of this SnapPoint so we must ignore that
                Collider[] otherSnapPoints = Physics.OverlapSphere(origin, sphereRadius, layerMask);

                if (otherSnapPoints.Length > 1)
                {
                    Collider otherSnapCol = otherSnapPoints[0];
                    int i = 1;
                    while (i < otherSnapPoints.Length && otherSnapCol == col)
                    {
                        otherSnapCol = otherSnapPoints[i];
                        i++;
                    }
                    //Debug.Log(transform.parent.name);
                    //currentHitDistance = hit.distance;
                    isSnapped = true;
                    col.isTrigger = false;
                    shouldSnap = false;
                    transform.parent.transform.position += otherSnapCol.transform.position - col.bounds.center;
                }
                else
                {
                    //currentHitDistance = maxDistance;
                }
            }
        }
        else
        {
            sphere.enabled = false;
        }

    }

    public void Selected(bool isSelected = true)
    {
        sphere.material.SetColor("_Color", isSelected ? selectedColor : unselectedColor);
        sphere.material.SetColor("_SpecColor", isSelected ? selectedColor : unselectedColor);
    }

    /*
    private void OnDrawGizmosSelected()
    {
        //if (shouldSnap)
        //{
            Gizmos.color = Color.red;

            Debug.DrawLine(origin, origin + direction * currentHitDistance);
            Gizmos.DrawWireSphere(origin + direction * currentHitDistance, sphereRadius);
        //}
    }

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
