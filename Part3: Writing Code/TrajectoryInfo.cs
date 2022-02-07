using UnityEngine;

public struct TrajectoryInfo
{
    public Vector3 LaunchDirection { get; private set; }
    public Vector3 StartingPosition { get; private set; }
    public float InitialHorizontalSpeed { get; private set; }
    public float InitialVerticalSpeed { get; private set; }
    public float TimeToFloor { get; private set; }

    public TrajectoryInfo(Vector3 launchDirection, Vector3 startingPos, float initialSpeed, float launchAngle, float timeToFloor)
    {
        LaunchDirection = launchDirection;
        StartingPosition = startingPos;
        InitialHorizontalSpeed = Mathf.Cos(launchAngle) * initialSpeed;
        InitialVerticalSpeed = Mathf.Sin(launchAngle) * initialSpeed;
        TimeToFloor = timeToFloor;
    }
}
