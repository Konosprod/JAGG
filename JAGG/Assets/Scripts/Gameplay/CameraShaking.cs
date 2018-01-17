using UnityEngine;

public class CameraShaking : MonoBehaviour {

    public float power = 0.7f;
    public float duration = 1.0f;
    public Transform camera;
    public float slowDownAmount = 1.0f;
    public bool shouldShake = false;

    private Vector3 startPosition;
    float initialDuration;

	// Use this for initialization
	void Start () {

        camera = Camera.main.transform;
        initialDuration = duration;
        startPosition = camera.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		
        if(shouldShake)
        {
            if(duration > 0)
            {
                camera.localPosition = startPosition + Random.insideUnitSphere * power;
                duration -= Time.deltaTime;
            }
            else
            {
                shouldShake = false;
                duration = initialDuration;
                camera.localPosition = startPosition;
            }
        }

	}
}
