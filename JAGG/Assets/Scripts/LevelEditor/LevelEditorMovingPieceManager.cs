using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditorMovingPieceManager : MonoBehaviour
{

    public static LevelEditorMovingPieceManager _instance;

    private List<RotatePiece> rotatePieces;
    private List<MovingPiece> movingPieces;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        rotatePieces = new List<RotatePiece>();
        movingPieces = new List<MovingPiece>();
        if (SceneManager.GetSceneAt(0).name == "ReplayTest")
        {
            GrabAllRotatePieces();
            GrabAllMovingPieces();
        }
    }

    public void GrabAllRotatePieces()
    {
        rotatePieces = new List<RotatePiece>(FindObjectsOfType<RotatePiece>());
    }

    public void GrabAllMovingPieces()
    {
        movingPieces = new List<MovingPiece>(FindObjectsOfType<MovingPiece>());
    }

    public void AddRotatePiece(RotatePiece rtp)
    {
        rotatePieces.Add(rtp);
    }

    public void AddMovingPiece(MovingPiece mvp)
    {
        movingPieces.Add(mvp);
    }

    public void ResetAllRTPs()
    {
        foreach (RotatePiece rtp in rotatePieces)
        {
            if (rtp.enabled)
            {
                rtp.Reset();
            }
        }
    }

    public void ResetAllMVPs()
    {
        foreach (MovingPiece mvp in movingPieces)
        {
            if (mvp.enabled)
            {
                mvp.Reset();
            }
        }
    }

    public void StopMyCoroutine(RotatePiece rtp)
    {
        /*if (rtp.coroutine != null)
            StopCoroutine(rtp.coroutine);*/
    }
    public void StopMyCoroutine(MovingPiece mvp)
    {
        if (mvp.coroutine != null)
            StopCoroutine(mvp.coroutine);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handle all RotatePiece
        foreach (RotatePiece rtp in rotatePieces)
        {
            if (rtp.enabled)
            {
                if (!rtp.flagStopRotation)
                {
                    rtp.timer += Time.fixedDeltaTime;

                    /*if ((rtp.isRotation && rtp.timer > rtp.spinTime) || (!rtp.isRotation && rtp.timer > rtp.pauseTime))
                    {
                        if (!rtp.isRotation)
                        {
                            //Debug.Log("Rotation at : " + ReplayManager._instance.fixedFrameCount);
                            rtp.isRotation = true;
                            rtp.timer = 0f;
                            rtp.coroutine = StartCoroutine(rtp.RotateMe(Vector3.up * rtp.rotationAngle, rtp.spinTime));
                        }
                    }*/ // TODO Replace coroutine


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
        foreach (MovingPiece mvp in movingPieces)
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
                                ball.transform.position += movement;
                            }
                        }
                    }
                }
            }
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

        //if (checkX) Debug.Log("Excessive movement on x ; ballPos.x : " + ballPos.x + ", movement.x : " + movement.x + ", endGoal.x : " + endGoal.x + "; calc : " + (ballPos.x + movement.x - endGoal.x));
        //if (checkY) Debug.Log("Excessive movement on y ; ballPos.y : " + ballPos.y + ", movement.y : " + movement.y + ", endGoal.y : " + endGoal.y + "; calc : " + (ballPos.y + movement.y - endGoal.y));
        //if (checkZ) Debug.Log("Excessive movement on z ; ballPos.z : " + ballPos.z + ", movement.z : " + movement.z + ", endGoal.z : " + endGoal.z + "; calc : " + (ballPos.z + movement.z - endGoal.z));

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
}
