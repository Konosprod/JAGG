using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RotatePieceManager : NetworkBehaviour {

    public static RotatePieceManager _instance;

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

    public void grabAllRotatePieces()
    {
        rotatePieces = new List<RotatePiece>(FindObjectsOfType<RotatePiece>());
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

    // Update is called once per frame
    void Update()
    {
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
                }
            }
        }
    }
}
