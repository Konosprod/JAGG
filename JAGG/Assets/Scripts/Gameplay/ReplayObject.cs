using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ReplayObject : MonoBehaviour
{

    public struct InputInfo
    {
        public int frame;
        public Vector3 dir;
        public float sliderValue;
        public Vector3 pos;

        public InputInfo(int f, Vector3 d, float sv, Vector3 t)
        {
            frame = f;
            dir = d;
            sliderValue = sv;
            pos = t;
        }

        public override string ToString()
        {
            return "Frame : " + frame + ", dir : " + dir + ", sliderValue : " + sliderValue + ", position : " + pos;
        }
    }

    sealed class InputInfoSerializationSurrogate : ISerializationSurrogate // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.surrogateselector?redirectedfrom=MSDN&view=netframework-4.7.2
    {
        // Serialize the Employee object to save the object's name and address fields.
        public void GetObjectData(object obj,
            SerializationInfo info, StreamingContext context)
        {
            InputInfo inp = (InputInfo)obj;
            info.AddValue("frame", inp.frame);
            info.AddValue("dirX", inp.dir.x);
            info.AddValue("dirY", inp.dir.y);
            info.AddValue("dirZ", inp.dir.z);
            info.AddValue("sliderValue", inp.sliderValue);
            info.AddValue("posX", inp.pos.x);
            info.AddValue("posY", inp.pos.y);
            info.AddValue("posZ", inp.pos.z);
        }

        // Deserialize the Employee object to set the object's name and address fields.
        public object SetObjectData(object obj,
            SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            InputInfo inp = (InputInfo)obj;
            inp.frame = int.Parse(info.GetString("frame"));
            inp.dir = new Vector3(float.Parse(info.GetString("dirX")), float.Parse(info.GetString("dirY")), float.Parse(info.GetString("dirZ")));
            inp.sliderValue = float.Parse(info.GetString("sliderValue"));
            inp.pos = new Vector3(float.Parse(info.GetString("posX")), float.Parse(info.GetString("posY")), float.Parse(info.GetString("posZ")));
            return inp;
        }
    }

    public Dictionary<int, Transform> frames = new Dictionary<int, Transform>();
    public List<InputInfo> inputs = new List<InputInfo>();
    public Rigidbody rb;


    // Use this for initialization
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (ReplayManager._instance != null)
        {
            ReplayManager._instance.AddReplayObject(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void TestSerialize() // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter.serialize?redirectedfrom=MSDN&view=netframework-4.7.2#System_Runtime_Serialization_Formatters_Binary_BinaryFormatter_Serialize_System_IO_Stream_System_Object_
    {
        IFormatter formatter = new BinaryFormatter();

        // Create a MemoryStream that the object will be serialized into and deserialized from.
        using (Stream stream = new MemoryStream())
        {
            SurrogateSelector ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(InputInfo),
            new StreamingContext(StreamingContextStates.All),
            new InputInfoSerializationSurrogate());
            // Associate the SurrogateSelector with the BinaryFormatter.
            formatter.SurrogateSelector = ss;

            try
            {
                // Serialize the InputInfos into the stream
                formatter.Serialize(stream, inputs);
            }
            catch (SerializationException e)
            {
                Debug.Log("Serialization failed : " + e.Message);
                throw;
            }

            // Rewind the MemoryStream.
            stream.Position = 0;

            try
            {
                // Deserialize the InputInfos from the stream
                List<InputInfo> inp = (List<InputInfo>)formatter.Deserialize(stream);

                // Verify that it all worked.
                foreach (InputInfo i in inp)
                    Debug.Log(i);
            }
            catch (SerializationException e)
            {
                Debug.Log("Deserialization failed : " + e.Message);
                throw;
            }
        }
    }


    public void Reset()
    {
        frames.Clear();
        inputs.Clear();
    }

    public void AddFrame(Transform t)
    {
        frames.Add(Time.frameCount, t);
    }

    public void AddInput(Vector3 dir, float sliderValue, Vector3 pos)
    {
        Debug.Log("Add input : dir=" + dir + ", sliderValue=" + sliderValue + ", pos=" + pos);
        inputs.Add(new InputInfo(Time.frameCount, dir, sliderValue, pos));
    }

    // Gives a list with the frames included between the frameStart and frameEnd (start of the shot to the hole for instance)
    public List<Transform> GetFramesInWindow(int frameStart, int frameEnd)
    {
        if (frameEnd < frameStart)
        {
            Debug.LogError("U wot m8 ?");
        }

        List<Transform> framesRes = new List<Transform>();

        for (int i = frameStart; i <= frameEnd; i++)
        {
            if (frames.ContainsKey(i))
                framesRes.Add(frames[i]);
            else
                Debug.LogError("Missing frame : " + i + " for object : " + gameObject.name);
        }

        return framesRes;
    }
}
