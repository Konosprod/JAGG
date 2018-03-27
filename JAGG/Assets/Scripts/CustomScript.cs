using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using UnityEngine;

public class CustomScript : MonoBehaviour {

    public JsonWriter ToJson(JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("class");
        writer.WriteValue(this.GetType().FullName);
        writer.WritePropertyName("properties");
        writer.WriteStartArray();

        foreach (FieldInfo f in this.GetType().GetFields())
        {
            object[] attributes = f.GetCustomAttributes(true);

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(CustomProp))
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(f.Name);
                    writer.WriteValue(f.GetValue(this));
                    writer.WriteEndObject();
                }
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();

        return writer;
    }

    public JContainer ToJson(JContainer parent = null)
    {
        JObject obj = new JObject
        {
            { "class", this.GetType().FullName }
        };

        JArray properties = new JArray();

        foreach (FieldInfo f in this.GetType().GetFields())
        {
            object[] attributes = f.GetCustomAttributes(true);

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(CustomProp))
                {
                    JObject property = new JObject
                        {
                            { f.Name, JToken.FromObject(f.GetValue(this)) }
                        };

                    properties.Add(property);
                }
            }
        }


        obj.Add("properties", properties);

        if (parent != null)
            parent.Add(obj);

        return obj;
    }

    public void FromJson(string json)
    {
        JObject obj = JObject.Parse(json);

        int i = 0;
        foreach (FieldInfo f in this.GetType().GetFields())
        {
            object[] attributes = f.GetCustomAttributes(true);

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(CustomProp))
                {
                    var value = ((JValue)obj["properties"][i][f.Name]).Value;
                    f.SetValue(this, Convert.ChangeType(value, f.FieldType));
                    i++;
                }
            }
        }
    }
}
