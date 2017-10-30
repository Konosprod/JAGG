using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

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

    private LevelProperties levelProperties;

    private bool isShooting = false;
    private bool slideUp = true;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();

        slider = GameObject.Find("Slider").GetComponent<Slider>();

        slider.minValue = minSliderVal;
        slider.maxValue = maxSliderVal;

        levelProperties = GameObject.FindObjectOfType<LevelProperties>();

    }


    private void Update()
    {
        if(isServer)
        {
            RpcUpdatePosition(transform.position);
            isMoving = rb.velocity.magnitude >= 0.001f;
        }

        if (!isLocalPlayer)
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


    [ClientRpc]
    void RpcUpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    [Command]
    void CmdShoot(Vector3 dir, float sliderVal)
    {
        rb.AddForce(dir * sliderVal * 10f);
        shots++;
    }

    /*public void Shoot(Vector3 dir)
    {
        //rb.AddForce(dir * force);

        rb.AddForce(dir * slider.value * 10f);

        //Debug.Log("dir = " + dir.ToString() + ", ball pos = " + transform.position.ToString() + ", cam pos = " + Camera.main.transform.position.ToString());

        shots++;

        Debug.Log(levelProperties.maxShot);

        if(shots >= levelProperties.maxShot)
        {
            CmdDisablePlayer();
        }
    }*/


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
        //PlayerStatus ps = LobbyManager.Instance.players.Find(p => p.connectionId == this.connectionToClient.connectionId);
        //Debug.Log(ps.connectionId);
        canShoot = false;
        RpcDisablePlayer();
    }

    [ClientRpc]
    private void RpcDisablePlayer()
    {
        if (isLocalPlayer)
        {
            GetComponent<PreviewLine>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            rb.velocity = Vector3.zero;
        }
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
}
