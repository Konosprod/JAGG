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
}
