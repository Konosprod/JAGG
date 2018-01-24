using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class CreateLevelProperties : MonoBehaviour
{
    private static string activeScene;

    [MenuItem("Tools/Generate Level Properties")]
    static void GenerateLevelPropeties()
    {
        activeScene = EditorSceneManager.GetActiveScene().path;

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        foreach (EditorBuildSettingsScene S in EditorBuildSettings.scenes)
        {
            if (S.enabled)
            {
                if (S.path.Contains("Levels"))
                {
                    CustomLevel customLevel = new CustomLevel();
                    customLevel.holes = new List<Hole>();

                    string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
                    name = name.Substring(0, name.Length - 6);

                    Debug.Log("Generating file for : " + name);

                    EditorSceneManager.OpenScene(S.path);
                    

                    GameObject holes = GameObject.Find("Holes");

                    foreach(LevelProperties p in holes.GetComponentsInChildren<LevelProperties>())
                    {
                        Hole h = new Hole();
                        h.properties = new HoleInfo();

                        h.properties.maxShot = p.maxShot;
                        h.properties.maxTime = p.maxTime;

                        customLevel.holes.Add(h);
                    }

                    string json = JsonUtility.ToJson(customLevel, true);
                    System.IO.File.WriteAllText(Application.dataPath + "/Resources/Levels/" + name + ".json", json);
                }
            }
        }

        EditorSceneManager.OpenScene(activeScene);

        Debug.Log("Done");
    }
}