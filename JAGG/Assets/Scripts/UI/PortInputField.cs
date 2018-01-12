using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class PortInputField : MonoBehaviour {

    public InputField inputField;

    [HideInInspector]
    public bool isValid = false;

    private string allowed = "[^0-9]";

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

        ValidatePort(output);
    }

    public void ValidatePort(string p)
    {
        int port = 0;
        int.TryParse(p, out port);

        isValid = (port > 1 && port < 65535);
    }
}
