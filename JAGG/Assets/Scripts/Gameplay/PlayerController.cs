using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {
    
    [SyncVar]
    bool canShoot = true;

    public int shots = 0;

    public int minSliderVal = 10;
    public int maxSliderVal = 150;

    private Slider slider;

    private LevelProperties levelProperties;

    private bool isShooting = false;
    private bool slideUp = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        slider = GameObject.Find("Slider").GetComponent<Slider>();

        slider.minValue = minSliderVal;
        slider.maxValue = maxSliderVal;

        levelProperties = GameObject.FindObjectOfType<LevelProperties>();

    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb.velocity.magnitude < 0.001f)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;

            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                if (isShooting)
                {
                    Shoot(dir, rb);
                    isShooting = false;
                    slideUp = true;
                    slider.value = minSliderVal;
                }
                else
                    isShooting = true;
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

    public void Shoot(Vector3 dir, Rigidbody rb)
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
        Debug.Log("Here");
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
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Hole"))
        {
            if (isLocalPlayer)
            {
                CmdDisablePlayer();
                Debug.Log("GG WP");
            }
        }
    }
}
