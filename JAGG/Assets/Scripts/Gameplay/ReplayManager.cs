using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    public static ReplayManager _instance;

    public List<ReplayObject> replayObjects = new List<ReplayObject>();
    public bool isReplayActive = false;
    public bool isReplayPlaying = false;

    private int replayFrame = 0;

    public long fixedFrameCount = 0;

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
        if (isReplayActive)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayReplay();
                LevelEditorMovingPieceManager._instance.ResetAllMVPs();
                LevelEditorMovingPieceManager._instance.ResetAllRTPs();

                replayObjects[0].TestSerialize();
                Debug.Log("Replay start at frame : " + fixedFrameCount);
            }
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            TestSerialization();
        }
    }

    void FixedUpdate()
    {
        if (isReplayPlaying)
        {
            foreach (ReplayObject ro in replayObjects)
            {
                if (ro.inputs.Count == 0)
                    continue;

                ReplayObject.InputInfo inp = ro.inputs[0];

                if (replayFrame == inp.frame)
                {
                    // Play the input
                    if (inp.sliderValue > 0f)
                    {
                        Debug.Log("Current frame : " + fixedFrameCount + ", input : " + inp + ", current position : " + ro.gameObject.transform.position.ToString("F6"));

                        if (inp.sliderValue != -2f)
                        {
                            ro.physics.AddForce(inp.dir * Mathf.Pow(inp.sliderValue, 1.4f) * 2f);
                        }
                    }
                    else if (inp.sliderValue == -1f)
                    {
                        // -1f is when the ball gets in the hole
                        StopReplay();
                    }
                    else if (inp.sliderValue == -2f)
                    {
                        // -2f is when the ball is reset to its former position
                        ro.transform.position = ro.inputs[0].pos;
                        ro.physics.velocityCapped = Vector3.zero;
                    }
                    else if (inp.sliderValue == -3f)
                    {
                        // -3f is RotatePiece must start coroutine
                        Debug.Log("Replay rotation at : " + fixedFrameCount);
                        ro.rtp.isRotation = true;
                        ro.rtp.coroutine = StartCoroutine(ro.rtp.RotateMe(Vector3.up * ro.rtp.rotationAngle, ro.rtp.spinTime));
                    }
                    else if (inp.sliderValue == -4f)
                    {
                        // -4f is MovingPiece must start coroutine (forwardMove true)
                        Debug.Log("Replay moving forward at : " + fixedFrameCount);
                        ro.mvp.isMoving = true;
                        ro.mvp.coroutine = StartCoroutine(ro.mvp.MoveMe(ro.mvp.initPos, ro.mvp.destPos, ro.mvp.travelTime));
                    }
                    else if (inp.sliderValue == -5f)
                    {
                        // -5f is MovingPiece must start coroutine (forwardMove false)
                        Debug.Log("Replay moving backward at : " + fixedFrameCount);
                        ro.mvp.isMoving = true;
                        ro.mvp.coroutine = StartCoroutine(ro.mvp.MoveMe(ro.mvp.destPos, ro.mvp.initPos, ro.mvp.travelTime));
                    }

                    ro.inputs.RemoveAt(0);
                }

            }

            replayFrame++;
        }

        fixedFrameCount++;
    }

    public void StartReplay()
    {
        isReplayActive = true;
        replayFrame = 0;
    }

    public void PlayReplay()
    {
        isReplayPlaying = true;
    }

    public void PauseReplay() // Not easy to really do correctly, NOT working atm (it will prevent the replay from triggering the next input but will not pause the ball movement)
    {
        isReplayPlaying = false;
    }

    public void StopReplay()
    {
        isReplayPlaying = false;
        isReplayActive = false;
        replayFrame = 0;
    }


    public void AddReplayObject(ReplayObject ro)
    {
        replayObjects.Add(ro);
    }


    // Serialization test placeholder for future file saving
    public void TestSerialization()
    {
        IFormatter formatter = new BinaryFormatter();

        // Create a MemoryStream that the object will be serialized into and deserialized from.
        using (Stream stream = new MemoryStream())
        {
            SurrogateSelector ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(ReplayObject),
            new StreamingContext(StreamingContextStates.All),
            new ReplayObject.ReplayObjectSerializationSurrogate());
            // Associate the SurrogateSelector with the BinaryFormatter.
            formatter.SurrogateSelector = ss;

            try
            {
                // Serialize the InputInfos into the stream
                formatter.Serialize(stream, replayObjects);
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
                List<ReplayObject> replayObjs = (List<ReplayObject>)formatter.Deserialize(stream);

                // Verify that it all worked.
                foreach (ReplayObject ro in replayObjs)
                    Debug.Log(ro);
            }
            catch (SerializationException e)
            {
                Debug.Log("Deserialization failed : " + e.Message);
                throw;
            }
        }
    }
}
