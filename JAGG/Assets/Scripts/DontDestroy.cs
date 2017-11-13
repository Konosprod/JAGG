using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour {

    [Tooltip("Enable or disable destroying the object on scene transition")]
    public bool DestroyOnLoad = false;

    void Awake()
    {
        if(!DestroyOnLoad)
            DontDestroyOnLoad(transform.gameObject);
    }
}
