using UnityEngine;
using System.Collections;

public class CameraFacing : MonoBehaviour
{
    public Camera m_Camera;

    void Start()
    {
        m_Camera = Camera.main;
    }

    void Update()
    {
        //No more exception when switching scene, yeay
        if (m_Camera != null)
            transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
                m_Camera.transform.rotation * Vector3.up);
    }
}