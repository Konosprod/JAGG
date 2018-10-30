using UnityEngine.UI;
using UnityEngine;

public class PlayerScoreEntry : MonoBehaviour {

    public Text playerName;
    public GameObject scorePrefab;
    public GameObject paneScore;
    public Text totalScore;

    void Start()
    {

    }

    // AddScore and SetTotal for shots (int)
    public void AddScore(int score)
    {
        GameObject scoreObject = Instantiate(scorePrefab.gameObject, paneScore.transform);
        scoreObject.GetComponent<Text>().text = score.ToString();
    }
    public void SetTotal(int total)
    {
        totalScore.text = total.ToString();
    }


    // AddScore and SetTotal for times (float)
    public void AddScore(float score)
    {
        GameObject scoreObject = Instantiate(scorePrefab.gameObject, paneScore.transform);
        scoreObject.GetComponent<Text>().text = score.ToString("0.##") + "s";
    }
    public void SetTotal(float total)
    {
        totalScore.text = total.ToString("0.##") + "s";
    }



    public void CleanScores()
    {
        foreach(Transform child in paneScore.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
