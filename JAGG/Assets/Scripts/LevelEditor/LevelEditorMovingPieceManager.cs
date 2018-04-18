using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (rtp.coroutine != null)
            StopCoroutine(rtp.coroutine);
    }
    public void StopMyCoroutine(MovingPiece mvp)
    {
        if (mvp.coroutine != null)
            StopCoroutine(mvp.coroutine);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle all RotatePiece
        foreach (RotatePiece rtp in rotatePieces)
        {
            if (rtp.enabled)
            {
                if (!rtp.flagStopRotation)
                {
                    rtp.timer += Time.deltaTime;

                    if ((rtp.isRotation && rtp.timer > rtp.spinTime) || (!rtp.isRotation && rtp.timer > rtp.pauseTime))
                    {
                        rtp.isRotation = !rtp.isRotation;
                        rtp.timer = 0f;
                        if (rtp.isRotation)
                        {
                            rtp.coroutine = StartCoroutine(rtp.RotateMe(Vector3.up * rtp.rotationAngle, rtp.spinTime));
                        }
                    }

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
                                Quaternion step = Quaternion.Inverse(fromAngle) * Quaternion.Slerp(fromAngle, toAngle, Time.deltaTime / rtp.spinTime);


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
                    mvp.timer += Time.deltaTime;

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
                                float lerpFactor = Time.deltaTime * (1.0f / mvp.travelTime);
                                Vector3 movement = Vector3.Lerp(start, end, lerpFactor) - start;

                                //Debug.Log("Start : " + start + ", end : " + end + ", movement : " + movement);

                                ball.transform.position += movement;
                            }
                        }
                    }
                }
            }
        }
    }
}
