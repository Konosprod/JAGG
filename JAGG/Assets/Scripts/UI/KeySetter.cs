using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeySetter : MonoBehaviour
{
    public Button buttonKey;
    public Text labelButton;
    public KeyAction keyAction;

    private bool isListening = false;

    // Start is called before the first frame update
    void Start()
    {
        buttonKey.onClick.RemoveAllListeners();
        buttonKey.onClick.AddListener(ListenToKey);

        labelButton.text = SettingsManager._instance.gameSettings.Keys[keyAction].ToString();

    }

    // Update is called once per frame
    void Update()
    {
        if(isListening)
        {
            Event e = Event.current;

            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (kcode != KeyCode.Escape)
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        isListening = false;
                        buttonKey.interactable = true;
                        labelButton.text = kcode.ToString();

                        SettingsManager._instance.gameSettings.Keys[keyAction] = kcode;

                        break;
                    }
                }
            }

        }
    }

    public void ListenToKey()
    {
        buttonKey.interactable = false;
        isListening = true;
    }
}
