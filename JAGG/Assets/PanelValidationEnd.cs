using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationEnd : MonoBehaviour {

    public EditorManager editorManager;
    public TestMode testMode;
    public PlayerScoreEntry scoreEntryPar;
    public PlayerScoreEntry scoreEntryPlayerShots;
    public PlayerScoreEntry scoreEntryPlayerTimes;


    void OnEnable()
    {
        GameObject[] lvlProps = editorManager.GetAllLevelProperties();

        //int parTotal = 0;
        int shotsTotal = 0;
        float timeTotal = 0f;

        for(int i=0; i<18; i++)
        {
            int par = lvlProps[i].GetComponent<LevelProperties>().par;
            int shots = testMode.validationShots[i];
            float time = testMode.validationTimes[i];
            scoreEntryPar.AddScore(par);
            scoreEntryPlayerShots.AddScore(shots);
            scoreEntryPlayerTimes.AddScore(time);
            //parTotal += par;
            shotsTotal += shots;
            timeTotal += time;
        }

        //scoreEntryPar.SetTotal(parTotal);
        scoreEntryPlayerShots.SetTotal(shotsTotal);
        scoreEntryPlayerTimes.SetTotal(timeTotal);
    }

    
    // For upload button
    public void Upload()
    {
        testMode.UploadMap();
        this.gameObject.SetActive(false);
    }
}
