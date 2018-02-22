using UnityEngine.UI;
using UnityEngine;
using Ionic.Zip;
using System.IO;

public class PanelRulesEdit : MonoBehaviour {


    [Header("UI")]
    public Transform contentPanel;
    public GameObject holeInfoPrefab;
    public Toggle highgravityToggle;
    public Toggle lowGravityToggle;

    [Header("Game Logic")]
    public LobbyManager lobbyManager;

    private CustomLevel level = new CustomLevel();

    // Use this for initialization
    void Start() {

        if (lobbyManager.playScene == "Custom")
        {
            string json = "";
            string filename = Application.persistentDataPath + "/levels/" + lobbyManager.customMapFile + ".map";

            using (ZipFile mapFile = ZipFile.Read(filename))
            {
                using (MemoryStream s = new MemoryStream())
                {
                    ZipEntry e = mapFile["level.json"];
                    e.Extract(s);

                    s.Seek(0, SeekOrigin.Begin);

                    using (StreamReader sr = new StreamReader(s))
                    {
                        json = sr.ReadToEnd();
                    }
                }
            }

            level = JsonUtility.FromJson<CustomLevel>(json);
        }
        else
        {
            string json = System.IO.File.ReadAllText(Application.dataPath + "/Resources/Levels/" + lobbyManager.playScene + ".json");
            level = JsonUtility.FromJson<CustomLevel>(json);
        }

        int i = 0;

        foreach (Hole h in level.holes)
        {
            GameObject newHoleEntry = Instantiate(holeInfoPrefab, contentPanel);

            newHoleEntry.GetComponentInChildren<Text>().text = (i + 1).ToString();

            InputField[] inputs = newHoleEntry.GetComponentsInChildren<InputField>();

            inputs[0].text = h.properties.maxTime.ToString();
            inputs[1].text = h.properties.maxShot.ToString();

            i++;
        }

        i = 0;
    }

    // Update is called once per frame
    void Update() {

    }

    public void Save()
    {
        for (int i = 0; i < contentPanel.childCount; i++)
        {
            GameObject go = contentPanel.GetChild(i).gameObject;

            InputField[] inputs = go.GetComponentsInChildren<InputField>();

            level.holes[i].properties.maxTime = int.Parse(inputs[0].text);
            level.holes[i].properties.maxShot = int.Parse(inputs[1].text);
        }

        if(highgravityToggle.group.AnyTogglesOn())
        {
            if (highgravityToggle.isOn)
                lobbyManager.gravity = 2;
            else
                lobbyManager.gravity = 1;
        }
        else
        {
            lobbyManager.gravity = 0;
        }
        

        lobbyManager.ruleSet = level;
    }
}
