using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowPop : MonoBehaviour
{
    public float scaleTargeted = 1.0f;
    public float startingScale = 0.3f;
    float smoothTime = 0.1f;
    float yVelocity = 0.0f;

    void Start()
    {
        transform.localScale = new Vector3(startingScale, startingScale);
    }

    void OnEnable()
    {
        transform.localScale = new Vector3(startingScale, startingScale);
    }

    void Update()
    {
        float newScale = Mathf.SmoothDamp(transform.localScale.x, scaleTargeted, ref yVelocity, smoothTime);
        transform.localScale = new Vector3(newScale, newScale);
    }
}
