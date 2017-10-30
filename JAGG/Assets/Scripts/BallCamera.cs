using UnityEngine;
using System.Collections;
 
public class BallCamera : MonoBehaviour
{

    public Transform target;

    public float distance = 5.0f;

    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float wheelSpeed = 5f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;
    float wheel = 0.0f;
    float lastWheelDistance;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        lastWheelDistance = distance;
    }

    void LateUpdate()
    {
        if (target)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Mathf.Pow(Mathf.Clamp(distance,1f,distanceMax), 1f / 3f) * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            wheel = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;

            Vector3 wdist = new Vector3(0.0f, 0.0f, -lastWheelDistance);
            Vector3 wpos = rotation * wdist + new Vector3(target.position.x, target.position.y + 0.2f, target.position.z);
            bool canWheel = !Physics.Linecast(target.position, wpos);
            if (canWheel)
                distance = Mathf.Clamp(distance - wheel, distanceMin, distanceMax);

            if (wheel != 0f && canWheel)
                lastWheelDistance = distance;

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
                distance = (distance < distanceMin ? distanceMin : distance);
            }
            else if(distance != lastWheelDistance)
            {
                Vector3 ndist = new Vector3(0.0f, 0.0f, -lastWheelDistance);
                Vector3 pos = rotation * ndist + new Vector3(target.position.x, target.position.y + 0.2f, target.position.z);
                if (!Physics.Linecast(target.position,pos))
                    distance = lastWheelDistance;
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + new Vector3(target.position.x, target.position.y + 0.2f, target.position.z);

            transform.rotation = rotation;
            transform.position = position;
            // Cameraman en difficulte
            //transform.position = Vector3.Lerp(transform.position, position, 5f * Time.deltaTime);
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
