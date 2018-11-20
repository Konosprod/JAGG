using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayObject : MonoBehaviour {

    public struct InputInfo
    {
        public int frame;
        public Vector3 dir;
        public float sliderValue;
        public Transform pos;

        public InputInfo(int f, Vector3 d, float sv, Transform t)
        {
            frame = f;
            dir = d;
            sliderValue = sv;
            pos = t;
        }
    }

    public Dictionary<int, Transform> frames = new Dictionary<int, Transform>();
    public List<InputInfo> inputs = new List<InputInfo>();
        

	// Use this for initialization
	void Start () {
        if(ReplayManager._instance != null)
        {
            ReplayManager._instance.AddReplayObject(this);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddFrame(Transform t)
    {
        frames.Add(Time.frameCount, t);
    }

    public void AddInput(Vector3 dir, float sliderValue, Transform pos)
    {
        inputs.Add(new InputInfo(Time.frameCount, dir, sliderValue, pos));
    }

    // Gives a list with the frames included between the frameStart and frameEnd (start of the shot to the hole for instance)
    public List<Transform> GetFramesInWindow(int frameStart, int frameEnd)
    {
        if(frameEnd < frameStart)
        {
            Debug.LogError("U wot m8 ?");
        }

        List<Transform> framesRes = new List<Transform>();

        for(int i=frameStart; i<=frameEnd; i++)
        {
            if (frames.ContainsKey(i))
                framesRes.Add(frames[i]);
            else
                Debug.LogError("Missing frame : " + i + " for object : " + gameObject.name);
        }

        return framesRes;
    }
}
