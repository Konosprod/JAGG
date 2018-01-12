using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class IPInputField : MonoBehaviour {

    public InputField inputField;

    [HideInInspector]
    public bool isValid = false;

    private string allowed = "[^0-9.]";

    void Start()
    {
        isValid = false;
        UpdateField();
    }

    public void UpdateField()
    {
        string text = inputField.text;
        string output = Regex.Replace(text, allowed, string.Empty, RegexOptions.IgnoreCase);
        inputField.text = output;

        ValidateIPv4(output);
    }

    public void ValidateIPv4(string ip)
    {
        string[] bytes = ip.Split('.');

        if(bytes.Length == 4)
        {
            foreach(string b in bytes)
            {
                if (b != "")
                {
                    byte res = 0;
                    if (!byte.TryParse(b, out res))
                    {
                        isValid = false;
                        return;
                    }
                }
                else
                {
                    isValid = false;
                    return;
                }
            }

            isValid = true;
        }
        else
        {
            isValid = false;
        }
    }
}
