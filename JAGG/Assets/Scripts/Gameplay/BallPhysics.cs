using System.Collections.Generic;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{

    private Rigidbody rb;

    private int layerFloor;
    private int layerWall;


    public bool squareDrag = false;

    private bool stable = false; // The ball is on the floor, not on a slope / mid-air
    private Vector3 currentFloorNormal = Vector3.up;
    
    // Hack for the trail on high-speed wall collisions
    private bool flagFixPos = false;
    private Vector3 fixedPos;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
    }

    void Update()
    {


        if(flagFixPos)
        {
            transform.position = fixedPos;
            flagFixPos = false;
        }
    }

    void FixedUpdate()
    {

        // Handle collisions with walls
        RaycastHit rayWallHit = CheckMovementWallCollision(transform.position, rb.velocity);
        if (rayWallHit.collider != null)
        {
            //Debug.Log("A wall : " + rayWallHit.collider.gameObject.name);

            // We calculate the perfect bounce angle with the wall
            Vector3 dir = rb.velocity;
            Vector3 wallDir = rayWallHit.normal;

            // New velocity direction
            Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);

            // We also have to adjust the position of ball
            // At low speeds the position adjustment looks really weird/clunky so we don't do it
            if (rb.velocity.magnitude > 2f)
                fixedPos = rayWallHit.point + wallDir * 0.055f;
            else
                fixedPos = transform.position;
            

            // We must check if we aren't in a corner and risk going into a wall
            Collider otherWallCol;
            RaycastHit otherWallHit = CheckMovementWallCollision(fixedPos, res);
            otherWallCol = otherWallHit.collider;
            int _pasDeBoucleInfiniPourAlex = 0;
            while (otherWallCol != null && _pasDeBoucleInfiniPourAlex < Random.Range(3, 6))
            {
                res = res - 2f * (Vector3.Dot(res, otherWallHit.normal) * otherWallHit.normal);
                //transform.position = otherWallHit.point + otherWallHit.normal * 0.055f;
                if (rb.velocity.magnitude > 2f)
                    fixedPos = otherWallHit.point + otherWallHit.normal * 0.055f;

                otherWallHit = CheckMovementWallCollision(fixedPos, res);
                otherWallCol = otherWallHit.collider;

                _pasDeBoucleInfiniPourAlex++;
            }

            flagFixPos = true;

            rb.velocity = res;
        }
        else
        {
            // Check on the left and right of the ball

            // Right first because why not
            float angle = Vector3.Angle(Vector3.forward, rb.velocity);

            Vector3 start = transform.position + rb.velocity * Time.fixedDeltaTime;
            Vector3 end = start + transform.right * 0.055f;

            //Debug.DrawRay(start, end - start);

            Physics.Linecast(start, end, out rayWallHit, (1 << layerWall));
            if (rayWallHit.collider != null)
            {
                //Debug.Log("A wall : " + rayWallHit.collider.gameObject.name);

                // We calculate the perfect bounce angle with the wall
                Vector3 dir = rb.velocity;
                Vector3 wallDir = rayWallHit.normal;

                // New velocity direction
                Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);
                rb.velocity = res;
            }
            else
            {
                // Left check
                start = transform.position + rb.velocity * Time.fixedDeltaTime;
                end = start - transform.right * 0.055f;

                //Debug.DrawRay(start, end - start);

                Physics.Linecast(start, end, out rayWallHit, (1 << layerWall));
                if (rayWallHit.collider != null)
                {
                    //Debug.Log("A wall : " + rayWallHit.collider.gameObject.name);

                    // We calculate the perfect bounce angle with the wall
                    Vector3 dir = rb.velocity;
                    Vector3 wallDir = rayWallHit.normal;

                    // New velocity direction
                    Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);
                    rb.velocity = res;
                }
            }

        }


        // Stay on the floor
        // Apply gravity when needed
        Vector3 position = transform.position;
        float length = 0.051f; // 0.05f is the ball radius + 0.001f because I'm an engineer
        //Debug.DrawRay(position, -currentFloorNormal * length, Color.cyan, 2f);
        RaycastHit test;
        bool grounded = Physics.Linecast(position, position + (-currentFloorNormal * length), out test, 1 << layerFloor);

        stable = false;

        if (grounded)
        {
            stable = test.normal == Vector3.up;
            currentFloorNormal = test.normal;
            float distToGround = (0.05f - Vector3.Distance(transform.position, test.point));
            if (distToGround > 0.001f)
            {
                //Debug.Log("Distance to the ground : " + distToGround + ", pos.y=" + transform.position.y + ", point.y=" + test.point.y);
                //rb.AddForce(test.normal * rb.velocity.sqrMagnitude); // Find better solution
                transform.position += test.normal * distToGround; // better solution
            }
        }
        else
        {
            // We are falling, we must check if we are not going inside a floor
            currentFloorNormal = Vector3.up;


            float angle = Vector3.Angle(Vector3.forward, rb.velocity);

            Vector3 start = transform.position /*+ vel * Time.fixedDeltaTime*/ + rb.velocity.normalized * 0.05f * Mathf.Abs(Mathf.Cos(angle) + Mathf.Sin(angle));
            Vector3 end = start + rb.velocity * Time.fixedDeltaTime;

            RaycastHit floorHit;
            bool floorCheck = Physics.SphereCast(position, 0.05f, rb.velocity, out floorHit, 0.01f * Mathf.Max(1f,rb.velocity.magnitude), (1 << layerFloor));

            if (floorCheck)
            {
                // Debug.DrawRay(floorHit.point, floorHit.normal, Color.red, 2f);

                // We calculate the perfect bounce angle with the floor
                Vector3 dir = rb.velocity;
                Vector3 wallDir = floorHit.normal;

                // New velocity direction
                Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);
                res = Vector3.Lerp(dir, res, 0.7f); // Random as fuck but works

                rb.velocity = res;
            }
            else
            {
                // Double-check with overlapsphere
                Collider[] cols = Physics.OverlapSphere(position, 0.05f, (1 << layerFloor));
                if(cols.Length > 0)
                {
                    //Debug.Log("We are overlapped with " + cols.Length + " colliders, the first is " + cols[0].gameObject.name);
                    RaycastHit lineHit;
                    bool line = Physics.Linecast(transform.position, cols[0].transform.position, out lineHit, (1<<layerFloor));
                    if(line)
                    {
                        float distToOverlap = (0.05f - Vector3.Distance(transform.position, lineHit.point));
                        //Debug.Log("distToOverLap = " + distToOverlap);
                        if (distToOverlap > -0.03f)
                        {
                            transform.position += lineHit.normal * 0.025f;
                        }
                    }
                    else
                    {
                        Debug.LogError("Fuck this"); // Very unlikely to be overlapping an object but unable to find it with a raycast
                    }
                }
            }
        }

        Vector3 grav = Physics.gravity;

        if (!stable)
        {
            // Mid-air
            if (!grounded)
            {
                rb.AddForce(grav);
            }
            else
            {
                // Worst case scenario, we are on a slope
                // But everything will be daijobu desu
                Vector3 vel = rb.velocity;
                Vector3 normal = test.normal;
                Vector3 project = Vector3.ProjectOnPlane(vel, normal).normalized;
                //Debug.DrawRay(transform.position, project * 10f, Color.red, 10f);
                //Debug.Log("Velocity : " + vel + ", normal : " + normal + ", cross : " + project);

                //Debug.Break();

                // Move alongside the slope
                rb.velocity = project * rb.velocity.magnitude;

                // Apply gravity 
                // We project the gravity along the slope
                // Using the dot product we get a value that is bigger the higher the angle of the slope is
                Vector3 projectGrav = Vector3.ProjectOnPlane(grav, normal).normalized;
                float dotGrav = Vector3.Dot(grav.normalized, projectGrav);
                //Debug.Log("Dot grav : " + dotGrav);
                rb.AddForce(projectGrav * grav.magnitude * dotGrav);
            }
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }



        // Slow down the ball
        if (squareDrag) // Somewhat realistic drag for a sphere (but we would also need friction to really make sense and fuck that)
        {
            float dragForceMagnitude = rb.velocity.sqrMagnitude * 0.47f * Time.fixedDeltaTime;
            Vector3 dragForceVector = dragForceMagnitude * -rb.velocity.normalized;
            rb.velocity += dragForceVector;
        }
        else // Not realistic but works well for gameplay
        {
            rb.velocity = rb.velocity * 0.99166f;
        }

    }

    RaycastHit CheckMovementWallCollision(Vector3 pos, Vector3 vel)
    {
        RaycastHit rayHit;

        Vector3 start = pos;
        Vector3 end = start + vel * Time.fixedDeltaTime;
        
        Physics.SphereCast(start, 0.05f, end - start, out rayHit, (end - start).magnitude, (1 << layerWall));

        //Debug.Log("Hit : " + rayHit.collider.gameObject.name + " in : " + transform.parent.gameObject.name);
        //Debug.DrawRay(start, end - start, Color.red, 0.5f);
        //Debug.Break();
        
        return rayHit;
    }
}
