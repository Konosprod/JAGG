using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTestDebug : MonoBehaviour
{

    private Rigidbody rb;
    private BallPhysicsTest physics;
    public ParticleSystem trail;


    [Header("Simulation properties")]
    public Vector3 shootDirection;
    public float forceOfShot;
    public float timerRestart; // In seconds


    [Header("Displays")]
    public float currentTimer;
    public float velocityMagnitude;
    public Vector3 velocity;
    public bool logToConsole = false;


    private Vector3 startPos;
    private bool flagEnableTrail = false;


    private int layerFloor/*,
                layerWall*/;


    [Header("Log")]
    public bool logToFile = false;
    public int nbRuns = 100;
    public int currentRun;
    private List<Vector3> endPositions = new List<Vector3>();
    private Dictionary<Vector3, int> endPositionsCount = new Dictionary<Vector3, int>();
    private Vector3 totalEndPosition = Vector3.zero;

    private int framesFromStart = 0;

    public List<RotatePiece> rtps = new List<RotatePiece>();
    public List<MovingPiece> mvps = new List<MovingPiece>();

    public void StopMyCoroutine(RotatePiece rtp)
    {
        if (rtp.coroutine != null)
            StopCoroutine(rtp.coroutine);
    }
    public void StopMyCoroutine(MovingPiece mvp)
    {
        if (mvp.coroutine != null)
            StopCoroutine(mvp.coroutine);
    }
    public void ResetAllRTPs()
    {
        foreach (RotatePiece rtp in rtps)
        {
            if (rtp.enabled)
            {
                rtp.Reset();
            }
        }
    }
    public void ResetAllMVPs()
    {
        foreach (MovingPiece mvp in mvps)
        {
            if (mvp.enabled)
            {
                mvp.Reset();
            }
        }
    }


    void Start()
    {
        currentRun = 0;
        startPos = transform.position;
        currentTimer = timerRestart;
        rb = GetComponent<Rigidbody>();
        physics = GetComponent<BallPhysicsTest>();
        layerFloor = LayerMask.NameToLayer("Floor");
        //layerWall = LayerMask.NameToLayer("Wall");

        if (Application.isEditor && logToFile)
            Application.runInBackground = true;

        foreach (RotatePiece rtp in rtps)
        {
            if (rtp.enabled)
            {
                rtp.SetPTD(this);
            }
        }
        foreach (MovingPiece mvp in mvps)
        {
            if (mvp.enabled)
            {
                mvp.SetPTD(this);
            }
        }

        SetTagsForOOB(transform.parent);
    }

    void SetTagsForOOB(Transform t)
    {
        if (t.CompareTag("Player"))
            return;

        t.tag = "Hole 1";
        foreach (Transform child in t)
        {
            //Debug.Log("SetTag on : " + child.name);
            SetTagsForOOB(child);
        }
    }

    void LateUpdate()
    {
        if (flagEnableTrail)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = true;
            flagEnableTrail = false;
        }
    }

    void FixedUpdate()
    {
        // Handle all RotatePiece
        foreach (RotatePiece rtp in rtps)
        {
            if (rtp.enabled)
            {
                if (!rtp.flagStopRotation)
                {
                    /*if (!ReplayManager._instance.isReplayPlaying)
                    {*/
                    rtp.timer += Time.fixedDeltaTime;

                    if ((rtp.isRotation && rtp.timer > rtp.spinTime) || (!rtp.isRotation && rtp.timer > rtp.pauseTime))
                    {
                        if (!rtp.isRotation)
                        {
                            //Debug.Log("Rotation at : " + Time.frameCount);
                            rtp.isRotation = true;
                            rtp.timer = 0f;
                            rtp.coroutine = StartCoroutine(rtp.RotateMe(Vector3.up * rtp.rotationAngle, rtp.spinTime));
                        }
                    }
                    //}

                    // Apply the rotation the balls on top of the piece
                    if (rtp.isRotation)
                    {
                        if (rtp.ballsOnTop.Count > 0)
                        {
                            foreach (GameObject ball in rtp.ballsOnTop)
                            {
                                Vector3 ballPos = ball.transform.position;
                                Vector3 piecePos = rtp.transform.position;

                                Quaternion fromAngle = Quaternion.Euler(rtp.goalAngle.eulerAngles - new Vector3(0f, rtp.rotationAngle, 0f));
                                Quaternion toAngle = rtp.goalAngle;
                                Quaternion step = Quaternion.Inverse(fromAngle) * Quaternion.Slerp(fromAngle, toAngle, Time.fixedDeltaTime / rtp.spinTime);


                                Vector3 currentOffset = ballPos - piecePos;
                                Vector3 nextStepOffset = step * currentOffset;
                                Vector3 movement = nextStepOffset - currentOffset;

                                //Debug.Log("BallPos : " + ballPos + ", piecePos : " + piecePos + ", fromAngle : " + fromAngle.eulerAngles + ", toAngle : " + toAngle.eulerAngles + ", step : " + step.eulerAngles + ", currentOffset : " + currentOffset + ", nextStepOffset : " + nextStepOffset + ", movement : " + movement);

                                ball.transform.position += movement;
                            }
                        }
                    }
                }
            }
        }

        // Handle all moving pieces
        foreach (MovingPiece mvp in mvps)
        {
            if (mvp.enabled)
            {
                if (!mvp.flagStopMove)
                {
                    mvp.timer += Time.fixedDeltaTime;

                    if ((mvp.isMoving && mvp.timer > mvp.travelTime) || (!mvp.isMoving && mvp.timer > mvp.pauseTime))
                    {
                        mvp.isMoving = !mvp.isMoving;
                        mvp.timer = 0f;
                        if (mvp.isMoving)
                        {
                            mvp.forwardMove = !mvp.forwardMove;
                            mvp.coroutine = StartCoroutine(mvp.MoveMe((mvp.forwardMove) ? mvp.initPos : mvp.destPos,
                                                                       (mvp.forwardMove) ? mvp.destPos : mvp.initPos,
                                                                       mvp.travelTime));
                        }
                    }

                    // Move the balls on top of the piece while it is moving
                    if (mvp.isMoving)
                    {
                        if (mvp.ballsOnTop.Count > 0)
                        {
                            foreach (GameObject ball in mvp.ballsOnTop)
                            {
                                Vector3 start = (mvp.forwardMove) ? mvp.initPos : mvp.destPos;
                                Vector3 end = (mvp.forwardMove) ? mvp.destPos : mvp.initPos;
                                float lerpFactor = Time.fixedDeltaTime * (1.0f / mvp.travelTime);
                                Vector3 movement = Vector3.Lerp(start, end, lerpFactor) - start;


                                //Debug.Log("Start : " + start + ", end : " + end + ", movement.x : " + movement.x + ", movement.y : " + movement.y + ", movement.z : " + movement.z);
                                movement = CheckMovementBoundariesExceded(ball.transform.position, movement, end, mvp.transform.position);
                                //Debug.Log("Movement : " + movement.ToString("F6") + ", ball position : " + ball.transform.position.ToString("F6"));
                                ball.transform.position += movement;
                                //Debug.Log("Ball position after movement : " + ball.transform.position.ToString("F6"));
                            }
                        }
                    }
                }
            }
        }



        velocity = rb.velocity;
        velocityMagnitude = rb.velocity.magnitude;

        if (currentTimer == timerRestart)
        {
            transform.position = startPos;
            physics.AddForce(shootDirection.normalized * forceOfShot);

            ResetAllRTPs();
            ResetAllMVPs();
            framesFromStart = 0;
        }

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = false;

            physics.velocityCapped = Vector3.zero;
            physics.velocityTrue = Vector3.zero;

            if (logToFile && currentRun < nbRuns)
            {
                endPositions.Add(transform.position);
                totalEndPosition += transform.position;

                if (endPositionsCount.ContainsKey(transform.position))
                {
                    endPositionsCount[transform.position]++;
                }
                else
                {
                    endPositionsCount.Add(transform.position, 1);
                }


                currentRun++;

                if (currentRun == nbRuns)
                {
                    // Export data
                    ExportLogInfo();
                }
            }

            //Debug.Log("Frames from start : " + framesFromStart);
            physics.currentFloorNormal = Vector3.up;

            ResetAllRTPs();
            ResetAllMVPs();
            transform.position = startPos;
            //Debug.Log("Startpos : " + startPos.ToString("F6") + ", transform.position : " + transform.position.ToString("F6") + ", velocity : " + physics.velocityCapped.ToString("F6"));
            currentTimer = timerRestart;
            flagEnableTrail = true;
        }


        framesFromStart++;

        if (logToConsole && physics.velocityCapped.magnitude > 0f)
        {
            if (framesFromStart % 1 == 0 /*&& framesFromStart <= 75*/)
                Debug.Log("Frame : " + framesFromStart + ", position : " + transform.position.ToString("F8") + ", velocity : " + physics.velocityCapped.ToString("F8"));

            /*if (framesFromStart == 74)
                Debug.Break();*/
        }


    }


    // Checks if the movement added to the ball will make it overshoot the endGoal postion
    // Should help avoiding some of the collision issues
    private Vector3 CheckMovementBoundariesExceded(Vector3 ballPos, Vector3 movement, Vector3 endGoal, Vector3 piecePos)
    {
        Vector3 res = movement;

        bool checkX = false;
        bool checkY = false;
        bool checkZ = false;
        float offsetX = ballPos.x - piecePos.x;
        float offsetY = ballPos.y - piecePos.y;
        float offsetZ = ballPos.z - piecePos.z;
        checkX = (movement.x >= 0) ? (ballPos.x + movement.x - endGoal.x) > offsetX : (ballPos.x + movement.x - endGoal.x) < offsetX;
        checkY = (movement.y >= 0) ? (ballPos.y + movement.y - endGoal.y) > offsetY : (ballPos.y + movement.y - endGoal.y) < offsetY;
        checkZ = (movement.z >= 0) ? (ballPos.z + movement.z - endGoal.z) > offsetZ : (ballPos.z + movement.z - endGoal.z) < offsetZ;

        if (checkX) Debug.Log("Excessive movement on x ; ballPos.x : " + ballPos.x + ", movement.x : " + movement.x + ", endGoal.x : " + endGoal.x + "; calc : " + (ballPos.x + movement.x - endGoal.x));
        if (checkY) Debug.Log("Excessive movement on y ; ballPos.y : " + ballPos.y + ", movement.y : " + movement.y + ", endGoal.y : " + endGoal.y + "; calc : " + (ballPos.y + movement.y - endGoal.y));
        if (checkZ) Debug.Log("Excessive movement on z ; ballPos.z : " + ballPos.z + ", movement.z : " + movement.z + ", endGoal.z : " + endGoal.z + "; calc : " + (ballPos.z + movement.z - endGoal.z));

        if (checkX)
        {
            res.x += offsetX - (ballPos.x + movement.x - endGoal.x);
        }
        if (checkY)
        {
            res.y += offsetY - (ballPos.y + movement.y - endGoal.y);
        }
        if (checkZ)
        {
            res.z += offsetZ - (ballPos.z + movement.z - endGoal.z);
        }

        return res;
    }


    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 point = contact.point;
            Vector3 normal = contact.normal;

            if (contact.otherCollider.gameObject.layer == layerFloor)
            {
                if (/*contact.otherCollider.transform*/Vector3.up != normal)
                {
                    Debug.DrawRay(point, normal, Color.red, 3f);
                    //Debug.Log(normal);
                    //Debug.Break();
                }
            }
            else
            {
                //Debug.Log(collision.gameObject.name);
                //Debug.DrawRay(point, normal, Color.cyan, 1f);
            }
        }
    }

    private void ExportLogInfo()
    {
        Debug.Log("Export log");

        using (System.IO.StreamWriter file = System.IO.File.AppendText(@"C:\Users\Public\TestFolder\log" + (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds + Random.Range(0, 5000) + ".txt") /*new System.IO.StreamWriter(@"C:\Users\Public\TestFolder\log" + System.DateTime.Now.ToShortDateString() + ".txt", true)*/)
        {
            file.WriteLine("Log " + System.DateTime.Now + ", object : " + gameObject.name);

            // end positions
            Vector3 averageEndPos = totalEndPosition / endPositions.Count;

            Vector3 mostCommonEndPos = Vector3.zero;
            int maxOcc = 0;
            file.WriteLine("End positions by occurences | Total unique end positions : " + endPositionsCount.Count);
            foreach (KeyValuePair<Vector3, int> pair in endPositionsCount)
            {
                file.WriteLine("Position : x=" + pair.Key.x + ", y=" + pair.Key.y + ", z=" + pair.Key.z + " | Occurences=" + pair.Value);
                if (pair.Value > maxOcc)
                {
                    maxOcc = pair.Value;
                    mostCommonEndPos = pair.Key;
                }
            }

            int i = 1;
            float totalDist = 0f;
            float totalDist2 = 0f;
            file.WriteLine("");
            file.WriteLine("All runs : ");
            foreach (Vector3 endPos in endPositions)
            {
                float dist = Vector3.Distance(endPos, averageEndPos);
                float dist2 = Vector3.Distance(endPos, mostCommonEndPos);
                file.WriteLine("Run " + i + " endPosition : x=" + endPos.x + ", y=" + endPos.y + ", z=" + endPos.z + " | Distance to avg=" + dist + " | Distance to most common : " + dist2);
                totalDist += dist;
                totalDist2 += dist2;
                i++;
            }
            file.WriteLine("Average distance to the average end position : " + (totalDist / endPositions.Count));
            file.WriteLine("Average distance to the most common  end position " + "(" + maxOcc + " occurences out of " + endPositions.Count + "runs)" + " : " + (maxOcc != endPositions.Count ? (totalDist2 / (endPositions.Count - maxOcc)) : 0f));
        }
    }
}
