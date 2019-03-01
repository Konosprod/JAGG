using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class ReplayObject : MonoBehaviour, ISerializable
{

    [System.Serializable]
    public class InputInfo : ISerializable
    {
        public long frame;
        public Vector3 dir;
        public float sliderValue;
        public Vector3 pos;

        public InputInfo(long f, Vector3 d, float sv, Vector3 t)
        {
            frame = f;
            dir = d;
            sliderValue = sv;
            pos = t;
        }

        protected InputInfo(SerializationInfo info, StreamingContext context)
        {
            frame = info.GetInt64("frame");
            dir = new Vector3(float.Parse(info.GetString("dirX")), float.Parse(info.GetString("dirY")), float.Parse(info.GetString("dirZ")));
            sliderValue = float.Parse(info.GetString("sliderValue"));
            pos = new Vector3(float.Parse(info.GetString("posX")), float.Parse(info.GetString("posY")), float.Parse(info.GetString("posZ")));
        }

        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("frame", frame);
            info.AddValue("dirX", dir.x);
            info.AddValue("dirY", dir.y);
            info.AddValue("dirZ", dir.z);
            info.AddValue("sliderValue", sliderValue);
            info.AddValue("posX", pos.x);
            info.AddValue("posY", pos.y);
            info.AddValue("posZ", pos.z);
        }

        public override string ToString()
        {
            return "Frame : " + frame + ", dir : " + dir.ToString("F6") + ", sliderValue : " + sliderValue + ", position : " + pos.ToString("F6");
        }
    }


    public List<InputInfo>[] inputs;
    public Rigidbody rb;
    //public RotatePiece rtp;
    //public MovingPiece mvp;
    public BallPhysicsNetwork physics;

    private string goName;
    public int currentHole = -1;
    public string steamName;
    public Color trailColor;
    public int[] scores;
    public float[] times;

    // Use this for initialization
    void Awake()
    {
        inputs = new List<InputInfo>[18];
        rb = gameObject.GetComponent<Rigidbody>();
        //rtp = gameObject.GetComponent<RotatePiece>();
        //mvp = gameObject.GetComponent<MovingPiece>();
        physics = GetComponent<BallPhysicsNetwork>();
        scores = new int[18];
        times = new float[18];
        if (ReplayManager._instance != null)
        {
            ReplayManager._instance.AddReplayObject(this);
        }
        goName = gameObject.name;
        //Debug.Log("ReplayObject start : " + gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {

    }
       

    public void ResetInputs()
    {
        foreach (List<InputInfo> inps in inputs)
        {
            if(inps != null)
                inps.Clear();
        }
    }

    public void SetupReplay()
    {
        ResetInputs();
        StartHoleReplay(0);
    }

    public void StartHoleReplay(int hole)
    {
        currentHole = hole;
        inputs[currentHole] = new List<InputInfo>();
    }

    public void SetScoreTime(int hole, int score, float time)
    {
        scores[hole] = score;
        times[hole] = time;
    }

    public void AddInput(Vector3 dir, float sliderValue, Vector3 pos)
    {
        Debug.Log("Add input : frame=" + (ReplayManager._instance != null ? ReplayManager._instance.fixedFrameCount : Time.frameCount) + ", dir=" + dir + ", sliderValue=" + sliderValue + ", pos=" + pos);
        inputs[currentHole].Add(new InputInfo(ReplayManager._instance != null ? ReplayManager._instance.fixedFrameCount : Time.frameCount, dir, sliderValue, pos));
    }

         
    // Serialization implementation
    protected ReplayObject(SerializationInfo info, StreamingContext context)
    {
        inputs = (List<InputInfo>[])info.GetValue("inputs", typeof(List<InputInfo>[]));
        goName = info.GetString("goName");
        //currentHole = info.GetInt32("currentHole");
        steamName = info.GetString("steamName");
        trailColor = new Color(float.Parse(info.GetString("r")), float.Parse(info.GetString("g")), float.Parse(info.GetString("b")) , float.Parse(info.GetString("a")));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("inputs", inputs);
        info.AddValue("goName", goName);
        info.AddValue("steamName", steamName);
        info.AddValue("r", trailColor.r);
        info.AddValue("g", trailColor.g);
        info.AddValue("b", trailColor.b);
        info.AddValue("a", trailColor.a);
    }

       
    public override string ToString()
    {
        string res = goName + " replay" + '\n';
        int k = 1;
        foreach(List<InputInfo> inps in inputs)
        {
            if (inps != null)
            {
                foreach (InputInfo i in inps)
                {
                    res += "Input n°" + k + " : " + i.ToString() + '\n';
                    k++;
                }
            }
        }
        return res;
    }
}
