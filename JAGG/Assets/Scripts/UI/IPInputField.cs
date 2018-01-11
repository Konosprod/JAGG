using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class IPInputField : MonoBehaviour {

    public InputField inputField;

    private string allowed = "[^0-9.]";

    public void UpdateField()
    {
        string text = inputField.text;
        string output = Regex.Replace(text, allowed, string.Empty, RegexOptions.IgnoreCase);
        inputField.text = output;

    }
}
