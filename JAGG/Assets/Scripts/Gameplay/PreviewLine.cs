using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PreviewLine : NetworkBehaviour
{
    public PlayerController pc;
    public LineRenderer line;

    public float lineLength = 0.85f;

    // Use this for initialization
    void Start()
    {

    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (pc != null && line != null)
        {
            if (!pc.isMoving)
            {
                if (!line.enabled)
                    line.enabled = true;

                Vector3 dir = transform.position - Camera.main.transform.position;
                dir = new Vector3(dir.x, 0f, dir.z).normalized;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, dir * lineLength + transform.position);
            }
            else
            {
                line.enabled = false;
            }
        }
    }

    public void SetEnabled(bool enable)
    {
        line.enabled = enable;
    }
}
