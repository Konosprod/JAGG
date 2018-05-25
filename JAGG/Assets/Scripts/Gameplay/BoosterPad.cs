using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoosterPad : CustomScript {

    [CustomProp]
    [Tooltip("Formula is newVel = oldVel * multFactor + addFactor")]
    public float multFactor = 2.0f;

    [CustomProp]
    [Tooltip("We use addForce for this part, for reference 1500 is the maximum shooting force (currently)")]
    public float addFactor = 1500.0f;

    public void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;

        if(go.CompareTag("Player"))
        {
            Vector3 dir = transform.forward.normalized;
            PlayerController controller = go.GetComponent<PlayerController>();

            //If we are online
            if (controller != null)
                go.GetComponent<PlayerController>().OnBoosterPad(dir, multFactor, addFactor);
            else
                go.GetComponent<OfflineBallController>().OnBoosterPad(dir, multFactor, addFactor);
        }
    }
}
