using UnityEngine.UI;
using UnityEngine;

public class PlayerScoreEntry : MonoBehaviour {

    public GameObject scorePrefab;
    public GameObject paneScore;
    public Text totalScore;

    void Start()
    {

    }

    public void AddScore(int score)
    {
        GameObject scoreObject = Instantiate(scorePrefab.gameObject, paneScore.transform);
        scoreObject.GetComponent<Text>().text = score.ToString();
    }

    public void SetTotal(int total)
    {
        totalScore.text = total.ToString();
    }

    public void CleanScores()
    {
        foreach(Transform child in paneScore.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
