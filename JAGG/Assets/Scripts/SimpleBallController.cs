﻿using System.Collections;
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


    // Use this for initialization
    void Start()
    {
        initialPos = transform.position;
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

        isMoving = rb.velocity.magnitude >= 0.001f;

        if (!isMoving)
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
                    rb.AddForce(dir * Mathf.Pow(slider.value, 1.4f) * 2f);
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
                rb.velocity = Vector3.zero;
                transform.position = lastPos;
                flagEnableTrail = true;
            }
        }


        if (rb.velocity.magnitude > 0.005f)
            sphere.Rotate(new Vector3(rb.velocity.z * 10f, 0f, -rb.velocity.x * 10f), Space.World);

        timer += Time.deltaTime;


        if (!Physics.Raycast(transform.position, Vector3.down, Mathf.Infinity, ~(1 << 0/*layerDecor*/)))
        {
            if (isOOB)
            {
                Debug.Log("OOB, time left before reset : " + oobActualResetTimer);
                oobActualResetTimer -= Time.deltaTime;
                if (oobActualResetTimer < 0f)
                {
                    isOOB = false;
                    ParticleSystem.EmissionModule em = trail.emission;
                    em.enabled = false;
                    rb.velocity = Vector3.zero;
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
        else
        {
            isOOB = false;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.CompareTag("Hole"))
        {
            /*int saveShots = shots;
            float saveTimer = timer;*/
            ResetTest();
        }
    }

    public void OnBoosterPad(Vector3 dir, float multFactor, float addFactor)
    {
        float angle = Vector3.Angle(rb.velocity, dir);
        rb.velocity *= multFactor * (angle > 90f ? -0.1f : 1f);
        rb.AddForce(dir * addFactor);
    }

    public void InWindArea(float strength, Vector3 direction)
    {
        rb.AddForce(direction * strength);
    }

    public void ResetTest()
    {
        shots = 0;
        timer = 0f;
        rb.velocity = Vector3.zero;

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
