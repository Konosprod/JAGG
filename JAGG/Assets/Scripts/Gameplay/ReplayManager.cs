using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public enum HighlightType
    {
        HoleInOne,                  // Self-explanatory
        LongestShotOnTarget,        // Same
        DenyShotOnTarget,           // A shot where you deny another player from getting into the hole AND you get into the hole
        FriendshipShotOnTarget,     // A shot where you get another player into the hole AND yourself at the same time
        DenyShot,                   // A shot where you deny another player from getting into the hole
        FriendshipShot,             // A shot where you get another player into the hole




        LAST_HIGHLIGHT_TYPE_PLUS_ONE    // YES (for static array)
    }


    [System.Serializable]
    public class Highlight : ISerializable
    {
        public int replayObjectIndex;
        public int replayHoleIndex;
        public int replayInputIndex;
        public HighlightType highlightType;


        public Highlight(int roi, int rhi, int rii, HighlightType ht)
        {
            replayObjectIndex = roi;
            replayHoleIndex = rhi;
            replayInputIndex = rii;
            highlightType = ht;
        }

        protected Highlight(SerializationInfo info, StreamingContext context)
        {
            replayObjectIndex = info.GetInt32("roIndex");
            replayHoleIndex = info.GetInt32("rhIndex");
            replayInputIndex = info.GetInt32("riIndex");
            highlightType = (HighlightType)info.GetValue("highlightType", typeof(HighlightType));
        }

        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("roIndex", replayObjectIndex);
            info.AddValue("rhIndex", replayHoleIndex);
            info.AddValue("riIndex", replayInputIndex);
            info.AddValue("highlightType", highlightType);
        }
    }



    [System.Serializable]
    private class Replay
    {
        public string customMapPath;
        public List<ReplayObject> replayObjects;
        public List<Highlight>[] highlights = new List<Highlight>[(int)HighlightType.LAST_HIGHLIGHT_TYPE_PLUS_ONE];

        public Replay()
        {
            customMapPath = "";
            replayObjects = new List<ReplayObject>();
            for(int i=0; i<(int)HighlightType.LAST_HIGHLIGHT_TYPE_PLUS_ONE; i++)
            {
                highlights[i] = new List<Highlight>();
            }
        }

        // Serialization implementation
        protected Replay(SerializationInfo info, StreamingContext context)
        {
            customMapPath = info.GetString("customMap");
            replayObjects = (List<ReplayObject>)info.GetValue("replayObjects", typeof(List<ReplayObject>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("customMap", customMapPath);
            info.AddValue("replayObjects", replayObjects);
        }
    }


    public static ReplayManager _instance;

    private Replay replay;

    //public List<ReplayObject> replayObjects = new List<ReplayObject>();
    public bool isReplayActive = false;
    public bool isReplayPlaying = false;

    private int replayFrame = 0;
    public long fixedFrameCount = 0;

    public bool isGameplayStarted = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void Start()
    {
        replay = new Replay();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (isReplayActive)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayReplay();
                LevelEditorMovingPieceManager._instance.ResetAllMVPs();
                LevelEditorMovingPieceManager._instance.ResetAllRTPs();
                
                Debug.Log("Replay start at frame : " + fixedFrameCount);
            }
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            TestSerialization();
            SaveInFile(@"C:\Users\Public\TestFolder\TestReplay.rpl");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadFromFile(@"C:\Users\Public\TestFolder\TestReplay.rpl");
            PlayReplay();
            LevelEditorMovingPieceManager._instance.ResetAllMVPs();
            LevelEditorMovingPieceManager._instance.ResetAllRTPs();

            Debug.Log("Replay start at frame : " + fixedFrameCount);
        }*/
    }

    void FixedUpdate()
    {
        /*if (isReplayPlaying)
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
                        ro.transform.position = inp.pos;
                        ro.physics.velocityCapped = Vector3.zero;
                    }
                    //else if (inp.sliderValue == -3f)
                    //{
                    //    // -3f is RotatePiece must start coroutine
                    //    Debug.Log("Replay rotation at : " + fixedFrameCount);
                    //    ro.rtp.isRotation = true;
                    //    ro.rtp.coroutine = StartCoroutine(ro.rtp.RotateMe(Vector3.up * ro.rtp.rotationAngle, ro.rtp.spinTime));
                    //}
                    //else if (inp.sliderValue == -4f)
                    //{
                    //    // -4f is MovingPiece must start coroutine (forwardMove true)
                    //    Debug.Log("Replay moving forward at : " + fixedFrameCount);
                    //    ro.mvp.isMoving = true;
                    //    ro.mvp.coroutine = StartCoroutine(ro.mvp.MoveMe(ro.mvp.initPos, ro.mvp.destPos, ro.mvp.travelTime));
                    //}
                    //else if (inp.sliderValue == -5f)
                    //{
                    //    // -5f is MovingPiece must start coroutine (forwardMove false)
                    //    Debug.Log("Replay moving backward at : " + fixedFrameCount);
                    //    ro.mvp.isMoving = true;
                    //    ro.mvp.coroutine = StartCoroutine(ro.mvp.MoveMe(ro.mvp.destPos, ro.mvp.initPos, ro.mvp.travelTime));
                    //}

                    ro.inputs.RemoveAt(0);
                }

            }

            replayFrame++;
        }*/

        if (isGameplayStarted)
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


    public void StartGameplay(bool start, string customMap = "", bool startOfGame = false)
    {
        if (startOfGame)
        {
            replay = new Replay();
            replay.customMapPath = customMap;
        }
        isGameplayStarted = start;
        fixedFrameCount = 0;
    }

    public void ResetReplayObjects()
    {
        Debug.Log("ResetReplayObjects");
        replay.replayObjects.Clear();
    }

    public void AddReplayObject(ReplayObject ro)
    {
        //Debug.Log("AddReplayObject : " + ro.name + " " + ro.steamName);
        replay.replayObjects.Add(ro);
    }

    public void AddHighlight(HighlightType ht, ReplayObject ro)
    {
        switch(ht)
        {
            case HighlightType.HoleInOne:
                {
                    Debug.Log(ro.name + " " + ro.steamName);
                    Debug.Log(replay.replayObjects.Count);
                    Debug.Log(replay.replayObjects[0].name + " " + replay.replayObjects[0].steamName);
                    Debug.Log("Add highlight : " + ht.ToString() + ", roi : " + replay.replayObjects.IndexOf(ro) + ", rhi : " + ro.currentHole + ", rii : " +  0);
                    replay.highlights[(int)HighlightType.HoleInOne].Add(new Highlight(replay.replayObjects.IndexOf(ro), ro.currentHole, 0, ht));
                    break;
                }
            default:
                {

                    break;
                }
        }
    }

    public void SaveInFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "replays");

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        Debug.Log(filePath);
        filePath += "/" + replay.customMapPath + "_" + System.DateTime.Now.ToString("ddMMyyyyHHmm");

        using (Stream stream = File.Open(filePath, FileMode.Create))
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                // Serialize the InputInfos into the stream
                formatter.Serialize(stream, replay);
            }
            catch (SerializationException e)
            {
                Debug.Log("Serialization failed : " + e.Message);
                throw;
            }
        }
    }

    public void LoadFromFile(string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                // Deserialize the InputInfos from the stream
                Replay rep = (Replay)formatter.Deserialize(stream);

                replay = rep;

                // Verify that it all worked.
                Debug.Log("CustomMapPath : " + rep.customMapPath);
                foreach (ReplayObject ro in rep.replayObjects)
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
