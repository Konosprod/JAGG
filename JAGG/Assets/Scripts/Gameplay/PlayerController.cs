using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour {

    public float force;
    public bool canShoot = true;
    public int shots = 0;

    //public Slider slider;

    private LevelProperties levelProperties;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        //slider = GameObject.Find("Slider").GetComponent<Slider>();

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
                Shoot(dir, rb);
            }
        }

    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<BallCamera>().target = GetComponent<Transform>();
        GetComponent<PreviewLine>().enabled = true;
    }

    public void Shoot(Vector3 dir, Rigidbody rb)
    {
        rb.AddForce(dir * force);
        //Debug.Log("dir = " + dir.ToString() + ", ball pos = " + transform.position.ToString() + ", cam pos = " + Camera.main.transform.position.ToString());

        shots++;

        Debug.Log(levelProperties.maxShot);

        if(shots >= levelProperties.maxShot)
        {
            //if(hasAuthority)
                CmdDisablePlayer();
        }
    }

    [Command]
    private void CmdDisablePlayer()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Here");
            canShoot = false;
            GetComponent<PreviewLine>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
