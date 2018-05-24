using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindArea : CustomScript {

    [CustomProp]
    public float strength;

    public void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;

        if(go.CompareTag("Player"))
        {
            go.GetComponent<PlayerController>().InWindArea(strength, transform.up);
        }
    }
}
