using UnityEngine;
using System.Collections;
 
public class BallCamera : MonoBehaviour
{

    public Transform target;

    public bool doZoomOnCollide = true;

    public float distance = 5.0f;

    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float xSpeedAccurate = 12.0f;
    public float ySpeedAccurate = 12.0f;

    private static float defaultXSpeed = 120.0f;
    private static float defaultYSpeed = 120.0f;
    private static float defaultXSpeedAcc = 12.0f;
    private static float defaultYSpeedAcc = 12.0f;

    public float wheelSpeed = 5f;

    private bool isAccurateMode = false;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;
    float wheel = 0.0f;
    float lastWheelDistance;


    public float shakingTime = 0.75f;
    private float originalShakingtime;
    public bool isShaking = false;

    public bool shouldFollow = true;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        originalShakingtime = shakingTime;

        lastWheelDistance = distance;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            isAccurateMode = !isAccurateMode;
        }

        xSpeed = defaultXSpeed * rescale(SettingsManager._instance.gameSettings.Sensibility);
        ySpeed = defaultYSpeed * rescale(SettingsManager._instance.gameSettings.Sensibility);
        xSpeedAccurate = defaultXSpeedAcc * rescale(SettingsManager._instance.gameSettings.AccurateSensibility);
        ySpeedAccurate = defaultYSpeedAcc * rescale(SettingsManager._instance.gameSettings.AccurateSensibility);
    }

    private float rescale(float sens, float min = 0f, float max = 100f, float minAfterRescale = 0.5f, float maxAfterRescale = 1.5f)
    {
        return ( ((maxAfterRescale - minAfterRescale) * (sens - min)) / (max - min) ) + minAfterRescale;
    }

    void LateUpdate()
    {
        if (target && shouldFollow)
        {
            float trueXSpeed = (isAccurateMode) ? xSpeedAccurate : xSpeed;
            float trueYSpeed = (isAccurateMode) ? ySpeedAccurate : ySpeed;

            x += Input.GetAxis("Mouse X") * trueXSpeed * Mathf.Pow(Mathf.Clamp(distance,1f,distanceMax), 1f / 3f) * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * trueYSpeed * Time.deltaTime;

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
            if (doZoomOnCollide && Physics.Linecast(target.position, transform.position, out hit))
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

            if (isShaking)
            {
                doZoomOnCollide = false;
                float randomShakeX = Random.Range(-0.25f, 0.25f);
                float randomShakeY = Random.Range(-0.25f, 0.25f);
                transform.position = new Vector3(position.x + randomShakeX, position.y + randomShakeY, position.z);

                shakingTime -= Time.deltaTime;

                if (shakingTime <= 0)
                {
                    isShaking = false;
                    shakingTime = originalShakingtime;
                    doZoomOnCollide = true;
                }
            }
            else
            {
                transform.position = position;
            }
            transform.rotation = rotation;
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
