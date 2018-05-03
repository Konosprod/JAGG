using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///     Simple script to handle the functionality of the Translate Gizmo (i.e. move the gizmo
///     and the target object along the axis the user is dragging towards)
/// </summary>
/// 
/// <author>
///     Michael Hillman - thisishillman.co.uk
/// </author>
/// 
/// <version>
///     1.0.0 - 01st January 2016
/// </version>
public class GizmoTranslateScript : MonoBehaviour {

    /// <summary>
    ///     X axis of gizmo
    /// </summary>
    public GameObject xAxisObject;

    /// <summary>
    ///     Y axis of gizmo
    /// </summary>
    public GameObject yAxisObject;

    /// <summary>
    ///     Z axis of gizmo
    /// </summary>
    public GameObject zAxisObject;

    /// <summary>
    ///     Target for translation
    /// </summary>
    public List<GameObject> translateTarget;

    /// <summary>
    ///     Array of detector scripts stored as [x, y, z]
    /// </summary>
    private GizmoClickDetection[] detectors;

    public Vector3 origin;
    public Vector3 end;
    private SnappingPoint[] snappingPoints;

    /// <summary>
    ///     On wake up
    /// </summary>
    public void Awake() {

        // Get the click detection scripts
        detectors = new GizmoClickDetection[3];
        detectors[0] = xAxisObject.GetComponent<GizmoClickDetection>();
        detectors[1] = yAxisObject.GetComponent<GizmoClickDetection>();
        detectors[2] = zAxisObject.GetComponent<GizmoClickDetection>();

        // Set the same position for the target and the gizmo
        transform.position = translateTarget[0].transform.position;
    }

    private void OnEnable()
    {
        if (translateTarget.Count <= 1)
        {
            snappingPoints = translateTarget[0].GetComponentsInChildren<SnappingPoint>();
        }
        else
        {
            snappingPoints = new SnappingPoint[0];
        }
        transform.position = translateTarget[0].transform.position;
        origin = transform.position;
    }

    /// <summary>
    ///     Once per frame
    /// </summary>
    public void Update() {

        if(Input.GetMouseButtonDown(0))
        {
            foreach(SnappingPoint sp in snappingPoints)
            {
                sp.col.isTrigger = false;
                sp.shouldSnap = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach (SnappingPoint sp in snappingPoints)
            {
                sp.isSnapped = false;
                sp.col.isTrigger = true;
                sp.shouldSnap = false;
            }
        }

        for (int i = 0; i < 3; i++) {
            bool isSnapped = false;

            foreach (SnappingPoint sp in snappingPoints)
            {
                if (sp.isSnapped)
                {
                    isSnapped = true;
                }
            }

            if (Input.GetMouseButton(0) && detectors[i].pressing && !isSnapped) {

                // Get the distance from the camera to the target (used as a scaling factor in translate)
                float distance = Vector3.Distance(Camera.main.transform.position, translateTarget[0].transform.position);
                distance = distance * 2.0f;

                // Will store translate values
                Vector3 offset = Vector3.zero;

                switch (i) {
                    // X Axis
                    case 0:
                        {
                            // If the user is pressing the plane, move along Y and Z, else move along X

                            if (detectors[i].pressingPlane) {
                                float deltaY = Input.GetAxis("Mouse Y") * (Time.deltaTime * distance);
                                offset = Vector3.up * deltaY;
                                offset = new Vector3(0.0f, offset.y, 0.0f);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }

                                float deltaZ = Input.GetAxis("Mouse X") * (Time.deltaTime * distance);
                                offset = Vector3.forward * deltaZ;
                                offset = new Vector3(0.0f, 0.0f, offset.z);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }
                                //translateTarget.transform.Translate(offset);

                            } else {
                                distance = Vector3.Distance(Camera.main.transform.position, translateTarget[0].transform.position);
                                distance = distance * 2.0f;

                                float delta = Input.GetAxis("Mouse X") * (Time.deltaTime * distance);
                                offset = Vector3.right * delta;
                                offset = new Vector3(offset.x, 0.0f, 0.0f);

                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset, Space.World);
                                }
                                //translateTarget.transform.Translate(offset, Space.Self);
                            }
                        }
                        break;

                    // Y Axis
                    case 1:
                        {
                            // If the user is pressing the plane, move along X and Z, else just move along X

                            if (detectors[i].pressingPlane) {
                                float deltaX = Input.GetAxis("Mouse X") * (Time.deltaTime * distance);
                                offset = Vector3.left * deltaX;
                                offset = new Vector3(offset.x, 0.0f, 0.0f);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }
                                //translateTarget.transform.Translate(offset);

                                float deltaZ = Input.GetAxis("Mouse Y") * (Time.deltaTime * distance);
                                offset = Vector3.forward * deltaZ;
                                offset = new Vector3(0.0f, 0.0f, -offset.z);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }
                                //translateTarget.transform.Translate(offset);

                            } else {
                                
                                float delta = Input.GetAxis("Mouse Y") * (Time.deltaTime * distance);
                                offset = Vector3.up * delta;
                                offset = new Vector3(0.0f, offset.y, 0.0f);
                                
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset, Space.World);
                                }
                                //translateTarget.transform.Translate(offset, Space.Self);
                            }
                        }
                        break;

                    // Z Axis
                    case 2:
                        {
                            // If the user is pressing the plane, move along X and Y, else just move along Z

                            if (detectors[i].pressingPlane) {
                                float deltaX = Input.GetAxis("Mouse X") * (Time.deltaTime * distance);
                                offset = Vector3.left * deltaX;
                                offset = new Vector3(offset.x, 0.0f, 0.0f);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }
                                //translateTarget.transform.Translate(offset);

                                float deltaY = Input.GetAxis("Mouse Y") * (Time.deltaTime * distance);
                                offset = Vector3.up * deltaY;
                                offset = new Vector3(0.0f, offset.y, 0.0f);
                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset);
                                }
                                //translateTarget.transform.Translate(offset);

                            } else {
                                
                                float delta = Input.GetAxis("Mouse X") * (Time.deltaTime * distance);
                                offset = Vector3.back * delta;
                                offset = new Vector3(0.0f, 0.0f, offset.z);

                                foreach (GameObject go in translateTarget)
                                {
                                    go.transform.Translate(offset, Space.World);
                                }
                                //translateTarget.transform.Translate(offset, Space.Self);
                            }
                        }
                        break;
                }

                // Move the gizmo to match the target position
                transform.position = translateTarget[0].transform.position;

                break;
            }
            else
            {
                end = transform.position;
            }
        }
    }

}
// End of script.
