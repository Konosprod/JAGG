using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldValidate : MonoBehaviour {

    public InputField inputField;

    [HideInInspector]
    public bool isValid = false;

    public string allowed = "[^0-9.]";

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

        Validate(output);
    }

    public virtual void Validate(string text)
    {

    }
}
