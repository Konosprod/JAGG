using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // The amount of time the piece will wait before cycling
    [CustomProp]
    public float timerOffset = 0f;

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

    [HideInInspector]
    public bool flagStopRotation = false;
    [HideInInspector]
    public bool isRotation = false;
    [HideInInspector]
    public float timer;
    [HideInInspector]
    public float rotationAngle;

    private Transform targetRot;

    [HideInInspector]
    public Coroutine coroutine;

    private List<ChildColliderRotatePiece> ccrtps;
    
    // List of the balls that are currently on top on the piece
    [HideInInspector]
    public List<GameObject> ballsOnTop;

    public Quaternion goalAngle = Quaternion.identity;

    // Use this for initialization
    void Start()
    {
        if (initX != -1f && initY != -1f && initZ != -1f)
        {
            transform.eulerAngles = new Vector3(initX, initY, initZ);
        }
        
        UpdateInitialRotation();
        timer = -timerOffset;
        rotationAngle = 360 / nbRotations;

        ballsOnTop = new List<GameObject>();

        ccrtps = new List<ChildColliderRotatePiece>();
        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach(Collider col in cols)
        {
            ChildColliderRotatePiece ccrtp = col.gameObject.AddComponent<ChildColliderRotatePiece>();
            ccrtp.SetRtpParent(this);
            ccrtps.Add(ccrtp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if (!flagStopRotation)
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
        }*/
    }


    public IEnumerator RotateMe(Vector3 byAngles, float inTime)
    {
        Quaternion fromAngle = transform.rotation;
        goalAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (float t = 0f; t < 1f; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, goalAngle, t);
            yield return null;
        }

        transform.rotation = goalAngle;
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
        if (coroutine != null)
        {
            if (SceneManager.GetSceneAt(0).name == "LevelEditor")
                LevelEditorRotatePieceManager._instance.StopMyCoroutine(this);
            else
                RotatePieceManager._instance.StopMyCoroutine(this);
        }


        transform.eulerAngles = initialRotation;
    }


    public void Reset()
    {
        if (coroutine != null)
        {
            if (SceneManager.GetSceneAt(0).name == "LevelEditor")
                LevelEditorRotatePieceManager._instance.StopMyCoroutine(this);
            else
                RotatePieceManager._instance.StopMyCoroutine(this);
        }

        transform.eulerAngles = initialRotation;
        timer = -timerOffset;
        isRotation = false;
        rotationAngle = 360 / nbRotations;
        ballsOnTop.Clear();
    }

}
