using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewLine : MonoBehaviour {

    public Rigidbody rb;
    public Camera cam;
    public LineRenderer line;

	// Use this for initialization
	void Start ()
    {
        if (rb == null)
            Debug.Log("PAS DE RIGIDBODY WTF BRO");
        if (cam == null)
            Debug.Log("CAMERA STP");
        if (line == null)
            Debug.Log("ELLE EST OU LA LINE ???");
    }
	
	// Update is called once per frame
	void Update () {
		if (rb != null && cam != null && line != null)
        {
            if (Mathf.Approximately(rb.velocity.magnitude, 0f))
            {
                if (!line.enabled)
                    line.enabled = true;

                Vector3 dir = transform.position - cam.transform.position;
                dir = new Vector3(dir.x, 0f, dir.z).normalized;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, dir / 1.3f + transform.position);
            }
            else
            {
                line.enabled = false;
            }
        }
	}
}
