using System.Collections;
using UnityEngine;

public class CameraShaking : MonoBehaviour {

    private bool isShaking = false;
    private float baseX, baseY, baseZ;
    private float intensity;
    private int shakes = 0;

    void Start()
    {
        baseX = transform.position.x;
        baseY = transform.position.y;
        baseZ = transform.position.z;

        intensity = 0.1f;
    }

    void Update()
    {
        if (isShaking)
        {
            float randomShakeX = Random.Range(-intensity, intensity);
            float randomShakeY = Random.Range(-intensity, intensity);
            transform.position = new Vector3(baseX + randomShakeX, baseY + randomShakeY, baseZ);

            shakes--;

            if (shakes <= 0)
            {
                isShaking = false;
                transform.position = new Vector3(baseX, baseY, transform.position.z);
            }
        }
    }

    public void Shake(float in_intensity)
    {
        isShaking = true;
        shakes = 100;
        intensity = in_intensity;
    }

    public void SetOriginTransform(Transform t)
    {
        baseX = t.position.x;
        baseY = t.position.y;
        baseZ = t.position.z;
    }
}
