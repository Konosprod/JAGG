using System.Collections;
using System.Collections.Generic;
using JAGG;
using UnityEngine;
using UnityEngine.UI;

public class TextTranslator : MonoBehaviour {

    private I18n i18n = I18n.Instance;

    private Text textEntry;

	// Use this for initialization
	void Start() {
        textEntry = GetComponent<Text>();

        Mgl.I18n.SetLocale("fr-FR");
        string text = textEntry.text;
        textEntry.text = i18n.__(text);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
