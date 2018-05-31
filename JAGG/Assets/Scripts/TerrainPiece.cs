using UnityEngine;
using System.Reflection;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TerrainPiece : MonoBehaviour {
    public string id;
    public bool prefab = true;
    public int number;
    public int parentNumber = -1;

    public JContainer ToJson(JContainer parent = null)
    {
        JObject obj = new JObject
        {
            { "prefab", this.prefab }
        };

        JObject rotation = new JObject
        {
            { "x", gameObject.transform.localEulerAngles.x },
            { "y", gameObject.transform.localEulerAngles.y },
            { "z", gameObject.transform.localEulerAngles.z }
        };

        JObject scale = new JObject
        {
            { "x", gameObject.transform.localScale.x },
            { "y", gameObject.transform.localScale.y },
            { "z", gameObject.transform.localScale.z }
         };

        JObject position = new JObject
        {
            { "x", gameObject.transform.position.x },
            { "y", gameObject.transform.position.y },
            { "z", gameObject.transform.position.z }
        };

        obj.Add("id", id);
        obj.Add("number", number);
        obj.Add("parentNumber", parentNumber);
        obj.Add("rotation", rotation);
        obj.Add("scale", scale);
        obj.Add("position", position);

        JArray scripts = new JArray();

        foreach(CustomScript s in this.GetComponents<CustomScript>())
        {
            s.ToJson(scripts);
        }

        obj.Add("scripts", scripts);

        if (parent != null)
            parent.Add(obj);

        return obj;
    }

    public void FromJson(string json)
    {
        JObject obj = JObject.Parse(json);

        number = (int)obj["number"];
        parentNumber = (int)obj["parentNumber"];

        gameObject.transform.position = new Vector3(
            (float)obj["position"]["x"],
            (float)obj["position"]["y"],
            (float)obj["position"]["z"]
        );

        gameObject.transform.localScale = new Vector3(
            (float)obj["scale"]["x"],
            (float)obj["scale"]["y"],
            (float)obj["scale"]["z"]
        );

        gameObject.transform.localEulerAngles = new Vector3(
            (float)obj["rotation"]["x"],
            (float)obj["rotation"]["y"],
            (float)obj["rotation"]["z"]
        );

        if(obj["scripts"].HasValues)
        {
            JArray scripts = (JArray)obj["scripts"];

            foreach(JObject script in scripts)
            {
                CustomScript cScript = null;
                if (gameObject.GetComponent((string)script["class"]))
                {
                    cScript = gameObject.GetComponent((string)script["class"]) as CustomScript;
                }
                else
                {
                    cScript = gameObject.AddComponent(Type.GetType((string)script["class"])) as CustomScript;
                }

                cScript.FromJson(script.ToString());
            }
        }
    }
}
