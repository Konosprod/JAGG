using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public Button btn_1920x1080;
    public Button btn_1280x720;
    public Button btn_Quit;
    public Button btn_Start;
    public int sceneIndex;

    void Start()
    {
        btn_1280x720.onClick.AddListener(Set1280x720);
        btn_1920x1080.onClick.AddListener(Set1920x1080);
        btn_Quit.onClick.AddListener(Quit);

        SoundManager.PlayMusic(SoundType.MainMenu);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Set1280x720()
    {
        Screen.SetResolution(1280, 720, Screen.fullScreen);
    }

    private void Set1920x1080()
    {
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
    }

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }
}
