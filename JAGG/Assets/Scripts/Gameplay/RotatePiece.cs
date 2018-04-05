using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Allows a piece to rotate on itself
// Turns in increments defined by the user
public class RotatePiece : CustomScript
{

    public const float defaultSpinTime = 1.0f;
    public const float defaultPauseTime = 0.2f;
    public const int defaultNbRotations = 4;

    // Amount of seconds it takes to perform a single rotation 
    [CustomProp]
    public float spinTime = 1.0f;
    // Amount of seconds a piece will stay in place
    [CustomProp]
    public float pauseTime = 0.2f;

    // The amount of rotations required to perform a full turn (4 means 90 degrees each time)
    [CustomProp]
    public int nbRotations = 4;


    [CustomProp]
    public float initX = -1f;
    [CustomProp]
    public float initY = -1f;
    [CustomProp]
    public float initZ = -1f;

    public Vector3 initialRotation = Vector3.one;

    private bool flagStopRotation = false;

    private bool isRotation = true;
    private float timer;
    private float rotationAngle;

    private Transform targetRot;

    private IEnumerator coroutine;
    

    // Use this for initialization
    void Start()
    {
        if (initX != -1f && initY != -1f && initZ != -1f)
        {
            transform.eulerAngles = new Vector3(initX, initY, initZ);
        }
        
        UpdateInitialRotation();
        timer = 0f;
        rotationAngle = 360 / nbRotations;
    }

    // Update is called once per frame
    void Update()
    {
        if (!flagStopRotation)
        {
            timer += Time.deltaTime;

            if ((isRotation && timer > spinTime) || (!isRotation && timer > pauseTime))
            {
                isRotation = !isRotation;
                timer = 0f;
                if (isRotation)
                {
                    coroutine = RotateMe(Vector3.up * rotationAngle, spinTime);
                    StartCoroutine(coroutine);
                }
            }
        }
    }


    IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        Quaternion fromAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (float t = 0f; t < 1f; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }

        transform.rotation = toAngle;
    }

    public void UpdateRotations()
    {
        transform.rotation = Quaternion.identity;
        rotationAngle = 360 / nbRotations;
    }

    public void UpdateInitialRotation()
    {
        initialRotation = transform.eulerAngles;
        initX = initialRotation.x;
        initY = initialRotation.y;
        initZ = initialRotation.z;
    }

    // true stops the piece from spinning
    // we put the piece back to its original rotation either way
    public void SetStopSpinFlag(bool f)
    {
        flagStopRotation = f;
        if(coroutine != null)
            StopCoroutine(coroutine);
        transform.eulerAngles = initialRotation;
    }
}
