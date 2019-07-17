using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Replaces PlayerController / CustomPhysics / PreviewLine
// Works offline and removes all parts related to networking
public class OfflineBallController : MonoBehaviour
{

    public LineRenderer line;
    public ParticleSystem trail;
    public Slider slider;
    public Transform sphere;
    public TestMode testMode;

    private BallPhysics physics;

    public float lineLength = 0.85f;

    [Header("UI")]
    public Text shotsText;
    public Text timeText;
    public GameObject validationQuitPanel;


    private bool isMoving = false;
    private bool isShooting = false;
    private bool slideUp = true;
    private Vector3 lastPos = Vector3.zero;
    private int minSliderVal = 10;
    private int maxSliderVal = 150;

    private int shots = 0;
    private float timer = 0f;

    private bool flagEnableTrail = false;


    // Handling reset of position when out-of-bounds
    private float oobInitialResetTimer = 2.0f;
    private float oobActualResetTimer;
    private bool isOOB = false;


    // Use this for initialization
    void Start()
    {
        physics = GetComponent<BallPhysics>();
    }

    void OnEnable()
    {
        shotsText.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (shotsText.gameObject != null)
            shotsText.gameObject.SetActive(false);
        if (timeText.gameObject != null)
            timeText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        shotsText.text = shotsText.text.Split(':')[0] + ": " + shots;
        timeText.text = timeText.text.Split(':')[0] + ": " + timer.ToString("0.##") + "s";

        if (testMode.IsInValidation())
            testMode.CheckTime(timer, shots);

        if (flagEnableTrail)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = true;
            flagEnableTrail = false;
        }

        isMoving = physics.velocityCapped.magnitude >= 0.001f;

        if (!isMoving)
        {
            lastPos = transform.position;

            if (testMode.IsInValidation())
                testMode.CheckShots(shots, timer);

            if (!line.enabled)
                line.enabled = true;

            Vector3 dir = transform.position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0f, dir.z).normalized;

            line.SetPosition(0, transform.position);
            line.SetPosition(1, dir * lineLength + transform.position);

            if (Input.GetMouseButtonDown(0) && !validationQuitPanel.activeSelf)
            {
                if (isShooting)
                {
                    shots++;
                    physics.AddForce(dir * Mathf.Pow(slider.value, 1.4f) * 2f);
                    isShooting = false;
                    isMoving = true;
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
                physics.StopBall();
                transform.position = lastPos;
                flagEnableTrail = true;
            }
        }


        if (physics.velocityCapped.magnitude > 0.005f)
            sphere.Rotate(new Vector3(physics.velocityCapped.z * 10f, 0f, -physics.velocityCapped.x * 10f), Space.World);

        timer += Time.deltaTime;


        RaycastHit oobHit;
        Debug.DrawRay(transform.position, Vector3.down * 100, Color.red, 1f);
        if (Physics.Raycast(transform.position, Vector3.down, out oobHit, Mathf.Infinity, 1 << EditorManager.layerFloor | 1 << EditorManager.layerWall))
        {
            if (oobHit.collider.gameObject.CompareTag("Hole " + (EditorManager._instance.GetCurrentHoleNumber() + 1)))
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
                        physics.StopBall();
                        transform.position = lastPos;
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
            // Instant reset, we should always have something below us (the plane at least)
            isOOB = false;
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = false;
            physics.StopBall();
            transform.position = lastPos;
            flagEnableTrail = true;
            Debug.LogError("Void below us, is it ok ?");
        }
    }


    void OnTriggerEnter(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.layer == LayerMask.NameToLayer("Hole"))
        {
            testMode.TestHole(false);
            int saveShots = shots;
            float saveTimer = timer;
            ResetTest();
            testMode.EndOfTest(saveShots, saveTimer);
        }
    }

    public void OnBoosterPad(Vector3 dir, float multFactor, float addFactor)
    {
        //Debug.Log("OnBoosterPad : mult = " + multFactor + ", add = " + addFactor);
        float angle = Vector3.Angle(physics.velocityCapped, dir);
        physics.MultiplySpeed(multFactor * (angle > 90f ? -0.1f : 1f));
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
        physics.StopBall();

        ResetSlider();
        gameObject.SetActive(false);
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
