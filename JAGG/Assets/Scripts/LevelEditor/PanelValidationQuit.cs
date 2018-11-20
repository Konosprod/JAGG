using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationQuit : MonoBehaviour
{

    [Header("UI")]
    public Button quitButton;
    public Button cancelButton;

    [Header("Other")]
    public EditorManager editorManager;


    private CursorLockMode saveCursorLockMode;
    private bool saveCursorVisibility;

    void OnEnable()
    {
#if UNITY_EDITOR
#else
        saveCursorLockMode = Cursor.lockState;
        saveCursorVisibility = Cursor.visible;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
#endif
    }

    // Use this for initialization
    void Start()
    {
        // Buttons callbacks
        quitButton.onClick.AddListener(Quit);
        cancelButton.onClick.AddListener(Cancel);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    public void Quit()
    {
        editorManager.testMode.TestHole(false, false, true);
        this.gameObject.SetActive(false);
    }

    public void Cancel()
    {
#if UNITY_EDITOR
#else
        Cursor.lockState = saveCursorLockMode;
        Cursor.visible = saveCursorVisibility;
#endif
        this.gameObject.SetActive(false);
    }
}
