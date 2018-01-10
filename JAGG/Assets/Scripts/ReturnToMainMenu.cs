using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ReturnToMainMenu : MonoBehaviour {

    private Button buttonReturn;

    public GameObject[] toDestroy;

    void Start()
    {
        buttonReturn = GetComponent<Button>();

        buttonReturn.onClick.RemoveAllListeners();
        buttonReturn.onClick.AddListener(ReturnToScene);
    }

    public void ReturnToScene()
    {
        for(int i = 0; i < toDestroy.Length; i++)
        {
            Destroy(toDestroy[i]);
        }

        /*SoundManager go = GameObject.FindObjectOfType<SoundManager>();

        if (go)
            Destroy(go.gameObject);
        */
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
