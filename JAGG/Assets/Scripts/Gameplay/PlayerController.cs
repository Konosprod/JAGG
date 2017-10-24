using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

    public float force;
    public bool canShoot = true;
    public int shots = 0;
    public Text timerText;

    //public Slider slider;

    private float timer = 0f;
    private bool isStarted = false;
    LevelProperties levelProperties;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;


        //Find better way to do this
        levelProperties = GameObject.Find("Level Properties").GetComponent<LevelProperties>();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();

        //slider = GameObject.Find("Slider").GetComponent<Slider>();

        this.timer = levelProperties.maxTime;
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        //Debug.Log("Started : " + isStarted.ToString() + " Timer : " + timer.ToString());

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb.velocity.magnitude < 0.001f)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;

            if (Input.GetMouseButtonDown(0) && canShoot)
            {
                Shoot(dir, rb);
            }
        }

        if (isStarted)
        {
            timer -= Time.deltaTime;

            if (timer > 0)
            {
                timerText.text = timer.ToString("Time: 0 s");
            }
            else
            {
                //send timer finished to the server

                DisablePlayer();
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<BallCamera>().target = GetComponent<Transform>();
        GetComponent<PreviewLine>().enabled = true;

        isStarted = true;

    }

    public void Shoot(Vector3 dir, Rigidbody rb)
    {
        rb.AddForce(dir * force);
        //Debug.Log("dir = " + dir.ToString() + ", ball pos = " + transform.position.ToString() + ", cam pos = " + Camera.main.transform.position.ToString());

        shots++;

        if(shots >= levelProperties.maxShot)
        {
            //send max shot reached

            DisablePlayer();
        }
    }

    private void DisablePlayer()
    {
        canShoot = false;
        GetComponent<PreviewLine>().enabled = false;
        GetComponent<LineRenderer>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
