using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

[NetworkSettings(sendInterval = 0f)]
public class PlayerController : NetworkBehaviour {

    [SyncVar]
    bool canShoot = true;

    [SyncVar]
    int shots = 0;

    [SyncVar]
    public bool isMoving = false;

    [SyncVar]
    public bool done = false;

    public SyncListInt score;

    private Rigidbody rb;
    private PreviewLine line;

    private UIManager ui;

    private bool isShooting = false;

    private bool isOver = false;

    private Vector3 serverPos = Vector3.zero;
    private Queue<Vector3> serverPositions = new Queue<Vector3>();

    private Vector3 lastStopPos = Vector3.zero;

    private const int FirstLayer = 9;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        line = GetComponent<PreviewLine>();

        ui = FindObjectOfType<UIManager>();

        if (!isServer)
            rb.isKinematic = true;

    }


    private void Update()
    {
        if(isServer)
        {
            isMoving = rb.velocity.magnitude >= 0.001f;
            if(isMoving)
                RpcUpdatePosition(transform.position);
        }

        if (!isLocalPlayer || isOver)
        {
            return;
        }

        if (!isMoving)
        {
            // Enable collision with other players only after the end of the first shot
            if (shots == 1)
                CmdEnableMyCollisionLayers();

            // Update the last position where the ball stopped
            lastStopPos = transform.position;


            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;


            // Handle shooting
            // Should likely be the last part of the if since the ball will start moving when calling CmdShoot so anything that's in this if(!isMoving) wouldn't be relevant past that point sometimes
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                if (isShooting)
                {
                    CmdShoot(dir, ui.GetSliderValue());
                    isShooting = false;
                    isMoving = true;
                    ui.ResetSlider();
                }
                else
                    isShooting = true;
            }

            if(Input.GetMouseButtonDown(1) && isShooting)
            {
                isShooting = false;
                ui.ResetSlider();
            }
        }
        else
        {
            // Handle the reset button
            if(Input.GetKeyDown(KeyCode.R))
            {
                CmdResetPosition(lastStopPos);
            }
        }

        if (isShooting)
            ui.UpdateSlider();
    }

    public override void OnStartLocalPlayer()
    {
        GameObject guiCam = GameObject.FindWithTag("GUICamera");
        guiCam.GetComponent<BallCamera>().target = transform;
        Camera.main.GetComponent<BallCamera>().target = transform;
        line = GetComponent<PreviewLine>();
        line.enabled = true;
    }

    private void FixedUpdate()
    {
        if (!isServer)
        {
            if ((transform.position - serverPos).magnitude < 0.1f && serverPositions.Count > 0)
            {
                serverPos = serverPositions.Dequeue();
            }

            float lerpRate = 0.5f;

            if (serverPos != Vector3.zero)
            {
                transform.position = Vector3.Lerp(transform.position, serverPos, lerpRate);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.CompareTag("Hole"))
        {
            if (isLocalPlayer)
            {
                CmdPlayerInHole();
            }
        }
        else if(LayerMask.LayerToName(otherGO.layer) == "BoosterPad")
        {
            Vector3 dir = otherGO.transform.forward.normalized;
            BoosterPad bp = otherGO.GetComponent<BoosterPad>();
            float multFactor = bp.multFactor;
            float addFactor = bp.addFactor;
            if(isLocalPlayer)
            {
                CmdBoost(dir,multFactor, addFactor);
            }
        }
        else
        {
            Debug.LogError("Ball entered unexpected trigger : " + other.gameObject.name);
        }
    }

    void OnGUI()
    {
        if (isLocalPlayer)
            ui.SetTextShots("Shots: " + shots.ToString());
    }

    public void SetDone()
    {
        CmdSetDone();
    }

    public void ResetPlayer()
    {
        CmdResetPlayer();
    }

    public void ShowScores()
    {
        RpcShowScores();
    }

    #region Command

    [Command]
    private void CmdEnablePlayer()
    {
        canShoot = true;
        RpcEnablePlayer();
    }

    [Command]
    private void CmdResetPlayer()
    {
        done = false;
        shots = 0;
    }

    public void EnablePlayer()
    {
        CmdEnablePlayer();
    }

    [Command]
    private void CmdEnableMyCollisionLayers()
    {
        for (int i = FirstLayer; i < FirstLayer + 4; i++)
        {
            Physics.IgnoreLayerCollision(gameObject.layer, i, false);
        }
    }

    [Command]
    void CmdShoot(Vector3 dir, float sliderVal)
    {
        rb.AddForce(dir * sliderVal * 10f);
        shots++;
    }

    [Command]
    private void CmdDisablePlayer()
    {
        canShoot = false;
        RpcDisablePlayer();
    }

    [Command]
    private void CmdPlayerInHole()
    {
        int type = -1;
        canShoot = false;
        rb.velocity = Vector3.zero;

        int par = GameObject.FindObjectOfType<LobbyManager>().GetPar();

        if(shots == 1)
        {
            type = 0;
        }
        else
        {
            int diff = par - shots;

            switch(diff)
            {
                case -2:
                    type = 5;
                    break;

                case -1:
                    type = 4;
                    break;

                case 0:
                    type = 3;
                    break;

                case 1:
                    type = 2;
                    break;

                case 2:
                    type = 1;
                    break;
            }
        }

        // Disable collisions with other balls while in the hole
        for (int i = FirstLayer; i < FirstLayer + 4; i++)
        {
            if (i != gameObject.layer)
                Physics.IgnoreLayerCollision(gameObject.layer, i, true);
        }

        score.Add(shots);
        shots = 0;
        RpcDisablePlayerInHole(type);
    }


    [Command]
    private void CmdResetPosition(Vector3 lastPos)
    {
        transform.position = lastPos;
        rb.velocity = Vector3.zero;
        RpcForceUpdatePosition(lastPos);
    }

    [Command]
    private void CmdSetDone()
    {
        done = true;
    }


    [Command]
    private void CmdBoost(Vector3 dir, float multFactor, float addFactor)
    {
        rb.velocity *= multFactor;
        rb.AddForce(dir * addFactor);
    }
#endregion

    #region ClientRpc

    [ClientRpc]
    void RpcShowScores()
    {
        StartCoroutine(test());
    }

    private IEnumerator test()
    {
        ui.ShowScores();
        yield return new WaitForSeconds(5);
        ui.HideScores();
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 position)
    {
        //transform.position = position;
        //serverPos = position;
        if (serverPos == Vector3.zero)
            serverPos = position;
        else
            serverPositions.Enqueue(position);
    }

    [ClientRpc]
    void RpcForceUpdatePosition(Vector3 position)
    {
        serverPositions.Clear();
        transform.position = position;
        serverPos = position;
    }

    [ClientRpc]
    private void RpcDisablePlayerInHole(int type)
    {
        if (isLocalPlayer)
        {
            string message = "";
            isOver = true;
            line.SetEnabled(false);
            line.enabled = false;

            switch(type)
            {
                case -1:
                    message = "NOT SURE WHAT I'M SUPPOSED TO WRITE FOR SUCH A BAD PLAY. LOL";
                    break;

                case 0:
                    message = "Hole in one";
                    break;

                case 1:
                    message = "Eagle";
                    break;

                case 2:
                    message = "Birdie";
                    break;

                case 3:
                    message = "Par";
                    break;

                case 4:
                    message = "Bogey";
                    break;

                case 5:
                    message = "Double Bogey";
                    break;
            }

            StartCoroutine(ui.ShowNotification(message, 1, SetDone));
        }
    }

    [ClientRpc]
    private void RpcDisablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = true;
            line.SetEnabled(false);
            line.enabled = false;
        }
    }

    [ClientRpc]
    private void RpcResetPlayer()
    {
        if(isLocalPlayer)
        {
            done = false;
            shots = 0;
        }
    }

    [ClientRpc]
    private void RpcEnablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = false;
            line.enabled = true;
        }
    }

#endregion
}
