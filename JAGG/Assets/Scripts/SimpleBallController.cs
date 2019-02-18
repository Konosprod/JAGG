using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleBallController : MonoBehaviour
{

    public LineRenderer line;
    public Rigidbody rb;
    public ParticleSystem trail;
    public Slider slider;
    public Transform sphere;

    public float lineLength = 0.85f;

    [Header("UI")]
    public Text shotsText;
    public Text timeText;


    private bool isMoving = false;
    private bool isShooting = false;
    private bool slideUp = true;
    private Vector3 lastPos = Vector3.zero;
    private const int minSliderVal = 10;
    private const int maxSliderVal = 150;

    private int shots = 0;
    private float timer = 0f;

    private bool flagEnableTrail = false;


    // Handling reset of position when out-of-bounds
    private const float oobInitialResetTimer = 2.0f;
    private float oobActualResetTimer;
    private bool isOOB = false;

    private Vector3 initialPos;


    private ReplayObject replay;
    public bool canShoot = true;

    private BallPhysicsTest physics;


    // Use this for initialization
    void Start()
    {
        initialPos = transform.position;
        replay = gameObject.AddComponent<ReplayObject>();
        physics = GetComponent<BallPhysicsTest>();
    }

    /*void OnEnable()
    {
        shotsText.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        shotsText.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
    }*/

    // Update is called once per frame
    void Update()
    {
        shotsText.text = shotsText.text.Split(':')[0] + ": " + shots;
        timeText.text = timeText.text.Split(':')[0] + ": " + timer.ToString("0.##") + "s";

        if (flagEnableTrail)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = true;
            flagEnableTrail = false;
        }

        isMoving = physics.velocityCapped.magnitude >= 0.001f;

        if (!isMoving && canShoot)
        {
            lastPos = transform.position;

            if (!line.enabled)
                line.enabled = true;

            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;

            line.SetPosition(0, transform.position);
            line.SetPosition(1, dir * lineLength + transform.position);

            if (Input.GetMouseButtonDown(0))
            {
                if (isShooting)
                {
                    shots++;
                    physics.AddForce(dir * Mathf.Pow(slider.value, 1.4f) * 2f);
                    isShooting = false;
                    isMoving = true;

                    replay.AddInput(dir, slider.value, transform.position);

                    ResetSlider();
                }
                else
                    isShooting = true;
            }

            if (Input.GetMouseButtonDown(1) && isShooting)
            {
                isShooting = false;
                ResetSlider();
            }
        }
        else
        {
            line.enabled = false;

            if (Input.GetKeyDown(KeyCode.R) && lastPos != Vector3.zero)
            {
                ParticleSystem.EmissionModule em = trail.emission;
                em.enabled = false;
                physics.velocityCapped = Vector3.zero;
                transform.position = lastPos;

                replay.AddInput(Vector3.zero, -2f, lastPos);

                flagEnableTrail = true;
            }
        }


        if (physics.velocityCapped.magnitude > 0.005f)
            sphere.Rotate(new Vector3(physics.velocityCapped.z * 10f, 0f, -physics.velocityCapped.x * 10f), Space.World);

        timer += Time.deltaTime;


        RaycastHit oobHit;
        if (Physics.Raycast(transform.position, Vector3.down, out oobHit, Mathf.Infinity, 1 << BallPhysicsNetwork.layerFloor | 1 << BallPhysicsNetwork.layerWall)) // TODO : redo OOB system entirely
        {
            if (oobHit.collider.gameObject.CompareTag("Hole 1"))
            {
                isOOB = false;
            }
            else
            {
                if (isOOB)
                {
                    //Debug.Log("OOB, time left before reset : " + oobActualResetTimer);
                    oobActualResetTimer -= Time.deltaTime;
                    if (oobActualResetTimer < 0f)
                    {
                        isOOB = false;
                        ParticleSystem.EmissionModule em = trail.emission;
                        em.enabled = false;
                        physics.velocityCapped = Vector3.zero;
                        transform.position = lastPos;

                        replay.AddInput(Vector3.zero, -2f, lastPos);

                        flagEnableTrail = true;
                    }
                }
                else
                {
                    isOOB = true;
                    oobActualResetTimer = oobInitialResetTimer;
                }
            }
        }
        else
        {
            isOOB = true;
            Debug.LogError("Void below us, is it ok ?");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.CompareTag("Hole"))
        {
            /*int saveShots = shots;
            float saveTimer = timer;*/
            canShoot = false;
            replay.AddInput(Vector3.zero, -1f, transform.position);
            ReplayManager._instance.StartReplay();
            ResetTest();
        }
    }

    public void OnBoosterPad(Vector3 dir, float multFactor, float addFactor)
    {
        float angle = Vector3.Angle(physics.velocityCapped, dir);
        physics.velocityCapped *= multFactor * (angle > 90f ? -0.1f : 1f);
        physics.AddForce(dir * addFactor);
    }

    public void InWindArea(float strength, Vector3 direction)
    {
        physics.AddForce(direction * strength);
    }

    public void ResetTest()
    {
        shots = 0;
        timer = 0f;
        physics.velocityCapped = Vector3.zero;

        transform.position = initialPos;

        ResetSlider();
    }

    private void UpdateSlider()
    {
        if (slideUp)
            slider.value += 1;
        else
            slider.value -= 1;


        // Start moving the other way when we reach either end otherwise keep moving in the same direction
        slideUp = (slider.value >= maxSliderVal) ? false : (slider.value <= minSliderVal) ? true : slideUp;
    }

    private void ResetSlider()
    {
        slideUp = true;
        slider.value = minSliderVal;
    }


    void FixedUpdate()
    {
        if (isShooting)
            UpdateSlider();
    }
}
