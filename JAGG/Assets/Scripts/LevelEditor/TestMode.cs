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
    public GameObject testButton;
    public GameObject exitTestButton;
    public GameObject holeSelection;
    public EditorManager editorManager;


    [Header("End of game panel")]
    public GameObject endOfTestPanel;
    public Text shotsText;
    public Text parText;
    public Text maxShotsText;
    public Text timeText;
    public Text maxTimeText;

    private bool isTestMode = false;
    private bool isValidationMode = false;
    private int currentValidationHole = -1;
    private Vector3 saveCameraPos = Vector3.zero;
    
    private float panelTimer = 0f;


    // Update is called once per frame
    void Update()
    {
        if (editorManager.canEdit)
        {
            if (Input.GetKeyDown(KeyCode.T) && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)))
            {
                editorManager.DisableGizmos();
                TestHole(true);
            }

            if (Input.GetKeyDown(KeyCode.Escape) && isTestMode)
            {
                TestHole(false, false, true);
                ball.GetComponent<OfflineBallController>().ResetTest();
            }

            if (endOfTestPanel != null && endOfTestPanel.activeSelf)
            {
                panelTimer -= Time.deltaTime;
                if (panelTimer < 0f)
                {
                    endOfTestPanel.SetActive(false);
                }
            }
        }
    }

    // Unity <3
    public void TestHoleForButton()
    {
        editorManager.DisableGizmos();
        TestHole(true);
    }

    // start = true => We start the test
    //       = false => We stop the test   
    public void TestHole(bool start, bool validate = false, bool forceExit = false)
    {
        if ((!isValidationMode || forceExit) && (editorManager.CanStartTestMode() || validate))
        {
            isTestMode = start;
            isValidationMode = validate;
            currentValidationHole = editorManager.GetNextValidHole(-1);
            if(validate)
            {
                if (currentValidationHole == -1)
                    Debug.LogError("We try to validate but there are no valid holes :[");
                else
                {
                    // Disable all MaterialSwaperoos
                    editorManager.EnableMaterialSwaperoo(false);
                    // Set currentHole to the one we are first testing
                    editorManager.ChangeCurrentHole(currentValidationHole);
                }
            }

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
            editorManager.setSelection(start);
            MainCamera.GetComponent<BallCamera>().enabled = start;
            GUICamera.GetComponent<BallCamera>().enabled = start;

            background.SetActive(!start);
            testButton.SetActive(!start);
            exitTestButton.SetActive(start);
            holeSelection.SetActive(!start);

            ball.transform.position = editorManager.GetSpawnPosition();

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
        if (isValidationMode)
        {
            LevelProperties lvlProp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
            Debug.Log("Shots : " + shots + ", timer : " + timer);
            bool success = shots <= lvlProp.maxShot && timer < lvlProp.maxTime;

            if (success)
            {
                currentValidationHole = editorManager.GetNextValidHole(currentValidationHole);
                // Well job, good done
                if (currentValidationHole == -1)
                {
                    // Validation is over :D
                    isValidationMode = false;
                    TestHole(false);

                    editorManager.escapeMenu.gameObject.SetActive(true);
                    editorManager.panelExport.gameObject.SetActive(true);
                    editorManager.panelExport.UploadMap();

                    // Enable all MaterialSwaperoos
                    editorManager.EnableMaterialSwaperoo(true);
                }
                else
                {
                    // Go to the next hole
                    editorManager.ChangeCurrentHole(currentValidationHole);

                    ball.transform.position = editorManager.GetSpawnPosition();
                    ball.SetActive(true);
                }
            }
            else
            {
                // Filthy casual, get on your level

                ball.transform.position = editorManager.GetSpawnPosition();
                ball.SetActive(true);
            }
        }
        else
        {
            LevelProperties lvlProp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();

            shotsText.text = shotsText.text.Split(':')[0] + ": " + shots;
            parText.text = parText.text.Split(':')[0] + ": " + lvlProp.par;
            maxShotsText.text = maxShotsText.text.Split(':')[0] + ": " + lvlProp.maxShot;
            timeText.text = timeText.text.Split(':')[0] + ": " + timer.ToString("0.##") + "s";
            maxTimeText.text = maxTimeText.text.Split(':')[0] + ": " + lvlProp.maxTime + "s";

            panelTimer = 5f;
            endOfTestPanel.SetActive(true);
        }
    }


    public bool isInTest()
    {
        return isTestMode;
    }

}
