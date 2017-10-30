using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LevelProperties : NetworkBehaviour {

    public Transform NextLevel;
    public int par;
    public int maxShot;
    public float maxTime;
}
