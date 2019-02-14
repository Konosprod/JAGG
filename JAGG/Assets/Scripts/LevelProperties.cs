using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


#pragma warning disable CS0618 // Le type ou le membre est obsolète

public class LevelProperties : NetworkBehaviour {

    public Transform nextSpawnPoint;
    public int par;
    public int maxShot;
    public float maxTime;
}
