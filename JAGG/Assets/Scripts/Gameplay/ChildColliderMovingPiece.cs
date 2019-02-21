using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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
        //Debug.Log("CCMVP : " + ball);
        if (ball.layer >= layerBall && ball.layer <= layerBall4)
        {
            if (rtpParent != null)
            {
                if (!rtpParent.ballsOnTop.Contains(ball))
                {
                    rtpParent.ballsOnTop.Add(ball);
                    if (SceneManager.GetSceneAt(0).name != "PhysicsTest" && SceneManager.GetSceneAt(0).name != "LevelEditor") // TODO : Ajouter la scène de Replay finale (quand elle existera)
                        ball.GetComponent<PlayerController>().isOnRtpMvp++;
                }
            }
            if (mvpParent != null)
            {
                if (!mvpParent.ballsOnTop.Contains(ball))
                {
                    mvpParent.ballsOnTop.Add(ball);
                    if (SceneManager.GetSceneAt(0).name != "PhysicsTest" && SceneManager.GetSceneAt(0).name != "LevelEditor")
                        ball.GetComponent<PlayerController>().isOnRtpMvp++;
                }
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
                if (SceneManager.GetSceneAt(0).name != "PhysicsTest" && SceneManager.GetSceneAt(0).name != "LevelEditor")
                    ball.GetComponent<PlayerController>().isOnRtpMvp--;
            }
            if (mvpParent != null)
            {
                mvpParent.ballsOnTop.Remove(ball);
                if (SceneManager.GetSceneAt(0).name != "PhysicsTest" && SceneManager.GetSceneAt(0).name != "LevelEditor")
                    ball.GetComponent<PlayerController>().isOnRtpMvp--;
            }


            if (rtpParent == null && mvpParent == null)
                Debug.LogError("Impossibru !?!");
        }
    }
}
