using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ChildColliderMovingPiece : MonoBehaviour
{

    private const int layerBall = 9;
    private const int layerBall2 = 10;
    private const int layerBall3 = 11;
    private const int layerBall4 = 12;

    
    private RotatePiece rtpParent = null;
    private MovingPiece mvpParent = null;

    // Use this for initialization
    void Start()
    {

    }

    public void SetRtpParent(RotatePiece rotatePieceParent)
    {
        rtpParent = rotatePieceParent;
    }

    public void SetMvpParent(MovingPiece movingPieceParent)
    {
        mvpParent = movingPieceParent;
    }


    void OnCollisionStay(Collision collisionInfo)
    {
        GameObject ball = collisionInfo.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            //Debug.Log("Collision with ball");
            if (rtpParent != null)
            {
                if (!rtpParent.ballsOnTop.Contains(ball))
                    rtpParent.ballsOnTop.Add(ball);
            }
            else if (mvpParent != null)
            {
                if (!mvpParent.ballsOnTop.Contains(ball))
                    mvpParent.ballsOnTop.Add(ball);
            }
            else
                Debug.LogError("Impossibru !?!");
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        GameObject ball = collisionInfo.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            //Debug.Log("End of collision with ball");
            if (rtpParent != null)
            {
                rtpParent.ballsOnTop.Remove(ball);
            }
            else if (mvpParent != null)
            {
                mvpParent.ballsOnTop.Remove(ball);
            }
            else
                Debug.LogError("Impossibru !?!");
        }
    }
}
