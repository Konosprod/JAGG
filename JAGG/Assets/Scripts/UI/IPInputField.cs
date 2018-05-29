using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;

public class IPInputField : InputFieldValidate {

    void Start()
    {
        isValid = false;
        UpdateField();
    }

    public override void Validate(string text)
    {
        string[] bytes = text.Split('.');

        if (bytes.Length == 4)
        {
            foreach (string b in bytes)
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
