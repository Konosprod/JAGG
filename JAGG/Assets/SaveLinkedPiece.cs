using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLinkedPiece : CustomScript {
    
    public GameObject spawnPoint;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (spawnPoint != null)
        {
            // y + 0.2f is used to keep visible above ground
            if ((transform.position.x != spawnPoint.transform.position.x) || (transform.position.z != spawnPoint.transform.position.z) || (transform.position.y + 0.2f  != spawnPoint.transform.position.y))
            {
                spawnPoint.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);
            }
        }
    }
}
