using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelValidationEnd : MonoBehaviour {

    public EditorManager editorManager;
    public TestMode testMode;

    public Text[] parList;
    public Text[] shotList;
    public Text[] timeList;

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

            parList[i].text = par.ToString();
            shotList[i].text = shots.ToString();
            timeList[i].text = time.ToString("0.##") + "s";

            //parTotal += par;
            shotsTotal += shots;
            timeTotal += time;
        }
    }

    
    // For upload button
    public void Upload()
    {
        testMode.UploadMap();
        this.gameObject.SetActive(false);
    }
}
