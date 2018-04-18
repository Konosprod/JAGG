using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MovingPieceManager : NetworkBehaviour {

    public static MovingPieceManager _instance;

    private List<RotatePiece> rotatePieces;
    
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

        DontDestroyOnLoad(this.gameObject);
        
        rotatePieces = new List<RotatePiece>();
    }

    public void GrabAllRotatePieces()
    {
        rotatePieces = new List<RotatePiece>(FindObjectsOfType<RotatePiece>());
    }

    public void ClearRotatePieces()
    {
        rotatePieces.Clear();
    }

    public void AddRotatePiece(RotatePiece rtp)
    {
        rotatePieces.Add(rtp);
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
        if (!isServer)
            return;


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
                            RpcStartCoroutine(rotatePieces.FindIndex(x => x == rtp));
                        }
                    }


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
    }

    [ClientRpc]
    private void RpcStartCoroutine(int rtpId)
    {
        if (!isServer)
        {
            if (rotatePieces.Count == 0)
                GrabAllRotatePieces();
                    
            RotatePiece rtp = rotatePieces[rtpId];
            rtp.coroutine = StartCoroutine(rtp.RotateMe(Vector3.up * rtp.rotationAngle, rtp.spinTime));
        }
    }
}
