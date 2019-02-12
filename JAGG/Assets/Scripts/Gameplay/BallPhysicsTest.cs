using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysicsTest : MonoBehaviour
{
    private int layerFloor;
    private int layerWall;

    private const float epsilon = 0.00001f;

    public bool squareDrag = false;

    public bool grounded = false;
    public bool stable = false; // The ball is on the floor, not on a slope / mid-air
    public Vector3 currentFloorNormal = Vector3.up;

    // Stop the ball at low speeds
    private const float stopSpeedThreshold = 0.1f;
    private const float unevenGroundstopSpeedThreshold = 0.01f;


    public bool isBouncingOnFloor = false;

    public Vector3 velocityTrue = Vector3.zero;
    public Vector3 velocityCapped = Vector3.zero;

    private const float maxVelocityMagnitude = 40f;

    private int personalFrames = -1;

    // Use this for initialization
    void Start()
    {
        layerFloor = LayerMask.NameToLayer("Floor");
        layerWall = LayerMask.NameToLayer("Wall");
    }

    /*void Update()
    {
            // Nothing in update for determinism's sake
    }*/

    public void AddForce(Vector3 force)
    {
        velocityTrue += force * Time.fixedDeltaTime;
        velocityCapped += force * Time.fixedDeltaTime;
        //Debug.Log("AddForce : " + force.ToString("F6"));
        //Debug.Log("Resulting velocity : x=" + velocityTrue.ToString("F6") + ", magnitude : " + velocityTrue.magnitude);
    }

    public void StopBall()
    {
        velocityCapped = Vector3.zero;
        velocityTrue = Vector3.zero;
    }

    public void MultiplySpeed(float mult)
    {
        velocityCapped *= mult;
        velocityTrue *= mult;
    }

    void FixedUpdate()
    {
        personalFrames++;

        grounded = false;

        if (velocityTrue.magnitude > maxVelocityMagnitude) // Cap the velocity of the ball
        {
            velocityCapped = velocityCapped.normalized * maxVelocityMagnitude;
        }

        //Debug.Log("VelocityTrue magnitude : " + velocityTrue.magnitude + ", velocityCapped magnitude : " + velocityCapped.magnitude);

        // Handle collisions with the movement of the ball
        if (velocityCapped != Vector3.zero)
        {
            RaycastHit rayWallHit = CheckMovementWallCollision(transform.position, velocityCapped);
            if (rayWallHit.collider != null)
            {
                //Debug.Log("A wall : " + rayWallHit.collider.gameObject.name);

                // We calculate the perfect bounce angle with the wall
                Vector3 dir = velocityCapped;
                Vector3 wallDir = rayWallHit.normal;

                // New velocity direction
                Vector3 res = dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir);

                // If we hit a floor, we change the bounciness for the collision
                if (rayWallHit.collider.gameObject.layer == layerFloor)
                {
                    res = Vector3.Lerp(dir, res, 0.7f);
                    Vector3 project = Vector3.ProjectOnPlane(dir - 2f * (Vector3.Dot(dir, wallDir) * wallDir), wallDir);

                    //Debug.Log("Dir : " + dir.ToString("F6") + ", wallDir : " + wallDir.ToString("F6") + ", res : " + res.ToString("F6") + ", project : " + project.ToString("F6") + ", dot : " + Vector3.Dot(res.normalized, project.normalized));
                    //Debug.DrawRay(rayWallHit.point, wallDir, Color.black, 2f);
                    //Debug.Break();

                    // If the bounce is very close to the direction of the projection (which is to move along the floor) we use the projection, which means we follow angled floors (like half-pipe) more easily
                    if (Vector3.Dot(res.normalized, project.normalized) > 0.985f)
                    {
                        res = project;
                    }

                    isBouncingOnFloor = true;
                    grounded = true;
                }

                // We are going to check for subsequent collisions, if we find the same normal multiple times it means we are probably stuck (and should slow down)
                List<Vector3> normals = new List<Vector3>
                {
                    wallDir
                };
                // We must check if we aren't in a corner and risk going into a wall
                Collider otherWallCol;
                RaycastHit otherWallHit = CheckMovementWallCollision(transform.position, res);
                otherWallCol = otherWallHit.collider;
                wallDir = otherWallHit.normal;
                int _pasDeBoucleInfiniPourAlex = 0;
                while (otherWallCol != null && _pasDeBoucleInfiniPourAlex < 200)
                {
                    Vector3 newRes = res - 2f * (Vector3.Dot(res, otherWallHit.normal) * otherWallHit.normal);
                    if (otherWallCol.gameObject.layer == layerFloor)
                    {
                        newRes = Vector3.Lerp(res, newRes, 0.7f);
                        Vector3 project = Vector3.ProjectOnPlane(res - 2f * (Vector3.Dot(res, wallDir) * wallDir), wallDir);

                        //Debug.Log("oi");
                        //Debug.Log("Dir : " + res.ToString("F6") + ", wallDir : " + wallDir.ToString("F6") + ", res : " + newRes.ToString("F6") + ", project : " + project.ToString("F6") + ", dot : " + Vector3.Dot(newRes.normalized, project.normalized));
                        //Debug.DrawRay(rayWallHit.point, wallDir, Color.black, 2f);
                        //Debug.Break();

                        // If the bounce is very close to the direction of the projection (which is to move along the floor) we use the projection
                        if (Vector3.Dot(newRes.normalized, project.normalized) > 0.985f)
                        {
                            newRes = project;
                        }

                        isBouncingOnFloor = true;
                        grounded = true;
                    }

                    // Reduce the velocity if we find the same normal again
                    if (normals.Contains(otherWallHit.normal))
                    {
                        newRes *= 0.9f;
                        velocityTrue *= 0.9f;
                        if (velocityTrue.magnitude > maxVelocityMagnitude)
                        {
                            newRes = newRes.normalized * maxVelocityMagnitude;
                        }
                        else
                        {
                            newRes = newRes.normalized * velocityTrue.magnitude;
                        }
                    }
                    else
                        normals.Add(otherWallHit.normal);

                    res = newRes;
                    otherWallHit = CheckMovementWallCollision(transform.position, res);
                    otherWallCol = otherWallHit.collider;
                    wallDir = otherWallHit.normal;

                    _pasDeBoucleInfiniPourAlex++;
                    //Debug.Log("Nb consecutive obstacles : " + _pasDeBoucleInfiniPourAlex);
                }

                velocityCapped = res;
            }
        }


        // Stay on the floor
        // Apply gravity when needed
        Vector3 position = transform.position;
        position += new Vector3(0f, 0.001f, 0f);

        //Debug.DrawRay(position, -currentFloorNormal * length, Color.cyan, 2f);

        stable = false;

        Vector3 meanNormal = Vector3.zero;
        Vector3 meanPoint = Vector3.zero;

        RaycastHit[] rayHits = Physics.SphereCastAll(position, 0.05f, -currentFloorNormal, 0.0015f, 1 << layerFloor | 1 << layerWall);
        //Debug.DrawRay(position, -currentFloorNormal * 0.051f, Color.cyan, 1f);

        if (rayHits.Length >= 1) // Ball is in contact with something
        {
            //Debug.Log("Number of objects hit with SphereCastAll : " + rayHits.Length);
            //Debug.Break();

            List<Vector3> normals = new List<Vector3>();
            List<Vector3> points = new List<Vector3>();

            /*RaycastHit test;
            bool lineTest = Physics.Linecast(position, position + (-currentFloorNormal * 0.051f), out test, 1 << layerFloor | 1 << layerWall);
            if (lineTest)
            {
                normals.Add(test.normal);
                points.Add(test.point);
                //Debug.DrawRay(position, (-currentFloorNormal * 0.051f), Color.yellow, 1f);
            }*/


            bool isValidCast = false;

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
                            //Debug.DrawRay(hit.point, hit.normal, Color.red, 0.1f);
                            normals.Add(hit.normal);
                            points.Add(hit.point);
                        }
                    }
                }
                else
                {
                    //Debug.Log("We are inside : " + hit.collider.name);
                    Collider other = hit.collider;
                    Vector3 direction;
                    float distance;
                    bool overlapped = Physics.ComputePenetration(GetComponent<Collider>(), transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out direction, out distance);
                    if (overlapped)
                    {
                        RaycastHit check;
                        //Debug.Log(overlapped);
                        //Debug.DrawRay(position, -direction * 0.051f, Color.cyan, 0.1f);
                        //Debug.Break();
                        RotatePiece rtp = other.GetComponentInParent<RotatePiece>();
                        MovingPiece mvp = other.GetComponentInParent<MovingPiece>();
                        if (rtp != null && !rtp.ballsOnTop.Contains(gameObject) && rtp.isRotation)
                        {
                            //Debug.Log("Overlap doesn't count because it's a RotatePiece, we want to deal with it in OnTriggerEnter");
                        }
                        else if (mvp != null && !mvp.ballsOnTop.Contains(gameObject))
                        {
                            //Debug.Log("Overlap doesn't count because it's a MovingPiece, we want to deal with it in OnTriggerEnter");
                        }
                        else if (Physics.Raycast(position, -direction, out check, 0.051f, 1 << layerFloor | 1 << layerWall))
                        {
                            //Debug.Log("Mission complete");
                            normals.Add(check.normal);
                            points.Add(check.point);
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
                //Debug.Log("Annoying"); // En vrai c'est pas si grave*/
                //Debug.Break();
            }


            if (isValidCast)
            {
                grounded = true;
                float dotMeanToUp = Vector3.Dot(meanNormal.normalized, Vector3.up);
                stable = 1f - dotMeanToUp < 0.005f; // TEST VALUE

                /*if (!stable)
                {
                    Debug.Log("Hit " + normals.Count + " colliders");
                    Debug.Log("MeanNormal : x= " + meanNormal.x + ", y= " + meanNormal.y + ", z= " + meanNormal.z);
                    Debug.Log("MeanNormal normalized : " + meanNormal.normalized.ToString("F6"));
                    Debug.DrawRay(position, meanNormal * 0.1f, Color.yellow, 1f);
                    Debug.Log("Distance between MeanNormal and Vector3.up : " + Vector3.Distance(meanNormal, Vector3.up));
                    Debug.Log("Dot product between MeanNormal and Vector3.up : " + Vector3.Dot(meanNormal, Vector3.up));
                    //Debug.Break();
                }*/

                /*if (lineTest)
                    currentFloorNormal = test.normal;
                else*/
                currentFloorNormal = meanNormal.normalized;

                float distToGround = 0.05f - Vector3.Distance(transform.position, meanPoint);
                if (Mathf.Abs(distToGround) > 0.001f && velocityCapped.magnitude > 0f)
                {
                    //Debug.Log("Distance to the ground : " + distToGround + ", pos.y=" + transform.position.y /*+ ", point.y=" + test.point.y*/);
                    //transform.position += meanNormal * distToGround; // better solution
                    transform.position = meanPoint + meanNormal * 0.05f; // betterer solution
                    //Debug.DrawRay(meanPoint, meanNormal * 0.05f, Color.blue, 0.2f);
                    //Debug.Log("Position adjusted");
                    //Debug.Break();
                }

                if (stable && velocityCapped.magnitude > 0f && normals.Count >= 2)
                {
                    int i = 0;
                    bool normalsAreSimilar = true;
                    while (i < normals.Count - 1)
                    {
                        Vector3 firstNorm = normals[i];
                        i++;
                        for (int j = i; j < normals.Count; j++)
                        {
                            Vector3 otherNorm = normals[j];
                            if (Vector3.Dot(firstNorm, otherNorm) < 0.97f)
                            {
                                //Debug.Log("FirstNorm : " + firstNorm.ToString("F6") + ", otherNorm : " + otherNorm.ToString("F6") + ", dot : " + Vector3.Dot(firstNorm, otherNorm));
                                normalsAreSimilar = false;
                            }
                        }
                    }

                    if (!normalsAreSimilar && velocityCapped.magnitude < 0.25f) // This is all to avoid getting stuck between multiple slopes/angled floors/whatever
                    {
                        StopBall();

                        /*Debug.Log(gameObject.name);
                        foreach (Vector3 n in normals)
                            Debug.Log(n.ToString("F6"));

                        Debug.Break();*/
                    }
                }

                // Check if we should stop when grounded
                if (velocityCapped.magnitude < (stable ? stopSpeedThreshold : unevenGroundstopSpeedThreshold))
                {
                    //Debug.Log("Stop the ball : " + velocityCapped.magnitude);
                    StopBall();
                }
            }
        }

        if (!grounded)
        {
            // We are falling, check if we aren't inside a floor
            currentFloorNormal = Vector3.up;

            Collider[] cols = Physics.OverlapSphere(position, 0.05f, (1 << layerFloor));
            if (cols.Length > 0)
            {
                //Debug.Log("We are overlapped with : " + cols.Length + " colliders");
                Collider other = cols[0];
                Vector3 direction;
                float distance;
                bool overlapped = Physics.ComputePenetration(GetComponent<Collider>(), transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out direction, out distance);
                if (overlapped)
                {
                    transform.position += direction * distance;
                    //Debug.Log("Resolved overlap with : " + other.name);
                }
            }
        }

        Vector3 grav = Physics.gravity;

        if (!stable)
        {
            //Debug.Log("Not stable");
            //Debug.Log("Grounded : " + grounded);
            //Debug.Break();
            // Mid-air
            if (!grounded)
            {
                AddForce(grav);
            }
            else if (!isBouncingOnFloor)
            {
                // Worst case scenario, we are on a slope
                // But everything will be daijobu desu
                Vector3 vel = velocityCapped;
                Vector3 normal = currentFloorNormal;
                Vector3 project = Vector3.ProjectOnPlane(vel, normal).normalized;
                //Debug.DrawRay(transform.position, project * 10f, Color.red, 10f);
                //Debug.Log("Velocity : " + vel + ", normal : " + normal + ", project : " + project);

                //Debug.Break();


                // Move alongside the slope
                velocityCapped = project * velocityCapped.magnitude;

                //Debug.Log("velocityCapped magnitude : " + velocityCapped.magnitude + ", velocity : " + velocityCapped.ToString("F6"));

                // Apply gravity 
                // We project the gravity along the slope
                // Using the dot product we get a value that is bigger the higher the angle of the slope is
                if (normal.y >= 0f)
                {
                    Vector3 projectGrav = Vector3.ProjectOnPlane(grav, normal).normalized;
                    float dotGrav = Vector3.Dot(grav.normalized, projectGrav);
                    //Debug.Log("Dot grav : " + dotGrav + ", normal : " + normal + ", angle : " + Vector3.Angle(normal, Vector3.up) + ", projectGrav : " + projectGrav);
                    AddForce(projectGrav * grav.magnitude * Mathf.Max(dotGrav, 0.125f));
                }
                else // We are upside-down so just gravity
                {
                    AddForce(grav);
                    currentFloorNormal = Vector3.up;
                }
            }
        }
        else
        {
            if (!isBouncingOnFloor)
            {
                //Debug.Log("Stable velocityCapped magnitude : " + velocityCapped.magnitude + ", velocity : " + velocityCapped.ToString("F6"));
                velocityCapped = new Vector3(velocityCapped.x, 0f, velocityCapped.z);
            }
        }


        isBouncingOnFloor = false;

        // Move the ball
        transform.position += velocityCapped * Time.fixedDeltaTime;


        // Slow down the ball
        /*if (squareDrag) // Somewhat realistic drag for a sphere (but we would also need friction to really make sense and lol nope)
        {
            float dragForceMagnitude = velocityCapped.sqrMagnitude * 0.47f * Time.fixedDeltaTime;
            Vector3 dragForceVector = dragForceMagnitude * -velocityCapped.normalized;
            velocityCapped += dragForceVector;
        }*/

        // Not realistic but works well for gameplay
        velocityTrue = velocityTrue * 0.99166f;
        velocityCapped = velocityCapped * 0.99166f;
    }

    RaycastHit CheckMovementWallCollision(Vector3 pos, Vector3 vel)
    {
        RaycastHit rayHit;

        Vector3 start = pos + new Vector3(0f, 0.001f, 0f);
        Vector3 end = start + vel * Time.fixedDeltaTime;

        bool check = Physics.SphereCast(start, 0.05f, end - start, out rayHit, (end - start).magnitude, 1 << layerWall);

        if (!check)
            check = Physics.SphereCast(start, 0.045f, end - start, out rayHit, (end - start).magnitude, 1 << layerFloor);

        /*Debug.Log("CheckMovementWallCollision : pos = " + pos.ToString("F6") + ", vel = " + vel.ToString("F6"));
        if (rayHit.collider != null)
            Debug.Log("Hit : " + rayHit.collider.gameObject.name + " in : " + transform.parent.gameObject.name);

        Debug.DrawRay(start, (end - start)*10f, Color.red);*/
        //Debug.Break();

        return rayHit;
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject collided = other.gameObject;
        //Debug.Log("Collided with : " + collided.name + ", at frame : " + personalFrames);

        // Other ball
        if (collided.CompareTag("Player"))
        {
            BallPhysicsTest physicsOther = collided.GetComponent<BallPhysicsTest>();
            Vector3 velOther = physicsOther.velocityCapped;
            //Debug.Log("My velocity magnitude : " + velocityCapped.magnitude + ", otherVel magnitude : " + velOther.magnitude);
            if (velocityCapped.magnitude > velOther.magnitude + 3f)
            {
                // Destroy other player
                Debug.Log("Destroy : " + collided.name);
            }
            else if (velocityCapped.magnitude > velOther.magnitude)
            {
                // Simple collision
                //Debug.Log("Collision : " + collided.name);

                StartCoroutine(HitABall(physicsOther, other.transform.position));
                //Debug.Break();
            }
        }

        // Hole trigger, we are inside the hole
        /*if(collided.CompareTag("Hole"))
        {

        }*/


        // Part of a rtp or mvp but not the MoveRotate trigger
        if (collided.layer != LayerMask.NameToLayer("MoveRotate"))
        {
            RotatePiece rtp = collided.GetComponentInParent<RotatePiece>();
            MovingPiece mvp = collided.GetComponentInParent<MovingPiece>();

            if (rtp != null && !rtp.ballsOnTop.Contains(gameObject) && rtp.isRotation)
            {
                //Debug.Log("RotatePiece attack : " + collided.name);
                Vector3 direction;
                float distance;

                bool overlapped = Physics.ComputePenetration(GetComponent<Collider>(), transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out direction, out distance);
                //Debug.Log(overlapped + ", direction : " + direction.ToString("F6") + ", distance : " + distance);
                //Debug.DrawRay(transform.position, direction, Color.red, 3f);
                

                if (overlapped)
                    StartCoroutine(HitByRTP(direction, distance, rtp));

                //Debug.Break();
            }
            if (mvp != null && !mvp.ballsOnTop.Contains(gameObject))
            {
                //Debug.Log("MovingPiece attack : " + collided.name);
                Vector3 direction;
                float distance;

                bool overlapped = Physics.ComputePenetration(GetComponent<Collider>(), transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out direction, out distance);

                //Debug.Log(overlapped + ", direction : " + direction.ToString("F6") + ", distance : " + distance);
                //Debug.Log(Vector3.Dot(direction, (mvp.initPos - mvp.destPos).normalized).ToString("F6"));
                //Debug.DrawRay(transform.position, direction, Color.red, 3f);
                

                if (overlapped && mvp.isMoving)
                    StartCoroutine(HitByMVP(direction, distance, mvp));


                Collider[] cols = Physics.OverlapSphere(transform.position + velocityCapped * Time.fixedDeltaTime, 0.049f, 1 << layerFloor | 1 << layerWall);
                if (cols.Length > 0)
                {
                    //Debug.LogError("We are getting crushed by a moving piece"); // LogError because there are two solutions to the problem => Explode the ball / Move it away from those colliders
                    //Debug.Break();
                    AddForce(-velocityCapped * 60f);
                    AddForce(Vector3.up * 360f);
                    transform.position += Vector3.up * 0.2f;
                }

                //Debug.DrawRay(transform.position + velocityCapped * Time.fixedDeltaTime, Vector3.up, Color.blue, 1f);

                //Debug.Break();
            }
        }
    }

    public IEnumerator HitABall(BallPhysicsTest physicsOther, Vector3 otherPos)
    {
        yield return new WaitForFixedUpdate();
        //Debug.Log(name + " solving collision with : " + physicsOther.name + " at frame : " + personalFrames);
        Vector3 v1 = velocityCapped;
        Vector3 v2 = physicsOther.velocityCapped;
        Vector3 x1 = transform.position;
        Vector3 x2 = otherPos;
        velocityCapped = v1 - (Vector3.Dot(v1 - v2, x1 - x2) / (x1 - x2).sqrMagnitude * (x1 - x2));
        physicsOther.velocityCapped = v2 - (Vector3.Dot(v2 - v1, x2 - x1) / (x2 - x1).sqrMagnitude * (x2 - x1));
    }

    public IEnumerator HitByRTP(Vector3 direction, float distance, RotatePiece rtp)
    {
        yield return new WaitForFixedUpdate();
        //Debug.Log("Solve rtp hit : " + personalFrames);
        if (!rtp.ballsOnTop.Contains(gameObject))
        {
            //Debug.Log("Apply rtp hit at : " + personalFrames);
            transform.position += direction * Mathf.Max(distance, 0.04f);
            AddForce(direction * 120f * 2f * (1 / rtp.spinTime * rtp.rotationAngle / 90f));
        }
    }

    public IEnumerator HitByMVP(Vector3 direction, float distance, MovingPiece mvp)
    {
        yield return new WaitForFixedUpdate();
        if(!mvp.ballsOnTop.Contains(gameObject))
        {
            transform.position += direction * Mathf.Max(distance, 0.01f) * (1 / mvp.travelTime * Vector3.Distance(mvp.initPos, mvp.destPos));
            AddForce(direction * 144f * (1 / mvp.travelTime * Vector3.Distance(mvp.initPos, mvp.destPos)));
        }
    }

    // We have almost-zero value differences (like some pieces will have y=0 and others y=4e-17)
    private static bool ApproximatelyEquals(float a, float b)
    {
        return Mathf.Abs(a - b) < epsilon;
    }
}
