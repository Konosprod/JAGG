using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ChildColliderRotatePiece : MonoBehaviour
{

    private const int layerBall = 9;
    private const int layerBall2 = 10;
    private const int layerBall3 = 11;
    private const int layerBall4 = 12;

    
    private RotatePiece rtpParent;

    // Use this for initialization
    void Start()
    {

    }

    public void SetRtpParent(RotatePiece rotatePieceParent)
    {
        rtpParent = rotatePieceParent;
    }


    void OnCollisionStay(Collision collisionInfo)
    {
        GameObject ball = collisionInfo.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            //Debug.Log("Collision with ball");
            if(!rtpParent.ballsOnTop.Contains(ball))
                rtpParent.ballsOnTop.Add(ball);
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        GameObject ball = collisionInfo.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            //Debug.Log("End of collision with ball");
            rtpParent.ballsOnTop.Remove(ball);
        }
    }
}
