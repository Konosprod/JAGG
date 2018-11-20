using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    public static ReplayManager _instance;

    private List<ReplayObject> replayObjects = new List<ReplayObject>();
    private bool isReplayActive = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void AddReplayObject(ReplayObject ro)
    {
        replayObjects.Add(ro);
    }
}
