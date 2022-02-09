using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TrajectoryCalculator
{
    public static TrajectoryInfo GetTragectoryInfo(Transform fireTransform, float initialSpeed, float startingHeight, float floor)
    {
        //we get the fireDirection from the forward of the fireTransform
        Vector3 fireDirection = GetLaunchDirection(fireTransform);

        //then we use that to get the fire angle off the horizon
        float fireAngle = GetLaunchAngle(fireDirection);

        //then with that info and some of the original parameters we can calculate a, b, and c of the quadratic formula
        //the QuadParams struct just wraps those up together for us
        QuadParams quadParams = GetQuadParams(initialSpeed, fireAngle, startingHeight, floor);

        //then we can use that to get the timeToFloor
        float timeToFloor = GetTimeToFloor(quadParams);

        //and then we have all the info to describe the trajectory of our projectile
        return new TrajectoryInfo(fireDirection, fireTransform.position, initialSpeed, fireAngle, timeToFloor);
    }

    //this formula takes in out trajectory info and some info and calculates the positions along the trajectory
    public static void GetTrajectoryPoints(TrajectoryInfo trajectoryInfo, int resolution, out Vector3[] points, out RaycastHit raycastHit, LayerMask collisionMask)
    {
        raycastHit = new RaycastHit();

        //calculate the time between points on our trajectory
        float timeStep = trajectoryInfo.TimeToFloor / resolution;

        //and assign the points array to have the correct size, we use + 1 because we want to go all the way to the floor
        points = new Vector3[resolution + 1];

        //we have to use the normalized vector of the horizontal components of our fireDirection, because they need to have a magnitude of 1
        //unless there is no horizontal comonent
        Vector2 horizontal = new Vector2(trajectoryInfo.LaunchDirection.x, trajectoryInfo.LaunchDirection.z);
        if (horizontal.sqrMagnitude != 0f)
        {
            horizontal = horizontal.normalized;
        }

        //then we just step through assigning all of the points
        for (int i = 0; i < points.Length; i++)
        {
            //current time of along trajectory
            float currentTime = timeStep * i;

            //then we just need to brake it down into x and z components by using the direction
            float xPart = horizontal.x * trajectoryInfo.InitialHorizontalSpeed;
            float zPart = horizontal.y * trajectoryInfo.InitialHorizontalSpeed;

            //and apply the time to get the x and y positions
            float xPos = xPart * currentTime + trajectoryInfo.StartingPosition.x;
            float zPos = zPart * currentTime + trajectoryInfo.StartingPosition.z;

            //then for the y position we just use the formula from above
            float yPos = trajectoryInfo.StartingPosition.y + trajectoryInfo.InitialVerticalSpeed * currentTime + (.5f * Physics.gravity.y) * (currentTime * currentTime);

            Vector3 point = new Vector3(xPos, yPos, zPos);

            if(i > 0)
            {
                raycastHit = FindCollisionPoint(points[i - 1], point, collisionMask);

                if (raycastHit.collider)
                {
                    points[i] = raycastHit.point;
                    System.Array.Resize(ref points, i + 1);
                    return;
                }
                else
                {
                    points[i] = point;
                }
            }
            else
            {
                points[i] = point;
            }                     
        }

        raycastHit.point = points[points.Length - 1];
        raycastHit.normal = Vector3.up;
    }

    static RaycastHit FindCollisionPoint(Vector3 from, Vector3 to, LayerMask collisionMask)
    {
        RaycastHit hit;
        Vector3 vector = to - from;
        float distance = vector.magnitude;
        Vector3 direction = vector / distance;

        Physics.Raycast(from, direction, out hit, distance, collisionMask);

        return hit;
    }

    static Vector3 GetLaunchDirection(Transform launchTransform)
    {
        return launchTransform.forward;
    }

    static float GetLaunchAngle(Vector3 launchDirection)
    {
        return Mathf.Asin(launchDirection.y);
    }

    //with the parameters given we can calculate a, b, and c of our quadratic equation and return them in a nice struct
    static QuadParams GetQuadParams(float initialVelocity, float projectileAngle, float startingHeight, float floor)
    {
        float a = .5f * Physics.gravity.y;
        float b = initialVelocity * Mathf.Sin(projectileAngle);
        float c = startingHeight - floor;

        return new QuadParams(a, b, c);
    }

    //with the quadParams we can use the quadratic formula to get the time to floor
    //if we will never hit the floor we return a negative value and a warning about what might be wrong
    static float GetTimeToFloor(QuadParams quadParams)
    {
        float determinant = (quadParams.B * quadParams.B) - (4 * quadParams.A * quadParams.C);

        if (determinant < 0)
        {
            Debug.LogWarning("Trying to take square root of negative number, check gravity in physics settings!!!");
            return -1;
        }
        else
        {
            determinant = Mathf.Sqrt(determinant);
        }

        return (-quadParams.B - determinant) / (2 * quadParams.A);
    }

    struct QuadParams
    {
        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }

        public QuadParams(float a, float b, float c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
