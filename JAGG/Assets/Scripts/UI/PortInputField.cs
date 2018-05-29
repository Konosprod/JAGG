using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class PortInputField : InputFieldValidate {

    void Start()
    {
        isValid = false;
        UpdateField();
    }

    public override void Validate(string text)
    {
        int port = 0;
        int.TryParse(text, out port);

        isValid = (port > 1 && port < 65535);
    }
}
