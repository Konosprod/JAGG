using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

[NetworkSettings(sendInterval = 0f)]
public class PlayerController : NetworkBehaviour {
    
    [SyncVar]
    bool canShoot = true;

    [SyncVar]
    int shots = 0;

    [SyncVar]
    public bool isMoving = false;

    public int minSliderVal = 10;
    public int maxSliderVal = 150;

    private Slider slider;
    private Rigidbody rb;
    private LineRenderer line;

    private Text textShots;
    private Text notificationText;

    private bool isShooting = false;
    private bool slideUp = true;

    private PlayerManager playerManager;
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
        line = GetComponent<LineRenderer>();

        slider = GameObject.Find("Slider").GetComponent<Slider>();
        textShots = GameObject.Find("PlayerShots").GetComponent<Text>();
        notificationText = GameObject.Find("NotificationText").GetComponent<Text>();
        playerManager = PlayerManager.Instance;

        slider.minValue = minSliderVal;
        slider.maxValue = maxSliderVal;

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

            
            // Show and update the preview line
            if (!line.enabled)
                line.enabled = true;

            line.SetPosition(0, transform.position);
            line.SetPosition(1, dir / 1.3f + transform.position);

            // Handle shooting
            // Should likely be the last part of the if since the ball will start moving when calling CmdShoot so anything that's in this if(!isMoving) wouldn't be relevant past that point sometimes
            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                if (isShooting)
                {
                    CmdShoot(dir, slider.value);
                    isShooting = false;
                    isMoving = true;
                    slideUp = true;
                    slider.value = minSliderVal;
                }
                else
                    isShooting = true;
            }
        }
        else
        {
            // Disable the preview line during movement (looks pretty bad otherwise)
            line.enabled = false;

            // Handle the reset button
            if(Input.GetKeyDown(KeyCode.R))
            {
                CmdResetPosition(lastStopPos);
            }
        }

        if (isShooting)
            updateSlider();
    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<BallCamera>().target = transform;
        GetComponent<PreviewLine>().enabled = true;
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

    private void updateSlider()
    {
        if (slideUp)
            slider.value += 2;
        else
            slider.value -= 2;


        // Start moving the other way when we reach either end otherwise keep moving in the same direction
        slideUp = (slider.value >= maxSliderVal) ? false : (slider.value <= minSliderVal) ? true : slideUp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hole"))
        {
            if (isLocalPlayer)
            {
                CmdPlayerInHole();
                Debug.Log("GG WP");
            }
        }
    }

    void OnGUI()
    {
        if (isLocalPlayer)
            textShots.text = "Shots: " + shots.ToString();
    }

    IEnumerator ShowNotification(Text label, string message, float time)
    {
        label.text = message;
        label.enabled = true;
        yield return new WaitForSeconds(time);
        label.enabled = false;
    }

    #region Command

    [Command]
    private void CmdEnablePlayer()
    {
        canShoot = true;
        RpcEnablePlayer();
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
        playerManager.SetPlayerShots(this.connectionToClient.connectionId, shots);
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
        int type = 0;
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


        shots = 0;
        RpcDisablePlayerInHole(type);
        playerManager.SetPlayerDone(this.connectionToClient.connectionId);
    }


    [Command]
    private void CmdResetPosition(Vector3 lastPos)
    {
        transform.position = lastPos;
        rb.velocity = Vector3.zero;
        RpcForceUpdatePosition(lastPos);
    }
#endregion

    #region ClientRpc

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
            GetComponent<PreviewLine>().enabled = false;
            line.enabled = false;

            switch(type)
            {
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

            StartCoroutine(ShowNotification(notificationText, message, 1));
        }
    }

    [ClientRpc]
    private void RpcDisablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = true;
            GetComponent<PreviewLine>().enabled = false;
            line.enabled = false;
        }
    }

    [ClientRpc]
    private void RpcEnablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = false;
            GetComponent<PreviewLine>().enabled = false;
            line.enabled = false;
        }
    }

#endregion
}
