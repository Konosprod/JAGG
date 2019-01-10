using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{

    public static ReplayManager _instance;

    public List<ReplayObject> replayObjects = new List<ReplayObject>();
    public bool isReplayActive = false;
    public bool isReplayPlaying = false;

    private int replayFrame = 0;
    private ReplayObject currentRO;

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
        if(isReplayActive)
        {
            if(Input.GetKeyDown(KeyCode.P))
            {
                PlayReplay();
                LevelEditorMovingPieceManager._instance.ResetAllMVPs();
                LevelEditorMovingPieceManager._instance.ResetAllRTPs();
                //currentRO.TestSerialize();
            }
        }

        if(isReplayPlaying)
        {
            if(replayFrame == currentRO.inputs[0].frame)
            {
                // Play the input
                if(currentRO.inputs[0].sliderValue != -1f)
                {
                    Debug.Log(currentRO.inputs[0]);

                    if (currentRO.inputs[0].sliderValue != -2f)
                    {
                        currentRO.rb.AddForce(currentRO.inputs[0].dir * Mathf.Pow(currentRO.inputs[0].sliderValue, 1.4f) * 2f);
                    }
                    else
                    {
                        // -2f in sliderValue is when the ball is reset to its former position
                        currentRO.transform.position = currentRO.inputs[0].pos;
                        currentRO.rb.velocity = Vector3.zero;
                    }


                    currentRO.inputs.RemoveAt(0);
                }
                else
                {
                    // -1f in sliderValue is when the ball gets in the hole
                    StopReplay();
                }
            }

            replayFrame++;
        }
    }

    public void StartReplay()
    {
        isReplayActive = true;
        currentRO = replayObjects[0];
        replayFrame = /*currentRO.inputs[0].frame - 1*/ 0;
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
        currentRO = null;
    }


    public void AddReplayObject(ReplayObject ro)
    {
        replayObjects.Add(ro);
    }
}
