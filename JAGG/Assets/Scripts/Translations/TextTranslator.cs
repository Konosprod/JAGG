using System.Collections;
using System.Collections.Generic;
using JAGG;
using UnityEngine;
using UnityEngine.UI;

public class TextTranslator : MonoBehaviour {

    private I18n i18n = I18n.Instance;

    public string text;

	// Use this for initialization
	void Start() {
        Mgl.I18n.SetLocale("fr-FR");
        GetComponent<Text>().text = i18n.__(text);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
