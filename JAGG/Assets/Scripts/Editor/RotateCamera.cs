using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RotateCamera : MonoBehaviour {

    public float angle = 20.0f;
    public float speed = 10.0f;

    public bool onPoint = false;

    public GameObject target;
    public Vector3 point;

    private Vector3 pivot;

	// Use this for initialization
	void Start () {

		if(onPoint)
        {
            pivot = point;
        }
        else
        {
            pivot = target.transform.position;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(pivot);
        transform.RotateAround(target.transform.position, Vector3.up, speed * angle * Time.deltaTime);
	}
}

[CustomEditor(typeof(RotateCamera))]
[CanEditMultipleObjects]
public class RotateCameraEditor : Editor
{
    void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        RotateCamera rotateCamera = target as RotateCamera;

        serializedObject.Update();
        rotateCamera.angle = EditorGUILayout.FloatField("Angle", rotateCamera.angle);
        rotateCamera.speed = EditorGUILayout.FloatField("Speed", rotateCamera.speed);

        rotateCamera.onPoint = EditorGUILayout.Toggle("On Point", rotateCamera.onPoint);

        if (rotateCamera.onPoint)
        {
            rotateCamera.point =  EditorGUILayout.Vector3Field("Point", rotateCamera.point);
        }
        else
        {
            rotateCamera.target = EditorGUILayout.ObjectField("Target", rotateCamera.target, typeof(GameObject), true) as GameObject;
        }

        serializedObject.ApplyModifiedProperties();
    }
}