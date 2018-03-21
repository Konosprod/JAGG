using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingOverlay : MonoBehaviour {

    public float rotationSpeed = 200f;
    public RectTransform progressImage;
    public Text messageText;

    private bool playing = false;


    void Start()
    {

    }

    void Update()
    {
        if (playing)
        {
            progressImage.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    public void PlayAnimation()
    {
        playing = true;
    }

    public void StopAnimation()
    {
        playing = false;
    }

    public bool IsLoading()
    {
        return playing;
    }

}
