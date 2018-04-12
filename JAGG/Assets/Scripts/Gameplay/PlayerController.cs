using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SyncVar]
    public string playerName;

    public SyncListInt score;

    private GameObject guiCam;

    private Rigidbody rb;
    private PreviewLine line;
    private LobbyManager lobbyManager;
    public ParticleSystem trail;
    public ParticleSystem explosion;
    public Text playerNameText;

    public GameObject[] ball_parts;
    public GameObject failSign;

    private UIManager ui;

    private bool isShooting = false;
    private bool isPaused = false;
    private bool isOver = false;

    private Vector3 serverPos = Vector3.zero;
    private Queue<Vector3> serverPositions = new Queue<Vector3>();


    private Vector3 lastStopPos = Vector3.zero;

    private const int FirstLayer = 9;

    private bool flagEnableTrail = false;


    private Vector3 save_velocity = Vector3.zero;
    private bool hasDestroyedPlayer = false;
    private Vector3 restore_velocity = Vector3.zero;

    private bool firstShotLayerActivated = false;

    // Handling reset of position when out-of-bounds
    private float oobInitialResetTimer = 2.0f;
    private float oobActualResetTimer;
    private bool isOOB = false;

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
        lobbyManager = LobbyManager._instance;

        playerNameText.text = playerName;

        if (!isServer)
            rb.isKinematic = true;

        if (isLocalPlayer)
        {
            playerNameText.gameObject.SetActive(false);
            ui.SetParList();

            oobActualResetTimer = oobInitialResetTimer;

            if (gameObject.layer == 0)
                CmdGetLayer();
        }
    }


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Camera.main.GetComponent<BallCamera>().shouldFollow = false;
                guiCam.GetComponent<BallCamera>().shouldFollow = false;
                canShoot = false;
                ui.ShowPause(
                    delegate()
                    {
                        Camera.main.GetComponent<BallCamera>().shouldFollow = true;
                        guiCam.GetComponent<BallCamera>().shouldFollow = true;
                        isPaused = false;
                        canShoot = true;
                    },
                    delegate ()
                    {
                        if (isServer)
                            lobbyManager.StopHost();
                        else
                            lobbyManager.StopClient();

                        lobbyManager.ReturnToLobby();
                    }
                );
                isPaused = true;
            }
            else
            {
                Camera.main.GetComponent<BallCamera>().shouldFollow = true;
                guiCam.GetComponent<BallCamera>().shouldFollow = true;
                ui.HidePauseMenu();
                isPaused = false;
                canShoot = true;
            }
        }


        if(isServer)
        {
            isMoving = rb.velocity.magnitude >= 0.001f;
            /*if(isMoving)
                RpcUpdatePosition(transform.position);
            else*/
            RpcUpdatePosition(transform.position); // Toujours update la position
            if (!isMoving)
            {
                // Update the last position where the ball stopped
                lastStopPos = transform.position;

                int maxShot = lobbyManager.GetMaxShot();
                if (shots == maxShot)
                {
                    CmdOutOfStrokes();
                }
            }
            
            save_velocity = rb.velocity;
        }

        // Enables the particles at the next frame (when resetting / moving to the next hole) to avoid weird trails effects
        if (flagEnableTrail)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = true;
            flagEnableTrail = false;
        }

        if (!isLocalPlayer || isOver)
        {
            return;
        }

        if (!isMoving)
        {
            // Enable collision with other players only after the end of the first shot
            if (shots == 1 && !firstShotLayerActivated)
            {
                // Debug.Log("Enabling collisions for " + LayerMask.LayerToName(gameObject.layer));
                firstShotLayerActivated = true;
                CmdEnableMyCollisionLayers();
            }

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

        // Handle oob
        if(!Physics.Raycast(new Vector3(transform.position.x,transform.position.y - 0.05f, transform.position.z), Vector3.down))
        {
            if (isOOB)
            {
                //Debug.Log(oobActualResetTimer);
                oobActualResetTimer -= Time.deltaTime;
                if(oobActualResetTimer < 0f)
                {
                    isOOB = false;
                    CmdResetPosition(lastStopPos);
                }
            }
            else
            {
                isOOB = true;
                oobActualResetTimer = oobInitialResetTimer;
            }
        }
        else
        {
            isOOB = false;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if(isPaused)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        guiCam = GameObject.FindWithTag("GUICamera");
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
        if (isServer)
        {
            GameObject collided = collision.gameObject;
            if (collided.CompareTag("Player"))
            {
                PlayerController pc_col = collided.GetComponent<PlayerController>();
                Vector3 other_vel = pc_col.save_velocity;

                //Debug.Log(Time.frameCount + "fr, BALLZ : " + LayerMask.LayerToName(collided.layer) + " , my vel : " + save_velocity + ", his vel : " + other_vel);
                

                if ((save_velocity.magnitude > other_vel.magnitude + 3f) && !hasDestroyedPlayer)
                {
                    // Disable collisions with other balls while destroyed
                    for (int i = FirstLayer; i < FirstLayer + 4; i++)
                    {
                        if (i != collided.layer)
                            Physics.IgnoreLayerCollision(collided.layer, i, true);
                    }

                    CmdDestroyBall(collided);
                    hasDestroyedPlayer = true;
                    restore_velocity = save_velocity;
                }
            }
        }
    }


    void OnCollisionExit(Collision collisionInfo)
    {
        if(collisionInfo.gameObject.CompareTag("Player") && hasDestroyedPlayer)
        {
            hasDestroyedPlayer = false;
            rb.velocity = restore_velocity;
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
        firstShotLayerActivated = false;
        shots = 0;

        if (failSign.activeSelf)
        {
            RpcChangeFailSignVisibility(false);
        }
    }

    public void EnablePlayer()
    {
        CmdEnablePlayer();
    }


    public void EnableMyCollisionLayers()
    {
        CmdEnableMyCollisionLayers();
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

    public void DisablePlayer()
    {
        CmdDisablePlayer();
    }

    [Command]
    private void CmdDisablePlayer()
    {
        canShoot = false;
        rb.velocity = Vector3.zero;
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
            {
                //Debug.Log("My layer : " + LayerMask.LayerToName(gameObject.layer) + ", other layer : " + LayerMask.LayerToName(i));
                Physics.IgnoreLayerCollision(gameObject.layer, i, true);
            }
        }

        score.Add(shots+2);
        shots = 0;
        RpcDisablePlayerInHole(-2);
        RpcChangeFailSignVisibility(true);
    }


    public void ResetPosition()
    {
        CmdResetPosition(lastStopPos);
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


    [Command]
    private void CmdDestroyBall(GameObject ball)
    {
        PlayerController ball_pc = ball.GetComponent<PlayerController>();
        ball_pc.DisablePlayer();
        ball_pc.ChangeBallVisibility(false);
        ball_pc.ExplodeBall();
        
        StartCoroutine(RespawnRoutine(ball));
    }

    private IEnumerator RespawnRoutine(GameObject ball)
    {
        yield return new WaitForSeconds(3);
        PlayerController ball_pc = ball.GetComponent<PlayerController>();
        ball_pc.ResetPosition();
        ball_pc.ChangeBallVisibility(true);
        ball_pc.EnableMyCollisionLayers();
        ball_pc.EnablePlayer();
    }

    public void ChangeBallVisibility(bool visi)
    {
        CmdChangeBallVisibility(visi);
    }

    [Command]
    private void CmdChangeBallVisibility(bool visi)
    {
        RpcChangeBallVisibility(visi);
    }

    public void ExplodeBall()
    {
        CmdExplodeBall();
    }

    [Command]
    private void CmdExplodeBall()
    {
        RpcExplodeBall();
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
        ParticleSystem.EmissionModule em = trail.emission;
        em.enabled = false;
        
        serverPositions.Clear();
        transform.position = position;
        serverPos = position;
        
        flagEnableTrail = true;
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

            SetDone();
            StartCoroutine(ui.ShowNotification(message, 5));
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
            firstShotLayerActivated = false;
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
    private void RpcChangeFailSignVisibility(bool visi)
    {
        failSign.SetActive(visi);
        if(visi)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir.y = 0;
            failSign.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    [ClientRpc]
    private void RpcChangeBallVisibility(bool visi)
    {
        foreach (GameObject go in ball_parts)
            go.SetActive(visi);
    }


    [ClientRpc]
    private void RpcExplodeBall()
    {
        Camera.main.GetComponent<BallCamera>().isShaking = true;
        explosion.Play();
    }
    #endregion
}
