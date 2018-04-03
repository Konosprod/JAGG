using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Replaces PlayerController / CustomPhysics / PreviewLine
// Works offline and removes all parts related to networking
public class OfflineBallController : MonoBehaviour {

    public LineRenderer line;
    public Rigidbody rb;
    public ParticleSystem trail;
    public Slider slider;
    public Transform sphere;
    public TestMode testMode;

    public float lineLength = 0.85f;

    
    private bool isMoving = false;
    private bool isShooting = false;
    private bool slideUp = true;
    private Vector3 lastPos = Vector3.zero;
    private int minSliderVal = 10;
    private int maxSliderVal = 150;

    private bool IsGrounded;
    private int i;
    private Vector3 lastWallHit;
    private int frameHit;
    private int layerWall;


    private int shots = 0;
    private float timer = 0f;

    private bool flagEnableTrail = false;


    // Use this for initialization
    void Start () {
        layerWall = LayerMask.NameToLayer("Wall");
        i = frameHit = 0;
        lastWallHit = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    }
	
	// Update is called once per frame
	void Update () {

        if(flagEnableTrail)
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
                    rb.AddForce(dir * slider.value * 10f);
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
    }

    
    void OnTriggerEnter(Collider other)
    {
        GameObject otherGO = other.gameObject;
        if (otherGO.CompareTag("Hole"))
        {
            testMode.TestHole(false);
            testMode.EndOfTest(shots, timer);
            resetTest();
        }
        else if (LayerMask.LayerToName(otherGO.layer) == "BoosterPad")
        {
            Vector3 dir = otherGO.transform.forward.normalized;
            BoosterPad bp = otherGO.GetComponent<BoosterPad>();
            float multFactor = bp.multFactor;
            float addFactor = bp.addFactor;

            float angle = Vector3.Angle(rb.velocity, dir);
            rb.velocity *= multFactor * (angle > 90f ? -0.1f : 1f);
            rb.AddForce(dir * addFactor);
        }
        else
        {
            Debug.LogError("Ball entered unexpected trigger : " + other.gameObject.name);
        }
    }



    void OnCollisionStay(Collision collisionInfo)
    {

        IsGrounded = true;


        GameObject piece = collisionInfo.gameObject;
        TerrainPiece tp = piece.GetComponent<TerrainPiece>();
        while(tp == null)
        {
            piece = piece.transform.parent.gameObject;
            tp = piece.GetComponent<TerrainPiece>();
        }
        
        RotatePiece rtp = piece.GetComponent<RotatePiece>();

        if (rtp != null && rtp.enabled)
        {
            transform.parent = collisionInfo.transform.parent;
        }
    }

    void OnCollisionExit(Collision collisionInfo)
    {
        IsGrounded = false;

        if (transform.parent != null)
        {
            transform.parent = null;

        }
    }

    /*void OnTriggerStay(Collider other)
    {

        if (other.transform.parent.gameObject.GetComponent<RotatePiece>()!=null)
        {
            transform.parent = other.transform.parent;

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (transform.parent != null)
        {
            transform.parent = null;

        }
    }*/


    public void resetTest()
    {
        shots = 0;
        timer = 0f;
        rb.velocity = Vector3.zero;
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


        Vector3 forwardBallSize = new Vector3(rb.velocity.normalized.x, 0f, rb.velocity.normalized.z) / 20f;
        Vector3 nextPosForward = new Vector3(rb.velocity.x, 0f, rb.velocity.z) * Time.fixedDeltaTime;

        Vector3 bestForwardCheck = (nextPosForward.magnitude > forwardBallSize.magnitude) ? nextPosForward : forwardBallSize;

        Vector3 downwardBallSize = new Vector3(0f, rb.velocity.normalized.y, 0f) / 20f;
        Vector3 nextPosDownward = new Vector3(0f, rb.velocity.y, 0f) * Time.fixedDeltaTime;

        Vector3 bestDownwardCheck = (nextPosForward.magnitude > forwardBallSize.magnitude) ? nextPosDownward : downwardBallSize;


        Vector3 topRightPos = new Vector3(bestForwardCheck.x + (rb.velocity.normalized.z / 20f), 0, bestForwardCheck.z - (rb.velocity.normalized.x / 20f));
        Vector3 topLeftPos = new Vector3(bestForwardCheck.x - (rb.velocity.normalized.z / 20f), 0, bestForwardCheck.z + (rb.velocity.normalized.x / 20f));


        RaycastHit hitForward;
        RaycastHit hitRight;
        RaycastHit hitLeft;
        RaycastHit hitTopRight;
        RaycastHit hitTopLeft;


        bool forward = Physics.Linecast(transform.position, transform.position + bestForwardCheck, out hitForward, 1 << layerWall);
        bool right = Physics.Linecast(transform.position, transform.position + (new Vector3(rb.velocity.normalized.z, 0, -rb.velocity.normalized.x)) / 20f, out hitRight, 1 << layerWall);
        bool left = Physics.Linecast(transform.position, transform.position + (new Vector3(-rb.velocity.normalized.z, 0, rb.velocity.normalized.x)) / 20f, out hitLeft, 1 << layerWall);
        bool topRight = Physics.Linecast(transform.position, transform.position + topRightPos, out hitTopRight, 1 << layerWall);
        bool topLeft = Physics.Linecast(transform.position, transform.position + topLeftPos, out hitTopLeft, 1 << layerWall);

        bool collision = forward || right || left || topRight || topLeft;

        // Find all unique walls collided
        if (collision)
        {
            int nbWallsHit = 0;
            RaycastHit[] walls = new RaycastHit[5];
            if (forward)
            {
                walls[nbWallsHit] = hitForward;
                nbWallsHit++;
            }
            if (right)
            {
                if ((nbWallsHit == 0) || (hitRight.transform.position != walls[nbWallsHit - 1].transform.position && hitRight.normal != walls[nbWallsHit - 1].normal))
                {
                    walls[nbWallsHit] = hitRight;
                    nbWallsHit++;
                }
            }
            if (left)
            {
                if (nbWallsHit == 0)
                {
                    walls[nbWallsHit] = hitLeft;
                    nbWallsHit++;
                }
                else
                {
                    int k = 0;
                    bool newWall = true;

                    while (newWall && k < nbWallsHit)
                    {
                        if (hitLeft.transform.position == walls[k].transform.position || hitLeft.normal == walls[k].normal)
                        {
                            newWall = false;
                        }

                        k++;
                    }

                    if (newWall)
                    {
                        walls[nbWallsHit] = hitLeft;
                        nbWallsHit++;
                    }
                }
            }
            if (topRight)
            {
                if (nbWallsHit == 0)
                {
                    walls[nbWallsHit] = hitTopRight;
                    nbWallsHit++;
                }
                else
                {
                    int k = 0;
                    bool newWall = true;

                    while (newWall && k < nbWallsHit)
                    {
                        if (hitTopRight.transform.position == walls[k].transform.position || hitTopRight.normal == walls[k].normal)
                        {
                            newWall = false;
                        }

                        k++;
                    }

                    if (newWall)
                    {
                        walls[nbWallsHit] = hitTopRight;
                        nbWallsHit++;
                    }
                }
            }
            if (topLeft)
            {
                if (nbWallsHit == 0)
                {
                    walls[nbWallsHit] = hitTopLeft;
                    nbWallsHit++;
                }
                else
                {
                    int k = 0;
                    bool newWall = true;

                    while (newWall && k < nbWallsHit)
                    {
                        if (hitTopLeft.transform.position == walls[k].transform.position || hitTopLeft.normal == walls[k].normal)
                        {
                            newWall = false;
                        }

                        k++;
                    }

                    if (newWall)
                    {
                        walls[nbWallsHit] = hitTopLeft;
                        nbWallsHit++;
                    }
                }
            }

            // Compute all unique collisions
            for (int k = 0; k < nbWallsHit; k++)
            {
                if (walls[k].transform.position != lastWallHit)
                {
                    Vector3 dir = rb.velocity;
                    Vector3 wallDir = walls[k].normal;

                    Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);

                    rb.velocity = res;

                    lastWallHit = walls[k].transform.position;
                    frameHit = i;
                }
            }
        }
        else // We check downward collisions only if we don't have any other collisions
        {
            RaycastHit hitDownward;
            bool downward = Physics.Linecast(transform.position, transform.position + bestDownwardCheck, out hitDownward, 1 << layerWall);

            if (downward)
            {
                Vector3 dir = rb.velocity;
                Vector3 wallDir = hitDownward.normal;

                Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);
                rb.velocity = res;
            }
        }

        Vector3 position = transform.position;
        position.y = GetComponent<Collider>().bounds.min.y + 0.02f;
        float length = 0.03f;
        Debug.DrawRay(position, Vector3.down * length);
        RaycastHit test;
        bool grounded = Physics.Linecast(position, position + (Vector3.down * length), out test);

        bool onEvenGround = false;

        if (grounded)
            onEvenGround = test.normal == Vector3.up;

        float stopSpeedThreshold = 0.1f;
        float unevenGroundstopSpeedThreshold = 0.01f;

        if (IsGrounded || grounded)
        {
            // Check if we should stop when grounded
            if (rb.velocity.magnitude < (onEvenGround ? stopSpeedThreshold : unevenGroundstopSpeedThreshold))
                rb.velocity = Vector3.zero;
        }

        // Slow down the ball
        rb.velocity = rb.velocity * 0.99f;

        if (!onEvenGround)
        {
            rb.AddForce(Physics.gravity);
        }

        if (frameHit < i)
            lastWallHit = new Vector3(-Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        i++;
    }


}
