using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ReplayManager : MonoBehaviour
{
    public enum HighlightType
    {
        HoleInOne,                  // Self-explanatory
        LongestShotOnTarget,        // Same
        DenyShotOnTarget,           // A shot where you deny another player from getting into the hole AND you get into the hole (Impossible as of now, technology isn't there yet)
        FriendshipShotOnTarget,     // A shot where you get another player into the hole AND yourself at the same time
        DenyShot,                   // A shot where you deny another player from getting into the hole (Impossible as of now, technology isn't there yet)
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

        public bool IsEqual(Highlight other)
        {
            return this.replayObjectIndex == other.replayObjectIndex && this.replayInputIndex == other.replayInputIndex && this.replayHoleIndex == other.replayHoleIndex;
        }

        public override string ToString()
        {
            return highlightType.ToString() + " for objectId : " + replayObjectIndex + ", on hole : " + replayHoleIndex + ", for input : " + replayInputIndex + '\n';
        }
    }



    [System.Serializable]
    private class Replay
    {
        public string customMapPath;
        public List<ReplayObject> replayObjects;
        public List<Highlight>[] highlights = new List<Highlight>[(int)HighlightType.LAST_HIGHLIGHT_TYPE_PLUS_ONE];
        public List<KeyValuePair<int, int>> selectedHighlights = new List<KeyValuePair<int, int>>();

        public Replay()
        {
            customMapPath = "";
            replayObjects = new List<ReplayObject>();
            for (int i = 0; i < (int)HighlightType.LAST_HIGHLIGHT_TYPE_PLUS_ONE; i++)
            {
                highlights[i] = new List<Highlight>();
            }
        }

        // Serialization implementation
        protected Replay(SerializationInfo info, StreamingContext context)
        {
            customMapPath = info.GetString("customMap");
            replayObjects = (List<ReplayObject>)info.GetValue("replayObjects", typeof(List<ReplayObject>));
            highlights = (List<Highlight>[])info.GetValue("highlights", typeof(List<Highlight>[]));
            selectedHighlights = (List<KeyValuePair<int, int>>)info.GetValue("selectedHighlights", typeof(List<KeyValuePair<int, int>>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("customMap", customMapPath);
            info.AddValue("replayObjects", replayObjects);
            info.AddValue("highlights", highlights);
            info.AddValue("selectedHighlights", selectedHighlights);
        }


        public List<string> GetPlayerNames()
        {
            List<string> playerNames = new List<string>();

            foreach (ReplayObject ro in replayObjects)
            {
                playerNames.Add(ro.steamName);
            }

            return playerNames;
        }

        public override string ToString()
        {
            string res = "Map : " + customMapPath + '\n' + "ReplayObjects : " + '\n';

            foreach (ReplayObject ro in replayObjects)
            {
                res += ro.ToString();
            }
            res += '\n' + "Highlights : " + '\n';

            foreach (List<Highlight> lh in highlights)
            {
                foreach (Highlight h in lh)
                {
                    res += h.ToString();
                }
            }

            res += '\n' + "Selected highlights : " + '\n';
            foreach (KeyValuePair<int, int> pair in selectedHighlights)
            {
                res += ((HighlightType)pair.Key).ToString() + " n°" + pair.Value + '\n';
            }

            return res;
        }
    }


    public static ReplayManager _instance;

    private Replay replay;

    [Header("Main menu UI")]
    public GameObject panelSelectReplay;
    public Transform contentUI;
    public GameObject prefabEntry;

    public ReplayBallUtility[] replayBalls = new ReplayBallUtility[4];

    //public List<ReplayObject> replayObjects = new List<ReplayObject>();
    public bool isReplayActive = false;
    public bool isReplayPlaying = false;

    //private int replayFrame = 0;
    public long fixedFrameCount = 0;

    public bool isGameplayStarted = false;

    public bool isHighlight = false;    // Are we currently playing a Highlight
    private bool isHighlightReady = false;  // Is the highlight ready to be played or are we simulating the physics still
    private bool isHighlightDone = false;   // Is the highlight done (the ball has reached the -1f input)
    private int nbPlayersReplay = 0;    // The number of players involved in the replay (1-4)
    private int currentHighlight = 0;   // The index of the highlight we are currently showing
    private Highlight currentHighlightObject = null;    // The highlight object we are currently showing
    public/*private TODO*/ long fixedHighlightFrames = 0;  // The frames for the replay

    public bool physicsDoUpdate = true;

    private List<Queue<ReplayObject.InputInfo>> allInputs = null; // All the inputs for the highlight (all players)
    private int currentPlayer = -1; // The player who has the next input to play
    private long nextFrame = long.MaxValue; // The frame of the next input
    private ReplayObject.InputInfo currentInputInfo = null; // The next input to play

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

        if (isHighlight)
        {
            // Setup the highlight
            if (!isHighlightReady)
            {
                Debug.Log("We have " + replay.selectedHighlights.Count + " highlights to show");
                foreach (KeyValuePair<int, int> _debugSelecHighlight in replay.selectedHighlights)
                {
                    Debug.Log(_debugSelecHighlight);
                }


                if (currentHighlightObject == null)
                {
                    currentHighlightObject = replay.highlights[replay.selectedHighlights[currentHighlight].Key][replay.selectedHighlights[currentHighlight].Value];


                    // Disable collisions between balls at the start
                    LobbyManager._instance.DisableAllBallsCollisions();

                    // Place the balls at the spawn position
                    Transform startPos = null;
                    Debug.Log("test.replayHoleIndex : " + currentHighlightObject.replayHoleIndex);
                    GameObject hole = GameObject.Find("Hole " + (currentHighlightObject.replayHoleIndex + 1).ToString());
                    foreach (Transform children in hole.transform)
                    {
                        if (children.name == "Spawn Point")
                        {
                            Debug.Log("Found the spawn point");
                            startPos = children;
                        }
                    }
                    foreach (ReplayBallUtility replayBall in replayBalls)
                    {
                        replayBall.MoveBall(startPos.position);
                    }

                    fixedHighlightFrames = 0;
                }

                List<ReplayObject.InputInfo> highlightedInputs = replay.replayObjects[currentHighlightObject.replayObjectIndex].inputs[currentHighlightObject.replayHoleIndex];

                // Inputs from all the players
                allInputs = new List<Queue<ReplayObject.InputInfo>>();
                // We queue all inputs that will happen before the highlighted play in order of frames
                foreach (ReplayObject ro in replay.replayObjects)
                {
                    List<ReplayObject.InputInfo> _inputInfos = ro.inputs[currentHighlightObject.replayHoleIndex];

                    // Sort the list by ascending order of frames
                    _inputInfos.Sort((x, y) => x.frame.CompareTo(y.frame));

                    /*foreach (ReplayObject.InputInfo _ii in _inputInfos)
                        Debug.Log(_ii);*/

                    // Remove all inputs that happen after the highlighted input (they will be handled later)
                    //_inputInfos.RemoveAll(x => x.frame > replay.replayObjects[currentHighlightObject.replayObjectIndex].inputs[currentHighlightObject.replayHoleIndex][currentHighlightObject.replayInputIndex].frame);

                    allInputs.Add(new Queue<ReplayObject.InputInfo>(_inputInfos));
                }

                int currentInputHighlight = 0;
                currentPlayer = -1;
                nextFrame = long.MaxValue;
                int i = 0;
                foreach (Queue<ReplayObject.InputInfo> inputs in allInputs)
                {
                    Debug.Log("Inputs for player " + i + " : ");
                    foreach (ReplayObject.InputInfo _inputDebug in inputs.ToArray())
                    {
                        Debug.Log(_inputDebug);
                    }


                    if (inputs.Peek().frame < nextFrame)
                    {
                        currentPlayer = i;
                        nextFrame = inputs.Peek().frame;
                    }
                    i++;
                }

                ReplayObject.InputInfo currentInputInfoHighlight = highlightedInputs[currentInputHighlight];
                currentInputInfo = allInputs[currentPlayer].Dequeue();

                //Debug.Log("Targeted input for player " + currentHighlightObject.replayObjectIndex + " : " + highlightedInputs[currentHighlightObject.replayInputIndex]);
                //Debug.Log("RII : " + currentHighlightObject.replayInputIndex + ", CIH : " + currentInputHighlight + ", FHF : " + fixedHighlightFrames + ", Target Frame : " + highlightedInputs[currentHighlightObject.replayInputIndex].frame);

                // We simulate at once all frames up to the highlighted input
                while (!isHighlightReady)
                {
                    // We are on the right input, start highlight when on the right frame
                    if (currentHighlightObject.replayInputIndex == currentInputHighlight && fixedHighlightFrames == highlightedInputs[currentHighlightObject.replayInputIndex].frame)
                    {
                        //Debug.Log("Highlight setup is done !");
                        isHighlightReady = true;

                        // Set the camera on the right replay ball
                        Camera.main.GetComponent<BallCamera>().target = replayBalls[currentHighlightObject.replayObjectIndex].transform;

                        physicsDoUpdate = true;
                    }
                    else // It's a later input that is highlighted so we need to get to the right position first
                    {
                        while (currentInputInfo.frame == fixedHighlightFrames) // while because we could have multiple inputs at the same frame (unlikely but not impossible)
                        {
                            //Debug.Log("Player " + currentPlayer + " simulating input : " + currentInputInfo);
                            if (currentInputInfo.sliderValue > 0f)
                            {
                                //Debug.Log("Current frame : " + fixedFrameCount + ", input : " + inp + ", current position : " + ro.gameObject.transform.position.ToString("F6"));
                                replayBalls[currentPlayer].physics.AddForce(currentInputInfo.dir * Mathf.Pow(currentInputInfo.sliderValue, 1.4f) * 2f);
                            }
                            else if (currentInputInfo.sliderValue == -1f)
                            {
                                // -1f is when the ball gets in the hole
                                replayBalls[currentPlayer].physics.StopBall();
                            }
                            else if (currentInputInfo.sliderValue == -2f)
                            {
                                // -2f is when the ball is reset to its former position
                                replayBalls[currentPlayer].transform.position = currentInputInfo.pos;
                                replayBalls[currentPlayer].physics.velocityCapped = Vector3.zero;
                            }
                            else if (currentInputInfo.sliderValue == -3f)
                            {
                                // -3f is when the collisions are enabled for the ball
                                for (int _enableLayer = PlayerController.FirstLayer; _enableLayer < PlayerController.FirstLayer + 4; _enableLayer++)
                                {
                                    Physics.IgnoreLayerCollision(replayBalls[currentPlayer].gameObject.layer, _enableLayer, false);
                                }
                            }

                            // Go to the next input
                            i = 0;
                            nextFrame = long.MaxValue;
                            foreach (Queue<ReplayObject.InputInfo> inputs in allInputs)
                            {
                                if (inputs.Count > 0 && inputs.Peek().frame < nextFrame)
                                {
                                    currentPlayer = i;
                                    nextFrame = inputs.Peek().frame;
                                }
                                i++;
                            }
                            currentInputInfo = allInputs[currentPlayer].Dequeue();
                            if (currentPlayer == currentHighlightObject.replayObjectIndex && currentHighlightObject.replayInputIndex != currentInputHighlight)
                            {
                                currentInputHighlight++;
                                currentInputInfoHighlight = highlightedInputs[currentInputHighlight];
                            }
                        }

                        // Advance the physics by one frame
                        for (int k = 0; k < nbPlayersReplay; k++)
                        {
                            replayBalls[k].physics.Step();
                        }
                        MovingPieceManager._instance.Step();

                        fixedHighlightFrames++;
                    }
                }


                //ReplayObject.InputInfo inputInfo = replay.replayObjects[currentHighlightObject.replayObjectIndex].inputs[currentHighlightObject.replayHoleIndex][currentHighlightObject.replayInputIndex];

            }
            else // Play the highlight
            {
                //Debug.Log("We can play the highlight ! YAY");
                //Debug.Log(currentInputInfo);


                while (currentInputInfo.frame == fixedHighlightFrames && !isHighlightDone) // while because we could have multiple inputs at the same frame (unlikely but not impossible)
                {
                    //Debug.Log("Player " + currentPlayer + " playing input : " + currentInputInfo);
                    if (currentInputInfo.sliderValue > 0f)
                    {
                        //Debug.Log("Current frame : " + fixedFrameCount + ", input : " + inp + ", current position : " + ro.gameObject.transform.position.ToString("F6"));
                        replayBalls[currentPlayer].physics.AddForce(currentInputInfo.dir * Mathf.Pow(currentInputInfo.sliderValue, 1.4f) * 2f);
                    }
                    else if (currentInputInfo.sliderValue == -1f)
                    {
                        // -1f is when the ball gets in the hole
                        replayBalls[currentPlayer].physics.StopBall();

                        if (currentPlayer == currentHighlightObject.replayObjectIndex)
                        {
                            Debug.Log("Highlight is over !");
                            isHighlightDone = true;
                        }
                    }
                    else if (currentInputInfo.sliderValue == -2f)
                    {
                        // -2f is when the ball is reset to its former position
                        replayBalls[currentPlayer].transform.position = currentInputInfo.pos;
                        replayBalls[currentPlayer].physics.velocityCapped = Vector3.zero;
                    }
                    else if (currentInputInfo.sliderValue == -3f)
                    {
                        // -3f is when the collisions are enabled for the ball
                        for (int _enableLayer = PlayerController.FirstLayer; _enableLayer < PlayerController.FirstLayer + 4; _enableLayer++)
                        {
                            Physics.IgnoreLayerCollision(replayBalls[currentPlayer].gameObject.layer, _enableLayer, false);
                        }
                    }

                    // Go to the next input
                    if (!isHighlightDone)
                    {
                        int i = 0;
                        nextFrame = long.MaxValue;
                        foreach (Queue<ReplayObject.InputInfo> inputs in allInputs)
                        {
                            if (inputs.Count > 0 && inputs.Peek().frame < nextFrame)
                            {
                                currentPlayer = i;
                                nextFrame = inputs.Peek().frame;
                            }
                            i++;
                        }
                        currentInputInfo = allInputs[currentPlayer].Dequeue();
                    }
                }


                MovingPieceManager._instance.Step();

                fixedHighlightFrames++;

                if (isHighlightDone)
                {
                    // Go to next highlight or to victory scene if highlights are over
                    currentHighlight++;
                    if (currentHighlight < replay.selectedHighlights.Count)
                    {
                        // More to go
                        currentHighlightObject = null;
                        isHighlightReady = false;
                        isHighlightDone = false;

                        physicsDoUpdate = false;

                        MovingPieceManager._instance.ReplayReset();
                    }
                    else
                    {
                        // We're done
                        ResetAfterHighlights();
                        LobbyManager._instance.AfterHighlightEndOfGame();
                    }
                }
            }
        }

        /*GameObject rplBall = replayBalls[0];
        rplBall.SetActive(true);

        // Put the ball at the spawn position of the highlight
        Highlight test = replay.highlights[replay.selectedHighlights[0].Key][replay.selectedHighlights[0].Value];
        Transform startPos = null;
        Debug.Log("test.replayHoleIndex : " + test.replayHoleIndex);
        GameObject hole = GameObject.Find("Hole " + (test.replayHoleIndex + 1).ToString());
        foreach (Transform children in hole.transform)
        {
            if (children.name == "Spawn Point")
            {
                Debug.Log("Found the spawn point");
                startPos = children;
            }
        }
        rplBall.transform.position = startPos.position;
        BallPhysics ballPhysics = rplBall.GetComponent<BallPhysics>();
        long currentHighlightFrames = 0;
        if (test.replayInputIndex > 0)
        {

        }
        else
        {
            ReplayObject.InputInfo inputInfo = replay.replayObjects[test.replayObjectIndex].inputs[test.replayHoleIndex][test.replayInputIndex];

            Debug.Log("Hole in one : " + (startPos.position == inputInfo.pos) + ", startPos : " + startPos.position.ToString("F6") + ", input info position : " + inputInfo.pos.ToString("F6"));
            rplBall.transform.position = inputInfo.pos;
            currentHighlightFrames = inputInfo.frame;
            //SceneManager.MoveGameObjectToScene(rplBall, SceneManager.GetActiveScene());
            Camera.main.GetComponent<BallCamera>().target = rplBall.transform;
            Debug.Log(inputInfo);
            ballPhysics.AddForce(Mathf.Pow(inputInfo.sliderValue, 1.4f) * inputInfo.dir * 2f);
        }*/




        if (isGameplayStarted)
            fixedFrameCount++;
    }

    // Things to do/clean after the highlights for the game are over
    private void ResetAfterHighlights()
    {
        isReplayActive = false;
        isHighlight = false;
        currentHighlightObject = null;
        isHighlightReady = false;
        isHighlightDone = false;
        physicsDoUpdate = true;

        // Disable the replay balls
        nbPlayersReplay = 0;
        foreach (ReplayObject ro in replay.replayObjects)
        {
            ReplayBallUtility replayBall = replayBalls[nbPlayersReplay];
            replayBall.SetActive(false);
            nbPlayersReplay++;
        }
    }

    public void StartReplay()
    {
        isReplayActive = true;
        //replayFrame = 0;
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
        //replayFrame = 0;
    }


    // Used at the start of the game to create the replay and start counting physics frames from 0
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

    // Adds the play as the highlight in the replay
    public void AddHighlight(HighlightType ht, ReplayObject ro)
    {
        switch (ht)
        {
            case HighlightType.HoleInOne:
                {
                    //Debug.Log("Add highlight : " + ht.ToString() + ", roi : " + replay.replayObjects.IndexOf(ro) + ", rhi : " + ro.currentHole + ", rii : " +  0);
                    replay.highlights[(int)HighlightType.HoleInOne].Add(new Highlight(replay.replayObjects.IndexOf(ro), ro.currentHole, 0, ht));
                    break;
                }
            case HighlightType.LongestShotOnTarget:
                {
                    //Debug.Log("Add highlight : " + ht.ToString() + ", roi : " + replay.replayObjects.IndexOf(ro) + ", rhi : " + ro.longestShotHoleIndex + ", rii : " + ro.longestShotInputIndex);
                    replay.highlights[(int)HighlightType.LongestShotOnTarget].Add(new Highlight(replay.replayObjects.IndexOf(ro), ro.longestShotHoleIndex, ro.longestShotInputIndex, ht));
                    break;
                }
            default:
                {
                    break;
                }
        }
    }


    // Setup for all things required for the highlights and we start showing them
    public void StartHighlights()
    {
        Debug.Log("StartHighlights");

        nbPlayersReplay = 0;
        foreach (ReplayObject ro in replay.replayObjects)
        {
            ReplayBallUtility replayBall = replayBalls[nbPlayersReplay];
            replayBall.SetActive(true);
            replayBall.SetReplayBallParams(ro.steamName, ro.trailColor);
            nbPlayersReplay++;
        }

        isReplayActive = true;
        isHighlight = true;
        isHighlightReady = false;
        currentHighlight = 0;

        physicsDoUpdate = false;

        MovingPieceManager._instance.ReplayReset();
    }

    // This is where we decide which highlights will be shown
    public void SelectHighlights()
    {
        for (int ht = 0; ht < (int)HighlightType.LAST_HIGHLIGHT_TYPE_PLUS_ONE; ht++)
        {
            List<Highlight> highlights = replay.highlights[ht];
            for (int i = 0; i < highlights.Count; i++)
            {
                // Don't add the same highlight twice for two different categories
                bool found = false;

                foreach (KeyValuePair<int, int> pair in replay.selectedHighlights)
                {
                    if (highlights[i].IsEqual(replay.highlights[pair.Key][pair.Value]))
                        found = true;
                }

                if (!found)
                    replay.selectedHighlights.Add(new KeyValuePair<int, int>(ht, i));
            }
        }
    }

    // Get the replay as byte[] to send over the network
    public byte[] GetReplayData()
    {
        byte[] data;
        IFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, replay);
            data = stream.ToArray();
        }
        return data;
    }

    // Get the replay from the byte[] we received from the server
    public void GetReplayFromData(byte[] data, int replaySize)
    {
        IFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream(data, 0, replaySize))
        {
            replay = (Replay)formatter.Deserialize(stream);
            Debug.Log(replay);
        }
    }

    // Save the replay to a file
    // We add the replay to the index file
    public void SaveInFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "replays");

        // Create the replay directory
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        // We update the replay file index
        string indexPath = filePath + "/replayIndex.rpi";
        ReplayIndexFile replayIndexFile = new ReplayIndexFile();
        if (!File.Exists(indexPath))
        {
            // First time that a replay is saved, we create the index file
            replayIndexFile.replayFileCount = 1;
        }
        else
        {
            // We update the index file
            using (Stream stream = File.Open(indexPath, FileMode.Open))
            {
                IFormatter formatter = new BinaryFormatter();
                try
                {
                    // Deserialize the InputInfos from the stream
                    ReplayIndexFile rif = (ReplayIndexFile)formatter.Deserialize(stream);

                    replayIndexFile = rif;
                }
                catch (SerializationException e)
                {
                    Debug.Log("Deserialization failed : " + e.Message);
                    throw;
                }
            }

            replayIndexFile.replayFileCount++;
        }

        ReplayIndexFile.ReplayFileMetaData rfmd = new ReplayIndexFile.ReplayFileMetaData(replay.customMapPath + "_" + System.DateTime.Now.ToString("ddMMyyyyHHmm"), replay.customMapPath, "TODO", System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"), replay.GetPlayerNames());
        replayIndexFile.replayInfos.Add(rfmd);

        // Write the index file
        using (Stream stream = File.Open(indexPath, FileMode.Create))
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                // Serialize the InputInfos into the stream
                formatter.Serialize(stream, replayIndexFile);
            }
            catch (SerializationException e)
            {
                Debug.Log("Serialization failed : " + e.Message);
                throw;
            }
        }


        // We save the replay

        //Debug.Log(filePath);
        filePath += "/" + replay.customMapPath + "_" + System.DateTime.Now.ToString("ddMMyyyyHHmm") + ".rep";

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

    // Open the replay file to watch it
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


    // MainMenu => Show the list of replays that are on the pc
    public void ShowReplayList()
    {
        panelSelectReplay.SetActive(true);

        ReplayIndexFile rif;

        using (Stream stream = File.Open(Path.Combine(Application.persistentDataPath, "replays") + "/replayIndex.rpi", FileMode.Open))
        {
            IFormatter formatter = new BinaryFormatter();
            try
            {
                // Get the replays metadata from the index file
                rif = (ReplayIndexFile)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Debug.Log("Deserialization failed : " + e.Message);
                throw;
            }
        }

        for (int i = 0; i < rif.replayFileCount; i++)
        {
            GameObject newEntry = Instantiate(prefabEntry, contentUI);
            ReplayEntry re = newEntry.GetComponent<ReplayEntry>();
            ReplayIndexFile.ReplayFileMetaData rfmd = rif.replayInfos[i];
            re.mapName = rfmd.mapName;
            re.replayName = rfmd.replayName;
            re.date = rfmd.date;
        }
    }
}
