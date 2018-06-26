using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System;
using UnityEngine.Networking;

public class MainMenuHandler : MonoBehaviour
{
    public Button btn_Quit;
    public Button btn_Start;
    public Fader fader;
    public Text version;
    public CanvasGroup canvasGroup;
    
    [HideInInspector]
    public int sceneIndex;

    private SoundManager soundManager;


    void Start()
    {
        btn_Quit.onClick.AddListener(Quit);
        btn_Start.onClick.AddListener(FadeOutMenu);

        soundManager = SoundManager._instance;

        soundManager.PlayMusic(SoundType.MainMenu);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        version.text = "Version " + Application.version;
    }

    private void FadeOutMenu()
    {
        canvasGroup.blocksRaycasts = false;
        fader.FadeOut();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  
#else
        Application.Quit();
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }
}
