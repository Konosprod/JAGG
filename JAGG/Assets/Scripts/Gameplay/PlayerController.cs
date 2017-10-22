using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerController : NetworkBehaviour {

    public float force;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (Mathf.Approximately(rb.velocity.magnitude, 0f))
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;
            if (Input.GetMouseButtonDown(0))
            {
                rb.AddForce(dir * force);
                Debug.Log("dir = " + dir.ToString() + ", ball pos = " + transform.position.ToString() + ", cam pos = " + Camera.main.transform.position.ToString());
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<BallCamera>().target = GetComponent<Transform>();
        GetComponent<PreviewLine>().enabled = true;
    }

}
