using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour {

    public Rigidbody rb;
    public Camera cam;

    public float minForce = 100f;
    public float force = 400f;
    public float maxForce = 1500f;

	// Use this for initialization
	void Start () {
        if (rb == null)
            Debug.Log("PAS DE RIGIDBODY WTF BRO");
        if (cam == null)
            Debug.Log("CAMERA STP");
	}
	
	// Update is called once per frame
	void Update () {
        if (cam != null && rb != null)
        {
            if (Mathf.Approximately(rb.velocity.magnitude,0f))
            {
                Vector3 dir = transform.position - cam.transform.position;
                dir = new Vector3(dir.x, 0f, dir.z).normalized;
                if (Input.GetMouseButtonDown(0))
                {
                    rb.AddForce(dir * force);
                    Debug.Log("dir = " + dir.ToString() + ", ball pos = " + transform.position.ToString() + ", cam pos = " + cam.transform.position.ToString());
                }
            }
        }
	}
}
