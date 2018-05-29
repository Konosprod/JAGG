using System.Text.RegularExpressions;

public class HexColorInputField : InputFieldValidate {

	// Use this for initialization
	void Start () {
        isValid = false;
        UpdateField();
    }

    public override void Validate(string text)
    {
        isValid = true;
    }
}
