using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class CustomPhysics : NetworkBehaviour {

    public Rigidbody rb;
    public Transform sphere;

    private int i;
    private Vector3 lastWallHit;
    private int frameHit;

    //private static float gravity = 9.81f;
    Quaternion serverRota = Quaternion.identity;

	// Use this for initialization
	void Start () {
        i = frameHit =  0;
        lastWallHit = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
	}

    void Update()
    {
        //Debug.Log("Velocity = " + rb.velocity + ", magnitude = " + rb.velocity.magnitude);

        // Rotate the ball if we are moving
        if (isServer)
        {
            if (rb.velocity.magnitude > 0.005f)
                sphere.Rotate(new Vector3(rb.velocity.z * 10f, 0f, -rb.velocity.x * 10f), Space.World);
            RpcRotateBall(sphere.rotation);
        }
    }


    [ClientRpc]
    void RpcRotateBall(Quaternion rota)
    {
        //sphere.transform.rotation = rota;
        serverRota = rota;
    }

	
	void FixedUpdate()
    {
        if (!isServer)
        {
            sphere.transform.rotation = Quaternion.Slerp(sphere.transform.rotation, serverRota, 15f * Time.deltaTime);
            return;
        }

        Vector3 forwardBallSize = new Vector3(rb.velocity.normalized.x, 0f, rb.velocity.normalized.z) / 20f;
        Vector3 nextPosForward = new Vector3(rb.velocity.x, 0f, rb.velocity.z) * Time.fixedDeltaTime;

        Vector3 bestForwardCheck = (nextPosForward.magnitude > forwardBallSize.magnitude) ? nextPosForward : forwardBallSize;

        Vector3 downwardBallSize = new Vector3(0f, rb.velocity.normalized.y, 0f) / 20f;
        Vector3 nextPosDownward = new Vector3(0f, rb.velocity.y, 0f) * Time.fixedDeltaTime;

        Vector3 bestDownwardCheck = (nextPosForward.magnitude > forwardBallSize.magnitude) ? nextPosDownward : downwardBallSize;


        Vector3 topRightPos = new Vector3(bestForwardCheck.x + (rb.velocity.normalized.z / 20f), 0, bestForwardCheck.z - (rb.velocity.normalized.x / 20f));
        Vector3 topLeftPos = new Vector3(bestForwardCheck.x - (rb.velocity.normalized.z / 20f), 0, bestForwardCheck.z + (rb.velocity.normalized.x / 20f));

        /*// Forward
        Debug.DrawLine(transform.position, transform.position + bestForwardCheck, Color.red, 10f);
        // Right
        Debug.DrawLine(transform.position, transform.position + (new Vector3(rb.velocity.normalized.z, 0, -rb.velocity.normalized.x)) / 20f, Color.blue, 10f);
        // Left
        Debug.DrawLine(transform.position, transform.position + (new Vector3(-rb.velocity.normalized.z, 0, rb.velocity.normalized.x)) / 20f, Color.yellow, 10f);
        // TopRight
        Debug.DrawLine(transform.position, transform.position + topRightPos, Color.green, 10f);
        // TopLeft
        Debug.DrawLine(transform.position, transform.position + topLeftPos, Color.grey, 10f);*/

        
        RaycastHit hitForward; 
        RaycastHit hitRight;
        RaycastHit hitLeft;
        RaycastHit hitTopRight;
        RaycastHit hitTopLeft;


        bool forward = Physics.Linecast(transform.position, transform.position + bestForwardCheck, out hitForward, 1 << 8);
        bool right = Physics.Linecast(transform.position, transform.position + (new Vector3(rb.velocity.normalized.z, 0, -rb.velocity.normalized.x)) / 20f, out hitRight, 1 << 8);
        bool left = Physics.Linecast(transform.position, transform.position + (new Vector3(-rb.velocity.normalized.z, 0, rb.velocity.normalized.x)) / 20f, out hitLeft, 1 << 8);
        bool topRight = Physics.Linecast(transform.position, transform.position + topRightPos, out hitTopRight, 1 << 8);
        bool topLeft = Physics.Linecast(transform.position, transform.position + topLeftPos, out hitTopLeft, 1 << 8);

        bool collision = forward || right || left || topRight || topLeft;

        // Find all unique walls collided
        if (collision)
        {
            /*
            if (forward)
                Debug.Log("Frame = " + i + ", hit forward");
            if (left)
                Debug.Log("Frame = " + i + ", hit left");
            if (right)
                Debug.Log("Frame = " + i + ", hit right");
            if (topRight)
                Debug.Log("Frame = " + i + ", hit topRight");
            if(topLeft)
                Debug.Log("Frame = " + i + ", hit topLeft");
            */
            int nbWallsHit = 0;
            RaycastHit[] walls = new RaycastHit[5];
            if (forward)
            {
                walls[nbWallsHit] = hitForward;
                nbWallsHit++;
            }
            if (right)
            {
                //Debug.Log("NbWallsHit = " + nbWallsHit + ", right");

                if ((nbWallsHit == 0) || (hitRight.transform.position != walls[nbWallsHit - 1].transform.position && hitRight.normal != walls[nbWallsHit - 1].normal))
                {
                    walls[nbWallsHit] = hitRight;
                    nbWallsHit++;
                }
            }
            if (left)
            {
                //Debug.Log("NbWallsHit = " + nbWallsHit + ", left");

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
                //Debug.Log("NbWallsHit = " + nbWallsHit + ", topRight");

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
                //Debug.Log("NbWallsHit = " + nbWallsHit + ", topLeft");

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

                    //Debug.Log("Frame = " + i + ",Dir = " + dir + ", WallDir = " + wallDir + ", res = " + res);

                    rb.velocity = res;

                    lastWallHit = walls[k].transform.position;
                    frameHit = i;
                }
            }
        }
        else // We check downward collisions only if we don't have any other collisions
        {
            // Downward
            //Debug.DrawLine(transform.position, transform.position + bestDownwardCheck, Color.black, 10f);

            RaycastHit hitDownward;
            bool downward = Physics.Linecast(transform.position, transform.position + bestDownwardCheck, out hitDownward, 1 << 8);

            if(downward)
            {
                Vector3 dir = rb.velocity;
                Vector3 wallDir = hitDownward.normal;

                Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);

                //Debug.Log("Frame = " + i + ",Dir = " + dir + ", WallDir = " + wallDir + ", res = " + res);

                rb.velocity = res;
            }
        }

        if (rb.velocity.magnitude < 0.005f)
            rb.velocity = Vector3.zero;
        rb.velocity = rb.velocity * 0.99f;

        //Vector3 grav = new Vector3(0, -1.622f, 0);

        rb.AddForce(Physics.gravity/*grav*/);

        if(frameHit < i)
            lastWallHit = new Vector3(-Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

        i++;
    }
}
