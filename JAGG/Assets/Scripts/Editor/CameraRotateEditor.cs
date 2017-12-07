using UnityEngine;
using UnityEditor;

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
            rotateCamera.point = EditorGUILayout.Vector3Field("Point", rotateCamera.point);
        }
        else
        {
            rotateCamera.target = EditorGUILayout.ObjectField("Target", rotateCamera.target, typeof(GameObject), true) as GameObject;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
