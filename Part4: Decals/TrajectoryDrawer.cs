using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(LaunchController))]
public class TrajectoryDrawer : MonoBehaviour
{
    public ParticleSystemController particleSystemController;

    public TrajectoryDrawingSettings drawSettings;

    [System.Serializable]
    public struct TrajectoryDrawingSettings
    {
        public int resolution;
        public float floor;
        public LayerMask collisionMask;
    }

    LineRenderer lineRenderer;
    LaunchController launchController;

    Vector3[] points;

    bool isDirty;

    public void SetDirty()
    {
        isDirty = true;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        launchController = GetComponent<LaunchController>();
    }

    private void Update()
    {
        if (isDirty)
        {
            CalculateTrajectoryPath();
            
            isDirty = false;
        }
    }

    void CalculateTrajectoryPath()
    {
        TrajectoryInfo trajectoryInfo = TrajectoryCalculator.GetTragectoryInfo(transform, launchController.launchVelocity, transform.position.y, drawSettings.floor);

        if (trajectoryInfo.TimeToFloor >= 0)
        {
            TrajectoryCalculator.GetTrajectoryPoints(trajectoryInfo, drawSettings.resolution, out points, out RaycastHit hit, drawSettings.collisionMask);

            particleSystemController.ClearParticles();
            particleSystemController.SetParticle(hit);
            particleSystemController.DisplayParticles();
        }

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
