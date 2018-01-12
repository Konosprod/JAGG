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
    public int shots = 0;

    [SyncVar]
    public bool isMoving = false;

    [SyncVar]
    public bool done = false;

    public SyncListInt score;

    private Rigidbody rb;
    private PreviewLine line;
    private LobbyManager lobbyManager;
    public ParticleSystem particleSys;
    public GameObject failSign;

    private UIManager ui;

    private bool isShooting = false;

    private bool isOver = false;

    private Vector3 serverPos = Vector3.zero;
    private Queue<Vector3> serverPositions = new Queue<Vector3>();

    private Vector3 lastStopPos = Vector3.zero;

    private const int FirstLayer = 9;

    private bool flagEnableParticle = false;

    private void Awake()
    {
        ui = FindObjectOfType<UIManager>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        line = GetComponent<PreviewLine>();
        lobbyManager = GameObject.FindObjectOfType<LobbyManager>();


        if (!isServer)
            rb.isKinematic = true;

        if (isLocalPlayer)
        {
            if (gameObject.layer == 0)
                CmdGetLayer();
        }
    }


    private void Update()
    {
        if(isServer)
        {
            isMoving = rb.velocity.magnitude >= 0.001f;
            if(isMoving)
                RpcUpdatePosition(transform.position);
            else
            {
                int maxShot = lobbyManager.hole.GetComponentInChildren<LevelProperties>().maxShot;
                if (shots == maxShot)
                {
                    CmdOutOfStrokes();
                }
            }
        }
        else
        {
            if (flagEnableParticle)
            {
                ParticleSystem.EmissionModule em = particleSys.emission;
                em.enabled = true;
                flagEnableParticle = false;
            }
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

            // Cancel the shooting attempt with right-click
            if(Input.GetMouseButtonDown(1) && isShooting)
            {
                isShooting = false;
                ui.ResetSlider();
            }
        }
        else
        {
            // If we get put in motion while trying to shoot we stop and reset the slider
            if(isShooting)
            {
                isShooting = false;
                ui.ResetSlider();
            }

            // Handle the reset button
            if(Input.GetKeyDown(KeyCode.R) && lastStopPos != Vector3.zero)
            {
                CmdResetPosition(lastStopPos);
            }
        }
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

        if(isLocalPlayer)
        {
            if (isShooting)
                ui.UpdateSlider();
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

    void OnCollisionEnter(Collision collision)
    {
        GameObject collided = collision.gameObject;
        if (collided.CompareTag("Player"))
        {
            Debug.Log("BALLZ");
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

        if(failSign.activeSelf)
        {
            RpcChangeFailSignVisibility(false);
        }
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
            // We check if the player on that specific layer is done or not to avoid enabling collisions on a player already in the hole
            if(!lobbyManager.playerManager.isPlayerOnLayerDone(i))
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

        int par = lobbyManager.GetPar();

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
    private void CmdOutOfStrokes()
    {
        canShoot = false;
        rb.velocity = Vector3.zero;

        // Disable collisions with other balls while in the hole
        for (int i = FirstLayer; i < FirstLayer + 4; i++)
        {
            if (i != gameObject.layer)
                Physics.IgnoreLayerCollision(gameObject.layer, i, true);
        }

        score.Add(shots+2);
        shots = 0;
        RpcDisablePlayerInHole(-2);
        RpcChangeFailSignVisibility(true);
    }


    [Command]
    private void CmdResetPosition(Vector3 lastPos)
    {
        rb.velocity = Vector3.zero;
        transform.position = lastPos;
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
        float angle = Vector3.Angle(rb.velocity, dir);
        rb.velocity *= multFactor * (angle>90f?-0.1f:1f);
        rb.AddForce(dir * addFactor);
    }


    [Command]
    private void CmdGetLayer()
    {
        RpcSetLayer(gameObject.layer);
    }


    public void ForcedMoveTo(Vector3 position)
    {
        CmdForcedMoveTo(position);
    }

    [Command]
    private void CmdForcedMoveTo(Vector3 position)
    {
        transform.position = position;
        RpcForceUpdatePosition(position);
    }
#endregion

    #region ClientRpc

    [ClientRpc]
    void RpcShowScores()
    {
        StartCoroutine(ShowScoresRoutine());
    }

    private IEnumerator ShowScoresRoutine()
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
        ParticleSystem.EmissionModule em = particleSys.emission;
        if (!isServer)
            em.enabled = false;
        
        serverPositions.Clear();
        transform.position = position;
        serverPos = position;

        if (!isServer)
            flagEnableParticle = true;
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
            lastStopPos = Vector3.zero;

            switch (type)
            {
                /*case -3:
                    message = "Out of time";
                    break;*/

                case -2:
                    message = "If at first you don't succeed,\nTry, try, try again.\nOn the next hole that is.";
                    break;

                case -1:
                    message = "Better luck next time.";
                    break;

                case 0:
                    message = "Hole in one.";
                    break;

                case 1:
                    message = "Eagle.";
                    break;

                case 2:
                    message = "Birdie.";
                    break;

                case 3:
                    message = "Par.";
                    break;

                case 4:
                    message = "Bogey.";
                    break;

                case 5:
                    message = "Double Bogey.";
                    break;

                default:
                    message = "UNEXPECTED RESULT : " + type + ", PLEASE REPORT / SCREEN";
                    break;
            }

            StartCoroutine(ui.ShowNotification(message, 2, SetDone));
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

    [ClientRpc]
    private void RpcSetLayer(int layer)
    {
        gameObject.layer = layer;
    }

    [ClientRpc]
    private void RpcChangeFailSignVisibility(bool vis)
    {
        failSign.SetActive(vis);
        if(vis)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir.y = 0;
            failSign.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    [ClientRpc]
    private void RpcActivateParticles()
    {
        flagEnableParticle = true;
    }



#endregion
}
