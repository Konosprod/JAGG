using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMode : MonoBehaviour {


    public GameObject ball;
    public GameObject MainCamera;
    public GameObject GUICamera;
    public GameObject slider;
    public GameObject background;   // Piece selection
    public GameObject scrollView;   // Piece selection
    public GameObject testButton;
    public GameObject exitTestButton;
    public GameObject holeSelection;
    public EditorManager editorMan;


    [Header("End of game panel")]
    public GameObject endOfTestPanel;
    public Text shotsText;
    public Text parText;
    public Text maxShotsText;
    public Text timeText;
    public Text maxTimeText;

    private bool isTestMode = false;
    private Vector3 saveCameraPos = Vector3.zero;
    
    private float panelTimer = 0f;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
        {
            TestHole(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isTestMode)
        {
            TestHole(false);
            ball.GetComponent<OfflineBallController>().resetTest();
        }

        if(endOfTestPanel != null  && endOfTestPanel.activeSelf)
        {
            panelTimer -= Time.deltaTime;
            if(panelTimer < 0f)
            {
                endOfTestPanel.SetActive(false);
            }
        }
    }

    // start = true => We start the test
    //       = false => We stop the test   
    public void TestHole(bool start)
    {
        if (editorMan.canStartTestMode())
        {
            isTestMode = start;

            if(start)
            {
                saveCameraPos = MainCamera.transform.position;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                MainCamera.transform.position = saveCameraPos;
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            Cursor.visible = !start;

            MainCamera.GetComponent<FreeCamera>().enabled = !start;
            editorMan.setSelection(start);
            MainCamera.GetComponent<BallCamera>().enabled = start;
            GUICamera.GetComponent<BallCamera>().enabled = start;

            background.SetActive(!start);
            scrollView.SetActive(!start);
            testButton.SetActive(!start);
            exitTestButton.SetActive(start);
            holeSelection.SetActive(!start);

            ball.transform.position = editorMan.getSpawnPosition();

            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.transform.parent = null;

            ball.SetActive(start);
            slider.SetActive(start);

            LevelEditorMovingPieceManager._instance.ResetAllRTPs();
            LevelEditorMovingPieceManager._instance.ResetAllMVPs();
        }
    }

    public void EndOfTest(int shots, float timer)
    {
        LevelProperties lvlProp = editorMan.getCurrentHoleLevelProp().GetComponent<LevelProperties>();

        shotsText.text = shotsText.text.Split(':')[0] + ": " + shots;
        parText.text = parText.text.Split(':')[0] + ": " + lvlProp.par;
        maxShotsText.text = maxShotsText.text.Split(':')[0] + ": " + lvlProp.maxShot;
        timeText.text = timeText.text.Split(':')[0] + ": " + timer.ToString("0.##") + "s";
        maxTimeText.text = maxTimeText.text.Split(':')[0] + ": " + lvlProp.maxTime + "s";
        
        panelTimer = 5f;
        endOfTestPanel.SetActive(true);
    }


    public bool isInTest()
    {
        return isTestMode;
    }

}
