using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;


public enum GravityType
{
    Normal = 0,
    Low = 1,
    High = 2
};

public class BallPhysicsNetwork : NetworkBehaviour {

    private Rigidbody rb;
    private int layerFloor;
    private int layerWall;

    public Transform sphere; // Transform of the mesh of the sphere so we can rotate it
    Quaternion serverRota = Quaternion.identity;

    public GravityType gravityType;
    private GravityType oldGravityType;

    public bool squareDrag = false;

    [HideInInspector]
    public bool stable = false; // The ball is on the floor, not on a slope / mid-air
    private Vector3 currentFloorNormal = Vector3.up;
    // Stop the ball at low speeds
    private const float stopSpeedThreshold = 0.1f;
    private const float unevenGroundstopSpeedThreshold = 0.01f;

    // Hack for the trail on high-speed wall collisions
    private bool flagFixPos = false;
    private Vector3 fixedPos;

    private bool isBouncingOnFloor = false;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
    }

    void Update()
    {
        // Fix the position of the ball after a collision with a wall (allows the trail to work normally)
        // Rotate the ball if we are moving
        if (isServer)
        {
            if (rb.velocity.magnitude > 0.005f)
                sphere.Rotate(new Vector3(rb.velocity.z * 10f, 0f, -rb.velocity.x * 10f), Space.World);
            RpcRotateBall(sphere.rotation);
        }
        if (flagFixPos)
        {
            transform.position = fixedPos;
            flagFixPos = false;
        }
    }



    void FixedUpdate()
    {
        if (!isServer)
        {
            sphere.transform.rotation = Quaternion.Slerp(sphere.transform.rotation, serverRota, 15f * Time.deltaTime);
            return;
        }

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
            if (rayWallHit.collider.gameObject.layer == layerFloor)
            {
                res = Vector3.Lerp(dir, res, 0.55f); // Random as fuck but works
            }

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
                if (otherWallCol.gameObject.layer == layerFloor)
                {
                    res = Vector3.ProjectOnPlane(res, otherWallHit.normal);
                }

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


        // Stay on the floor
        // Apply gravity when needed
        Vector3 position = transform.position;
        position += new Vector3(0f, 0.001f, 0f);

        //Debug.DrawRay(position, -currentFloorNormal * length, Color.cyan, 2f);

        bool grounded = false;
        stable = false;

        RaycastHit[] rayHits = Physics.SphereCastAll(position, 0.05f, -currentFloorNormal, 0.002f, 1 << layerFloor | 1 << layerWall);

        Vector3 meanNormal = Vector3.zero;
        Vector3 meanPoint = Vector3.zero;

        if (rayHits.Length >= 1) // Ball is in contact with something
        {
            //Debug.Log("Number of objects hit with SphereCastAll : " + rayHits.Length);

            List<Vector3> normals = new List<Vector3>();
            List<Vector3> points = new List<Vector3>();

            bool isValidCast = false;

            //Debug.Log("Current position : " + position);
            foreach (RaycastHit hit in rayHits) // Check if the collision are obstructed or not
            {
                if (hit.point != Vector3.zero)
                {
                    //Debug.Log("Object : " + hit.collider.name + ", point : x=" + hit.point.x + ", y=" + hit.point.y + ", z=" + hit.point.z);
                    RaycastHit check;
                    //Debug.DrawRay(position, (hit.point - position), Color.red, 1f);
                    if (Physics.Raycast(position, hit.point - position, out check, Mathf.Infinity, 1 << layerFloor | 1 << layerWall))
                    {
                        //Debug.Log("TOAST : " + check.collider.name);
                        if (hit.collider == check.collider)
                        {
                            normals.Add(hit.normal);
                            points.Add(hit.point);
                        }
                    }
                }
            }



            if (normals.Count > 1)
            {
                // We are sitting on multiple colliders at once, we want to find the average normal in order to know if we are on even ground or not
                foreach (Vector3 norm in normals)
                    meanNormal += norm;
                meanNormal /= normals.Count;

                foreach (Vector3 p in points)
                    meanPoint += p;
                meanPoint /= points.Count;

                isValidCast = true;
            }
            else if (normals.Count == 1)
            {
                // Only one collider is really in the path of the ball, the others are obstructed by this one
                meanNormal = normals[0];
                meanPoint = points[0];

                isValidCast = true;
            }
            else
            {
                // Bon le sphereCastAll à trouver des trucs mais ils sont apparemment tous obstrués, unlucky (on est posé pile entre 2 colliders en gros)
                // On fait le test legacy classique qui ne peut pas échouer normalement, ce qui donnera néanmoins pas la même précision pour la collision à cette frame de la physique

                RaycastHit test;
                bool lineTest = Physics.Linecast(position, position + (-currentFloorNormal * 0.052f), out test, 1 << layerFloor | 1 << layerWall);
                if (lineTest)
                {
                    meanNormal = test.normal;
                    meanPoint = test.point;
                    isValidCast = true;
                }
                else
                    Debug.Log("Annoying"); // En vrai c'est pas si grave

                //Debug.Break();
            }


            if (isValidCast)
            {
                grounded = true;
                stable = 1f - Vector3.Dot(meanNormal, Vector3.up) < 0.01f; // TEST VALUE

                /*if (!stable)
                {
                    Debug.Log("MeanNormal : x= " + meanNormal.x + ", y= " + meanNormal.y + ", z= " + meanNormal.z);
                    Debug.DrawRay(position, meanNormal, Color.cyan, 1f);
                    Debug.Log("Distance between MeanNormal and Vector3.up : " + Vector3.Distance(meanNormal, Vector3.up));
                    Debug.Log("Dot product between MeanNormal and Vector3.up : " + Vector3.Dot(meanNormal, Vector3.up));
                    Debug.Break();
                }*/

                currentFloorNormal = meanNormal;
                float distToGround = 0.05f - Vector3.Distance(transform.position, meanPoint);
                if (distToGround > 0.001f)
                {
                    //Debug.Log("Distance to the ground : " + distToGround + ", pos.y=" + transform.position.y + ", point.y=" + test.point.y);
                    //rb.AddForce(test.normal * rb.velocity.sqrMagnitude); // Find better solution
                    transform.position += meanNormal * distToGround; // better solution
                }

                // Check if we should stop when grounded
                if (rb.velocity.magnitude < (stable ? stopSpeedThreshold : unevenGroundstopSpeedThreshold))
                    rb.velocity = Vector3.zero;
            }
        }

        if (!grounded)
        {
            // We are falling, we must check if we are not going inside a floor
            currentFloorNormal = Vector3.up;

            RaycastHit floorHit;
            bool floorCheck = Physics.SphereCast(position, 0.05f, rb.velocity, out floorHit, 0.02f * Mathf.Max(1f, rb.velocity.magnitude), 1 << layerFloor);
            //Debug.DrawRay(position, rb.velocity * 0.01f * Mathf.Max(1f, rb.velocity.magnitude), Color.red);

            if (floorCheck)
            {
                // Debug.DrawRay(floorHit.point, floorHit.normal, Color.red, 2f);

                // We calculate the perfect bounce angle with the floor
                Vector3 dir = rb.velocity;
                currentFloorNormal = floorHit.normal;

                // New velocity direction
                Vector3 res = dir - 2f * (Vector3.Dot(dir, currentFloorNormal) * currentFloorNormal);
                res = Vector3.Lerp(dir, res, 0.7f); // Random as fuck but works

                flagFixPos = true;
                fixedPos = floorHit.point + currentFloorNormal * 0.05f;

                isBouncingOnFloor = true;

                rb.velocity = res;
            }
            else
            {
                // Double-check with overlapsphere
                Collider[] cols = Physics.OverlapSphere(position, 0.05f, (1 << layerFloor));
                if (cols.Length > 0)
                {
                    //Debug.Log("We are overlapped with " + cols.Length + " colliders, the first is " + cols[0].gameObject.name);
                    RaycastHit lineHit;
                    bool line = Physics.Linecast(transform.position, cols[0].transform.position, out lineHit, (1 << layerFloor));
                    if (line)
                    {
                        float distToOverlap = (0.05f - Vector3.Distance(transform.position, lineHit.point));
                        //Debug.Log("distToOverLap = " + distToOverlap);
                        if (distToOverlap > -0.03f)
                        {
                            transform.position += lineHit.normal * 0.005f;
                            currentFloorNormal = lineHit.normal;
                            //Debug.Log("OverlapSphere => move");
                        }
                    }
                    else
                    {
                        Debug.LogError("Fuck this : " + cols[0].gameObject.name); // Very unlikely (or not lul) to be overlapping an object but unable to find it with a raycast
                        Debug.DrawRay(transform.position, Vector3.up * 5f, Color.yellow, 5f);
                        //Debug.Break();
                    }
                }
            }
        }

        // Can use custom gravity to obtain various results
        Vector3 grav;

        switch (gravityType)
        {
            case GravityType.Normal:
                grav = Physics.gravity;
                break;

            case GravityType.Low:
                grav = new Vector3(0, Physics.gravity.y + 7, 0);
                break;

            case GravityType.High:
                grav = new Vector3(0, Physics.gravity.y - 7, 0);
                break;

            default:
                grav = Physics.gravity;
                break;
        }

        if (!stable)
        {
            // Mid-air
            if (!grounded)
            {
                rb.AddForce(grav);
            }
            else if (!isBouncingOnFloor)
            {
                // Worst case scenario, we are on a slope
                // But everything will be daijobu desu
                Vector3 vel = rb.velocity;
                Vector3 normal = meanNormal;
                Vector3 project = Vector3.ProjectOnPlane(vel, normal).normalized;
                //Debug.DrawRay(transform.position, project * 10f, Color.red, 10f);
                //Debug.Log("Velocity : " + vel + ", normal : " + normal + ", cross : " + project);

                //Debug.Break();

                // Move alongside the slope
                rb.velocity = project * rb.velocity.magnitude;

                // Apply gravity 
                // We project the gravity along the slope
                // Using the dot product we get a value that is bigger the higher the angle of the slope is
                if (normal.y >= 0f)
                {
                    Vector3 projectGrav = Vector3.ProjectOnPlane(grav, normal).normalized;
                    float dotGrav = Vector3.Dot(grav.normalized, projectGrav);
                    //Debug.Log("Dot grav : " + dotGrav + ", normal : " + normal + ", projectGrav : " + projectGrav);
                    rb.AddForce(projectGrav * grav.magnitude * dotGrav);
                }
                else // We are upside-down so just gravity
                {
                    rb.AddForce(grav);
                }
            }
            else
                isBouncingOnFloor = false;
        }
        else
        {
            if (rb.velocity.y < 0.2f)
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

        Physics.SphereCast(start, 0.05f, end - start, out rayHit, (end - start).magnitude, (1 << layerWall | 1 << layerFloor));

        //Debug.Log("Hit : " + rayHit.collider.gameObject.name + " in : " + transform.parent.gameObject.name);
        //Debug.DrawRay(start, end - start, Color.red, 0.5f);
        //Debug.Break();

        return rayHit;
    }


    public void ChangeGravity(GravityType gravityType)
    {
        oldGravityType = this.gravityType;
        this.gravityType = gravityType;
    }
    public void ResetGravity()
    {
        gravityType = oldGravityType;
    }


    [ClientRpc]
    void RpcRotateBall(Quaternion rota)
    {
        //sphere.transform.rotation = rota;
        serverRota = rota;
    }
}
