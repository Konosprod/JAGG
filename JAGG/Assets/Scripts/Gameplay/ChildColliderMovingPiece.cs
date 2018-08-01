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


    void OnTriggerEnter(Collider col)
    {
        GameObject ball = col.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            if (rtpParent != null)
            {
                if (!rtpParent.ballsOnTop.Contains(ball))
                    rtpParent.ballsOnTop.Add(ball);
            }
            if (mvpParent != null)
            {
                if (!mvpParent.ballsOnTop.Contains(ball))
                    mvpParent.ballsOnTop.Add(ball);
            }
            

            if(rtpParent == null && mvpParent == null)
                Debug.LogError("Impossibru !?!");
        }
    }

    void OnTriggerExit(Collider col)
    {
        GameObject ball = col.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            if (rtpParent != null)
            {
                rtpParent.ballsOnTop.Remove(ball);
            }
            if (mvpParent != null)
            {
                mvpParent.ballsOnTop.Remove(ball);
            }


            if (rtpParent == null && mvpParent == null)
                Debug.LogError("Impossibru !?!");
        }
    }

    /*void OnCollisionStay(Collision collisionInfo)
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
    }*/

    void OnCollisionExit(Collision collisionInfo)
    {
        GameObject ball = collisionInfo.gameObject;
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            //Debug.Log("End of collision with ball, frame : " + Time.frameCount);

            // We use a raycast to fix those annoying collision issues (Raycasts are love, Raycasts are life)
            bool mustRemoveBall = true;
            RaycastHit hitBallToBottom;
            bool ballToBottom = Physics.Linecast(ball.GetComponent<Collider>().bounds.max + ball.GetComponent<Rigidbody>().velocity.normalized / 10f, ball.GetComponent<Collider>().bounds.max + ball.GetComponent<Rigidbody>().velocity.normalized / 10f + Vector3.down, out hitBallToBottom, ~(1 << 30 | 1 << 9 | 1 << 10 | 1 << 11 | 1 << 12));

            //Debug.DrawLine(ball.GetComponent<Collider>().bounds.max + ball.GetComponent<Rigidbody>().velocity.normalized / 10f, ball.GetComponent<Collider>().bounds.max + ball.GetComponent<Rigidbody>().velocity.normalized / 10f + Vector3.down, Color.red, 20f);

            if (ballToBottom)
            {
                if (hitBallToBottom.transform.gameObject != gameObject)
                {
                    if (hitBallToBottom.transform.parent == transform.parent) // This test assumes that prefabs do not have nested pieces; which isn't always true (TODO Matthieu)
                    {
                        //Debug.Log("Same parent, no need to remove the ball from BallsOnTop");
                        mustRemoveBall = false;
                        //Debug.Break();
                    }
                    /*else
                        Debug.Log("Didn't hit the piece we were on when raycasting downwards, hit : " + hitBallToBottom.transform.gameObject.name + ", expected : " + gameObject.name);*/

                }
                else // Same object found with the raycast so this collision exit is bullshits
                {
                    mustRemoveBall = false;
                    //Debug.Break();
                }
                    
            }


            if (mustRemoveBall && rtpParent != null)
            {
                rtpParent.ballsOnTop.Remove(ball);
            }
            else if (mustRemoveBall && mvpParent != null)
            {
                /*Debug.Log("ballToBottom : " + ballToBottom);
                if (ballToBottom)
                    Debug.Log("Object hit : " + hitBallToBottom.transform.gameObject + ", its parent : " + hitBallToBottom.transform.parent + ", myself : " + gameObject + ", my parent : " + transform.parent);*/

                mvpParent.ballsOnTop.Remove(ball);
            }
            else if(rtpParent == null && mvpParent == null)
                Debug.LogError("Impossibru !?!");
        }
    }
}
