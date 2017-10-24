using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour {

    public Rigidbody rb;

    public float minForce = 100f;
    public float force = 400f;
    public float maxForce = 1500f;

	// Use this for initialization
	void Start () {
        if (rb == null)
            Debug.Log("PAS DE RIGIDBODY WTF BRO");

        if (Camera.main == null)
            Debug.Log("shit");
	}
	
	// Update is called once per frame
	void Update () {
        if (rb != null)
        {
            if (rb.velocity.magnitude < 0.001f)
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
	}
}
