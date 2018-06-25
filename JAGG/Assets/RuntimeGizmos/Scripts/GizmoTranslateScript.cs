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
public class GizmoTranslateScript : MonoBehaviour
{

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
    private FreeCamera fc; 

    /// <summary>
    ///     On wake up
    /// </summary>
    public void Awake()
    {
        fc = Camera.main.GetComponent<FreeCamera>();

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
    public void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            foreach (SnappingPoint sp in snappingPoints)
            {
                sp.col.isTrigger = true;
                sp.shouldSnap = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach (SnappingPoint sp in snappingPoints)
            {
                sp.isSnapped = false;
                sp.shouldSnap = false;
                sp.col.isTrigger = false;
            }
        }

        bool noPress = true;

        for (int i = 0; i < 3; i++)
        {
            bool isSnapped = false;

            foreach (SnappingPoint sp in snappingPoints)
            {
                if (sp.isSnapped)
                {
                    isSnapped = true;
                }
            }

            if (Input.GetMouseButton(0) && detectors[i].pressing && !isSnapped)
            {
                fc.bMoveOnEdge = true;
                noPress = false;

                // Get the distance from the camera to the target (used as a scaling factor in translate)
                float distance = Vector3.Distance(Camera.main.transform.position, translateTarget[0].transform.position);
                distance = distance * 2.0f;

                // Will store translate values
                Vector3 offset = Vector3.zero;

                switch (i)
                {
                    // X Axis
                    case 0:
                        {
                            distance = Vector3.Distance(Camera.main.transform.position, translateTarget[0].transform.position);
                            distance = distance * 2.0f;
                            float delta = 0;


                            Vector3 inputVector = (new Vector3(Input.GetAxis("Mouse X"), 0f, -Input.GetAxis("Mouse Y")).normalized);
                            delta = Vector3.Dot(Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * Vector3.right, inputVector) * ((Mathf.Abs(-Input.GetAxis("Mouse Y")) + Mathf.Abs(Input.GetAxis("Mouse X"))) / 2f) * (Time.deltaTime * distance);
                            offset = Vector3.right * delta;
                            offset = new Vector3(offset.x, 0.0f, 0.0f);

                            if (fc.cameraMoveOnEdge == FreeCamera.Move.Left)
                                offset.x = -(Mathf.Max(fc._distance, 4.0f) * 0.001f * fc.movingSpeed);
                            else if (fc.cameraMoveOnEdge == FreeCamera.Move.Right)
                                offset.x = (Mathf.Max(fc._distance, 4.0f) * 0.001f * fc.movingSpeed);

                            foreach (GameObject go in translateTarget)
                            {
                                go.transform.Translate(offset, Space.World);
                            }
                        }
                        break;

                    // Y Axis
                    case 1:
                        {
                            fc.jaiEnvieDeFaireChierAlex = false;

                            float delta = Input.GetAxis("Mouse Y") * (Time.deltaTime * distance);
                            offset = Vector3.up * delta;
                            offset = new Vector3(0.0f, offset.y, 0.0f);

                            if (fc.cameraMoveOnEdge == FreeCamera.Move.Top)
                                offset.y = Mathf.Max(fc._distance, 4.0f) * 0.06f * Time.deltaTime * fc.movingSpeed;
                            else if (fc.cameraMoveOnEdge == FreeCamera.Move.Bottom)
                                offset.y = -Mathf.Max(fc._distance, 4.0f) * 0.06f * Time.deltaTime * fc.movingSpeed;

                            foreach (GameObject go in translateTarget)
                            {
                                go.transform.Translate(offset, Space.World);
                            }
                        }
                        break;

                    // Z Axis
                    case 2:
                        {
                            fc.jaiEnvieDeFaireChierAlex = true;

                            distance = Vector3.Distance(Camera.main.transform.position, translateTarget[0].transform.position);
                            distance = distance * 2.0f;
                            float delta = 0;


                            Vector3 inputVector = (new Vector3(-Input.GetAxis("Mouse X"), 0f, Input.GetAxis("Mouse Y")).normalized);
                            delta = Vector3.Dot(Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * Vector3.back, inputVector) * ((Mathf.Abs(-Input.GetAxis("Mouse Y")) + Mathf.Abs(Input.GetAxis("Mouse X"))) / 2f) * (Time.deltaTime * distance);
                            offset = Vector3.back * delta;
                            offset = new Vector3(0.0f, 0.0f, offset.z);

                            if (fc.cameraMoveOnEdge == FreeCamera.Move.Top)
                                offset.z = Mathf.Max(fc._distance, 4.0f) * 0.06f * Time.deltaTime * fc.movingSpeed;
                            else if (fc.cameraMoveOnEdge == FreeCamera.Move.Bottom)
                                offset.z = -Mathf.Max(fc._distance, 4.0f) * 0.06f * Time.deltaTime * fc.movingSpeed;

                            foreach (GameObject go in translateTarget)
                            {
                                go.transform.Translate(offset, Space.World);
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
                transform.position = translateTarget[0].transform.position;
                end = transform.position;
            }
        }

        if(noPress)
        {
            fc.bMoveOnEdge = false;
        }
    }

}
// End of script.
