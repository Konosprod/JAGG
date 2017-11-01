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

    private bool isShooting = false;
    private bool slideUp = true;

    private PlayerManager playerManager;
    private bool isOver = false;

    private Vector3 serverPos = Vector3.zero;

    private Queue<Vector3> serverPositions = new Queue<Vector3>();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();

        slider = GameObject.Find("Slider").GetComponent<Slider>();
        textShots = GameObject.Find("PlayerShots").GetComponent<Text>();
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
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;

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

        if (!isMoving)
        {
            if (!line.enabled)
                line.enabled = true;

            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, dir / 1.3f + transform.position);
        }
        else
        {
            line.enabled = false;
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

    [Command]
    void CmdShoot(Vector3 dir, float sliderVal)
    {
        rb.AddForce(dir * sliderVal * 10f);
        shots++;
        playerManager.SetPlayerShots(this.connectionToClient.connectionId, shots);
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

    [Command]
    private void CmdDisablePlayer()
    {
        canShoot = false;
        RpcDisablePlayer();
    }

    [Command]
    private void CmdPlayerInHole()
    {
        playerManager.SetPlayerDone(this.connectionToClient.connectionId);
        shots = 0;
        canShoot = false;
        RpcDisablePlayer();
    }

    [ClientRpc]
    private void RpcDisablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = true;
            GetComponent<PreviewLine>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            rb.velocity = Vector3.zero;
        }
    }

    [ClientRpc]
    private void RpcEnablePlayer()
    {
        if (isLocalPlayer)
        {
            isOver = false;
            GetComponent<PreviewLine>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            rb.velocity = Vector3.zero;
        }
    }

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

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Hole"))
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
        if(isLocalPlayer)
            textShots.text = "Shots: " + shots.ToString();
    }
}
