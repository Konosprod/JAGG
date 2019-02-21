using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindArea : CustomScript {

    [CustomProp]
    public float strength;

    public void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        //Debug.Log("Wind area trigger : " + go.name);
        if(go.CompareTag("Player"))
        {
            PlayerController controller = go.GetComponent<PlayerController>();

            if (controller != null)
                go.GetComponent<PlayerController>().InWindArea(strength, transform.up);
            else
                go.GetComponent<OfflineBallController>().InWindArea(strength, transform.up);
        }
    }
}
