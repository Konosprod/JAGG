using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


// Allows the piece to move from one position to another
public class MovingPiece : CustomScript {

    // Amount of seconds it takes to travel from one place to the other
    [CustomProp]
    public float travelTime = 2.0f;

    // Amount of seconds it pauses between each movement
    [CustomProp]
    public float pauseTime = 2.0f;

    // Target position
    public Vector3 destPos = -Vector3.one;
    [CustomProp]
    public float destX = -1f;
    [CustomProp]
    public float destY = -1f;
    [CustomProp]
    public float destZ = -1f;


    // Initial position
    public Vector3 initPos = Vector3.zero;
    [CustomProp]
    public float initX = -1f;
    [CustomProp]
    public float initY = -1f;
    [CustomProp]
    public float initZ = -1f;


    [HideInInspector]
    public bool flagStopMove = false;
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool forwardMove = false; // Forward false means going from initPos to destPos and vice versa (to be exact forwardMove will be true while the piece is moving forward, but has to be false at first because the manager reverses the bool before the movement starts)
    [HideInInspector]
    public float timer;


    [HideInInspector]
    public Coroutine coroutine;

    public int counterCoroutine = 0;
    public int counterManager = 0;


    // Script that goes on every collider and reports when balls collide with the piece
    private List<ChildColliderMovingPiece> ccmvps;

    // List of the balls that are currently on top on the piece
    public List<GameObject> ballsOnTop;

    void Awake()
    {
        ballsOnTop = new List<GameObject>();
        ccmvps = new List<ChildColliderMovingPiece>();
    }
    // Use this for initialization
    void Start ()
    {
        if (initX != -1f && initY != -1f && initZ != -1f)
        {
            transform.position = new Vector3(initX, initY, initZ);
        }

        UpdateInitialPosition();

        if (destX != -1f && destY != -1f && destZ != -1f)
        {
            //Debug.Log("There : destX=" + destX + ", destY=" + destY + ", destZ=" + destZ);
            UpdateDestination(new Vector3(destX, destY, destZ));
        }

        //initPos = transform.position;

        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (Collider col in cols)
        {
            ChildColliderMovingPiece ccmvp = col.gameObject.AddComponent<ChildColliderMovingPiece>();
            ccmvp.SetMvpParent(this);
            ccmvps.Add(ccmvp);
        }

    }

    void OnDisable()
    {
        foreach(ChildColliderMovingPiece ccmvp in ccmvps)
        {
            ccmvp.enabled = false;
        }
    }

    void OnEnable()
    {
        foreach (ChildColliderMovingPiece ccmvp in ccmvps)
        {
            ccmvp.enabled = true;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    // Coroutine that moves the object
    public IEnumerator MoveMe(Vector3 startPos, Vector3 endPos, float time)
    {
        var i = 0.0f;
        var rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(startPos, endPos, i);
            yield return null;
        }
    }

    // Allows to set the destination in the levelEditor
    public void UpdateDestination(Vector3 newDest)
    {
        destPos = newDest;
        destX = destPos.x;
        destY = destPos.y;
        destZ = destPos.z;
    }

    // Use this after moving the piece in the levelEditor
    public void UpdateInitialPosition()
    {
        initPos = transform.position;
        initX = initPos.x;
        initY = initPos.y;
        initZ = initPos.z;
    }

    public void SetFlagStopMove(bool f)
    {
        flagStopMove = f;
        Reset();
    }

    public void Reset()
    {
        if (coroutine != null)
        {
            if (SceneManager.GetSceneAt(0).name == "LevelEditor")
                LevelEditorMovingPieceManager._instance.StopMyCoroutine(this);
            else
                MovingPieceManager._instance.StopMyCoroutine(this);
        }

        transform.position = initPos;
        timer = 0f;
        isMoving = false;
        forwardMove = false;
        ballsOnTop.Clear();
    }

    void OnDestroy()
    {
        if (coroutine != null)
        {
            if (SceneManager.GetSceneAt(0).name == "LevelEditor")
                LevelEditorMovingPieceManager._instance.StopMyCoroutine(this);
            else
                MovingPieceManager._instance.StopMyCoroutine(this);
        }
    }
}
