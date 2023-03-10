using UnityEngine.UI;
using UnityEngine;
using Ionic.Zip;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

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
    void OnEnable()
    {
        CleanPanel();
        JObject json = null;

        if (lobbyManager.playScene == "Custom")
        {
            string filename = Application.persistentDataPath + "/levels/" + lobbyManager.customMapFile + ".map";

            using (ZipFile mapFile = ZipFile.Read(filename))
            {
                using (MemoryStream s = new MemoryStream())
                {
                    ZipEntry e = mapFile["level.json"];
                    e.Extract(s);

                    s.Seek(0, SeekOrigin.Begin);

                    using (BsonReader br = new BsonReader(s))
                    {
                        json = (JObject)JToken.ReadFrom(br);
                        Debug.Log(json.ToString(Newtonsoft.Json.Formatting.None));
                    }
                }
            }
        }
        else
        {
            json = JObject.Parse(System.IO.File.ReadAllText(Application.dataPath + "/Resources/Levels/" + lobbyManager.playScene + ".json"));
        }

        level = JsonUtility.FromJson<CustomLevel>(json.ToString());

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
                lobbyManager.gravity = GravityType.High;
            else
                lobbyManager.gravity = GravityType.Low;
        }
        else
        {
            lobbyManager.gravity = GravityType.Normal;
        }
        

        lobbyManager.ruleSet = level;
    }

    void CleanPanel()
    {
        foreach(Transform t in contentPanel)
        {
            Destroy(t.gameObject);
        }
    }
}
