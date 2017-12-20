using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public Button btn_Quit;
    public Button btn_Start;

    public int sceneIndex;

    void Start()
    {
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

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }
}
