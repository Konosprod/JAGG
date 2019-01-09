using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMode : MonoBehaviour {

    [Header("Test objects")]
    public GameObject ball;
    public GameObject MainCamera;
    public GameObject GUICamera;
    public GameObject slider;
    public GameObject background;   // Piece selection
    public GameObject testButton;
    public GameObject exitTestButton;
    public GameObject holeSelection;
    public EditorManager editorManager;
    public BoxCollider planeDecorCol; // Activate the plane's collider during tests
    public GameObject planeRaycast; // Disable the plane raycast (for piece placement) during tests
    public GameObject grid;         // Disable the grid during tests as well


    [Header("End of game panel")]
    public GameObject endOfTestPanel;
    public Text shotsText;
    public Text parText;
    public Text maxShotsText;
    public Text timeText;
    public Text maxTimeText;


    [Header("Validation panels")]
    public GameObject validationBetweenHolePanel;
    public GameObject validationFailHolePanel;
    public GameObject validationEndPanel;
    public GameObject validationQuitPanel;

    private bool isTestMode = false;
    private bool isValidationMode = false;
    private int saveCurrentHole;
    private int currentValidationHole = -1;
    private LevelProperties currentLevelProp;
    private Vector3 saveCameraPos = Vector3.zero;
    private OfflineBallController ballController;

    [HideInInspector]
    public int[] validationShots = new int[18];
    [HideInInspector]
    public float[] validationTimes = new float[18];
    
    private float panelTimer = 0f;

    void Start()
    {
        ballController = ball.GetComponent<OfflineBallController>();
    }


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

            if (Input.GetKeyDown(KeyCode.Escape) && isTestMode && !validationQuitPanel.activeSelf)
            {
                if (!isValidationMode)
                {
                    TestHole(false, false, true);
                    ball.GetComponent<OfflineBallController>().ResetTest();
                }
                else
                {
                    validationQuitPanel.SetActive(true);
                }
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
                    saveCurrentHole = editorManager.GetCurrentHoleNumber();
                    // Disable all MaterialSwaperoos
                    editorManager.EnableMaterialSwaperoo(false);
                    // Set currentHole to the one we are first testing
                    editorManager.ChangeCurrentHole(currentValidationHole);
                    currentLevelProp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
                }
            }

#if UNITY_EDITOR
#else
            if (start)
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
#endif

            MainCamera.GetComponent<FreeCamera>().enabled = !start;
            editorManager.SetSelection(start);
            MainCamera.GetComponent<BallCamera>().enabled = start;
            GUICamera.GetComponent<BallCamera>().enabled = start;
            MainCamera.GetComponent<cakeslice.OutlineEffect>().enabled = !start;

            background.SetActive(!start);
            testButton.SetActive(!start);
            exitTestButton.SetActive(start);
            holeSelection.SetActive(!start);
            planeDecorCol.enabled = start;
            planeRaycast.SetActive(!start);
            grid.SetActive(!start);

            ball.transform.position = editorManager.GetSpawnPosition();

            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.transform.parent = null;

            ball.SetActive(start);
            slider.SetActive(start);

            LevelEditorMovingPieceManager._instance.ResetAllRTPs();
            LevelEditorMovingPieceManager._instance.ResetAllMVPs();


            if(forceExit)
            {
                validationBetweenHolePanel.SetActive(false);
                validationFailHolePanel.SetActive(false);
                validationEndPanel.SetActive(false);
            }
        }
    }

    public void EndOfTest(int shots, float timer, bool maxShotFail = false)
    {
        if (isValidationMode)
        {
            LevelProperties lvlProp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();
            Debug.Log("Shots : " + shots + ", timer : " + timer);
            bool success = shots <= lvlProp.maxShot && timer < lvlProp.maxTime && !maxShotFail;

            if (success)
            {
                // Save the shots and time
                validationShots[currentValidationHole] = shots;
                validationTimes[currentValidationHole] = timer;

                int nextValidationHole = editorManager.GetNextValidHole(currentValidationHole);
                // Well job, good done
                if (nextValidationHole == -1)
                {
#if UNITY_EDITOR
#else
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
#endif
                    validationEndPanel.SetActive(true);
                }
                else
                {
                    // Go to the next hole
#if UNITY_EDITOR
#else
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
#endif

                    validationBetweenHolePanel.SetActive(true);
                    validationBetweenHolePanel.GetComponent<PanelValidationBetweenHole>().SetTestResults(shots, timer);
                }
            }
            else
            {
                // Filthy casual, get on your level
#if UNITY_EDITOR
#else
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
#endif

                validationFailHolePanel.SetActive(true);
                validationFailHolePanel.GetComponent<PanelValidationFailHole>().SetTestResults(shots, timer);
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

    public void ValidationGoToNextHole()
    {
        currentValidationHole = editorManager.GetNextValidHole(currentValidationHole);
        editorManager.ChangeCurrentHole(currentValidationHole);
        currentLevelProp = editorManager.GetCurrentHoleLevelProp().GetComponent<LevelProperties>();

#if UNITY_EDITOR
#else
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif

        ball.transform.position = editorManager.GetSpawnPosition();
        ball.SetActive(true);
    }

    public void ValidationReplayHole()
    {
#if UNITY_EDITOR
#else
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif

        ball.transform.position = editorManager.GetSpawnPosition();
        ball.SetActive(true);
    }

    public void CheckShots(int shots, float time)
    {
        if(shots >= currentLevelProp.maxShot)
        {
            EndOfTest(shots, time, true);
            ballController.ResetTest();
        }
    }

    public void CheckTime(float time, int shots)
    {
        if(time >= currentLevelProp.maxTime)
        {
            EndOfTest(shots, time);
            ballController.ResetTest();
        }
    }


    public bool IsInTest()
    {
        return isTestMode;
    }

    public bool IsInValidation()
    {
        return isValidationMode;
    }

    // Uploads the map, should be called from PanelValidationEnd only
    public void UploadMap()
    {
        // Validation is over :D
        isValidationMode = false;
        TestHole(false);

        EditorManager.isModified = false;
        editorManager.escapeMenu.gameObject.SetActive(true);
        editorManager.panelExport.gameObject.SetActive(true);
        editorManager.panelExport.ValidationBeforeUpload();

        // Enable all MaterialSwaperoos
        editorManager.EnableMaterialSwaperoo(true);
        // Set the current hole back to what it was before validation
        editorManager.ChangeCurrentHole(saveCurrentHole);
    }

}
