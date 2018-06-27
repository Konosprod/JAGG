using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeCamera : MonoBehaviour
{

    [SerializeField] private Vector3 _target = Vector3.zero;
    [SerializeField]
    private float
        _xSpeed = 250f,
        _ySpeed = 120f,
        _xSlideSpeed = 250f,
        _ySlideSpeed = 250f,
        _zoomSpeed = 90f,
        _yMinLimit = -20f,
        _yMaxLimit = 80f;

    private float _x, _y;
    //private Vector3 _startPos;

    public EditorManager editorManager = null;

    [Header("Camera Moving")]
    public float _distance = 10f;
    public int sizeEdge = 15;
    public float movingSpeed = 30;
    public bool bMoveOnEdge = false; // Is the camera allowed to move
    public bool cameraMoveOnEdge = false; // Is the camera moving
    public enum Move { NoMove, Left, Right, Top, Bottom, Forward, Backward };
    public Move gizmoDirection = Move.NoMove; // Direction and axis of the gizmo (X axis with positive movement is Right, etc)

    private float ClampAngle(float angle, float min = 0f, float max = 360f)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void Start()
    {
        //_startPos = _target;
        var eulerAngles = transform.eulerAngles;
        _x = eulerAngles.y; //TODO fix maybe
        _y = eulerAngles.x;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    private void LateUpdate()
    {
        float axis = Input.GetAxis("Mouse X");
        float axis2 = Input.GetAxis("Mouse Y");
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        if (/*Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1)*/ wheel != 0f && !EventSystem.current.IsPointerOverGameObject())
        {
            // Could use alt + right-click to zoom like in unity editor (can do both if we want to)
            /*float num = (Mathf.Abs(axis) <= Mathf.Abs(axis2)) ? axis2 : axis;
            num = -num * _zoomSpeed * 0.03f;
            _distance += num * (Mathf.Max(_distance, 0.5f) * 0.03f);*/

            // Scale zoomSpeed based on distance to target
            _distance -= (wheel * (Mathf.Max(_distance, _zoomSpeed)));
            // Stops from zooming to close
            if (_distance <= -2f)
                _distance = -2f;
            else if (_distance >= 70f)
                _distance = 70f;
        }
        else if (Input.GetMouseButton(1))
        {
            _x += axis * _xSpeed * 0.03f;
            _y += axis2 * _ySpeed * 0.03f;
            _y = ClampAngle(_y, _yMinLimit, _yMaxLimit);
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 a = transform.rotation * Vector3.right;
            Vector3 a2 = transform.rotation * Vector3.up;
            Vector3 a3 = -a * axis * _xSlideSpeed * 0.02f;
            Vector3 b = -a2 * axis2 * _ySlideSpeed * 0.02f;
            _target += (a3 + b) * (Mathf.Max(_distance, 4.0f) * 0.01f);
            _target.y = 3.0f;
        }
        /*else if (Input.GetKey(KeyCode.F))
        {
            _target = _startPos;
        }*/

        cameraMoveOnEdge = false;
        if (bMoveOnEdge)
        {
            // Move camera when close to edges
            if (Input.mousePosition.x > (Screen.width - sizeEdge) || Input.mousePosition.x < sizeEdge || Input.mousePosition.y > (Screen.height - sizeEdge) || Input.mousePosition.y < sizeEdge)
            {
                cameraMoveOnEdge = true;

                if(gizmoDirection == Move.Right)
                    _target += new Vector3(Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed, 0, 0);
                else if(gizmoDirection == Move.Left)
                    _target -= new Vector3(Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed, 0, 0);
                else if (gizmoDirection == Move.Forward)
                    _target += new Vector3(0, 0, Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed);
                else if (gizmoDirection == Move.Backward)
                    _target -= new Vector3(0, 0, Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed);
                else if (gizmoDirection == Move.Top)
                    _target += new Vector3(0, Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed, 0);
                else if (gizmoDirection == Move.Bottom)
                    _target -= new Vector3(0, Mathf.Max(_distance, 4.0f) * 0.06f * Time.deltaTime * movingSpeed, 0);
            }
        }

        Quaternion rotation = Quaternion.Euler(_y, _x, 0f);
        Vector3 position = rotation * new Vector3(0f, 0f, -_distance) + _target;
        transform.rotation = rotation;
        transform.position = position;
    }

    public void SetTarget(Vector3 target)
    {
        _target = target;
    }
}
