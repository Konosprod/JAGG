using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelHoleProperties : MonoBehaviour {

    public EditorManager editorManager;

    [Header("Inputs")]
    public InputField spawnXInput;
    public InputField spawnYInput;
    public InputField spawnZInput;

    public InputField parInput;
    public InputField maxShotInput;
    public InputField timeInput;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Load(GameObject spawnPoint, GameObject levelProperties)
    {
        if (!spawnPoint.transform.GetChild(0).gameObject.activeSelf)
        {
            spawnXInput.text = "None";
            spawnXInput.interactable = false;
            spawnYInput.text = "None";
            spawnYInput.interactable = false;
            spawnZInput.text = "None";
            spawnZInput.interactable = false;
        }
        else
        {
            spawnXInput.interactable = true;
            spawnYInput.interactable = true;
            spawnZInput.interactable = true;

            Vector3 position = spawnPoint.transform.position;
            spawnXInput.text = position.x.ToString();
            spawnYInput.text = position.y.ToString();
            spawnZInput.text = position.z.ToString();
        }

        LevelProperties levelProp = levelProperties.GetComponent<LevelProperties>();

        parInput.text = levelProp.par.ToString();
        maxShotInput.text = levelProp.maxShot.ToString();
        timeInput.text = levelProp.maxTime.ToString();
    }

    public void UpdateSpawn()
    {
        float x = 0f;
        float.TryParse(spawnXInput.text, out x);
        float y = 0f;
        float.TryParse(spawnYInput.text, out y);
        float z = 0f;
        float.TryParse(spawnZInput.text, out z);

        editorManager.UpdateSpawnPoint(new Vector3(x, y, z));
    }

    public void UpdateLevelProperties()
    {
        int par = 0;
        int.TryParse(parInput.text, out par);

        int maxshot = 0;
        int.TryParse(maxShotInput.text, out maxshot);

        int time = 0;
        int.TryParse(timeInput.text, out time);

        editorManager.UpdateLevelProperties(par, maxshot, time);
    }
}
