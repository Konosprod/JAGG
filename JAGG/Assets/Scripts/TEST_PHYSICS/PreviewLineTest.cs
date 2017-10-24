using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewLineTest : MonoBehaviour
{

    public Rigidbody rb;
    public LineRenderer line;

    // Use this for initialization
    void Start()
    {
        if (rb == null)
            Debug.Log("PAS DE RIGIDBODY WTF BRO");
        if (line == null)
            Debug.Log("ELLE EST OU LA LINE ???");
    }

    // Update is called once per frame
    void Update()
    {
        if (rb != null && line != null)
        {
            if (rb.velocity.magnitude < 0.001f)
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
        }
    }
}
