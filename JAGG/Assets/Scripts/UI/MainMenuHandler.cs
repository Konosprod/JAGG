using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public Button btn_Quit;
    public Button btn_Start;
    public Fader fader;
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
    }

    void OnEnable()
    {
        fader.FadeIn(0.5f, delegate () { canvasGroup.blocksRaycasts = true; });
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
