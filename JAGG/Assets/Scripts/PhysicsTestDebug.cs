using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTestDebug : MonoBehaviour {

    private Rigidbody rb;
    public ParticleSystem trail;


    [Header("Simulation properties")]
    public Vector3 shootDirection;
    public float forceOfShot;
    public float timerRestart; // In seconds


    [Header("Displays")]
    public float currentTimer;
    public float velocityMagnitude;
    public Vector3 velocity;



    private Vector3 startPos;
    private bool flagEnableTrail = false;


    private int layerFloor/*,
                layerWall*/;

    void Awake()
    {
        startPos = transform.position;
        currentTimer = timerRestart;
        rb = GetComponent<Rigidbody>();
        layerFloor = LayerMask.NameToLayer("Floor");
        //layerWall = LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        if (flagEnableTrail)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = true;
            flagEnableTrail = false;
        }

        velocity = rb.velocity;
        velocityMagnitude = rb.velocity.magnitude;

        if (currentTimer == timerRestart)
        {
            rb.AddForce(shootDirection.normalized * forceOfShot);
        }

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            ParticleSystem.EmissionModule em = trail.emission;
            em.enabled = false;
            rb.velocity = Vector3.zero;
            transform.position = startPos;
            //Debug.Log("Startpos : " + startPos + ", transform.position : " + transform.position);
            currentTimer = timerRestart;
            flagEnableTrail = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 point = contact.point;
            Vector3 normal = contact.normal;

            if (contact.otherCollider.gameObject.layer == layerFloor)
            {
                if (/*contact.otherCollider.transform*/Vector3.up != normal)
                {
                    Debug.DrawRay(point, normal, Color.red, 3f);
                    //Debug.Log(normal);
                    //Debug.Break();
                }
            }
            else
            {
                //Debug.Log(collision.gameObject.name);
                //Debug.DrawRay(point, normal, Color.cyan, 1f);
            }
        }
    }
}
