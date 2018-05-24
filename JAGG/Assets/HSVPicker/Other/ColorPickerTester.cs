using UnityEngine;
using System.Collections;

public class ColorPickerTester : MonoBehaviour 
{

    public Renderer newRenderer;
    public ColorPicker picker;

    public Color Color = Color.red;

	// Use this for initialization
	void Start () 
    {
        picker.onValueChanged.AddListener(color =>
        {
            newRenderer.material.color = color;
            Color = color;
        });

		newRenderer.material.color = picker.CurrentColor;

        picker.CurrentColor = Color;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
