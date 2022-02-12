using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LineRenderer), typeof(LaunchController))]
public class TrajectoryDrawer : MonoBehaviour
{
    public ParticleSystemController particleSystemController;

    public TrajectoryDrawingSettings drawSettings;

    public SimulationSettings simulationSettings;

    [System.Serializable]
    public struct TrajectoryDrawingSettings
    {
        public int resolution;
        public float floor;
        public LayerMask collisionMask;
        public bool simulate;
    }

    [System.Serializable]
    public struct SimulationSettings
    {
        public float simulationTime;
        [Range(0.0f, 0.1f)]
        public float simulationSpeed;
    }

    struct SimulationData
    {
        public Scene simulatedScene;
        public PhysicsScene physicsScene;
        public ProjectileController simulatedProjectile;
        public GameObject[] colliders;
    }

    SimulationData simulationData;

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

        if (drawSettings.simulate)
        {
            SetupSimulationData();
        }
    }

    void SetupSimulationData()
    {
        simulationData = new SimulationData();

        CreateSceneParameters createSceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        simulationData.simulatedScene = SceneManager.CreateScene("Simulation Scene", createSceneParameters);
        simulationData.physicsScene = simulationData.simulatedScene.GetPhysicsScene();
        GetColliders();
        SetupSimulationScene();
    }

    void GetColliders()
    {
        Collider[] colliders = FindObjectsOfType<Collider>();
        List<GameObject> colliderRoots = new List<GameObject>();

        for(int i = 0; i < colliders.Length; i++)
        {
            GameObject go = colliders[i].transform.root.gameObject;
            if (!colliderRoots.Contains(go) && go.layer != LayerMask.NameToLayer("Player"))
            {
                colliderRoots.Add(go);
            }
        }

        simulationData.colliders = new GameObject[colliderRoots.Count];

        for (int i = 0; i < simulationData.colliders.Length; i++)
        {
            simulationData.colliders[i] = Instantiate(colliderRoots[i]);
            simulationData.colliders[i].layer = LayerMask.NameToLayer("Invisible");
        }

        simulationData.simulatedProjectile = Instantiate(launchController.projectilePrefab);
        simulationData.simulatedProjectile.gameObject.layer = LayerMask.NameToLayer("Invisible");
    }

    void SetupSimulationScene()
    {
        SceneManager.MoveGameObjectToScene(simulationData.simulatedProjectile.gameObject, simulationData.simulatedScene);

        foreach (GameObject go in simulationData.colliders)
        {
            SceneManager.MoveGameObjectToScene(go, simulationData.simulatedScene);
        }
    }

    private void Update()
    {
        if (isDirty)
        {
            isDirty = false;

            if (drawSettings.simulate)
            {
                StopAllCoroutines();
                StartCoroutine(SimulateTrajectory());
            }
            else
            {
                CalculateTrajectoryPath();
            }
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

    IEnumerator SimulateTrajectory()
    {
        lineRenderer.positionCount = 0;
        particleSystemController.ClearParticles();
        particleSystemController.DisplayParticles();

        //reset projectile and fire simulated projectile
        Rigidbody simBody = simulationData.simulatedProjectile.Rigidbody;
        simBody.position = launchController.transform.position;
        simBody.rotation = Quaternion.identity;
        simBody.angularVelocity = Vector3.zero;
        simBody.velocity = TrajectoryCalculator.GetLaunchDirection(launchController.transform) * launchController.launchVelocity;

        float timeStepToPoint = simulationSettings.simulationTime / drawSettings.resolution;
        points = new Vector3[drawSettings.resolution];
        int currentIndex = 0;

        float startTime = Time.realtimeSinceStartup;

        Vector3 lastPos = simBody.position;

        for (float t = 0f; t < simulationSettings.simulationTime; t += Time.fixedDeltaTime)
        {
            if (currentIndex >= points.Length || isDirty)
            {
                break;
            }

            if (t >= timeStepToPoint * currentIndex)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(currentIndex, simBody.position);

                points[currentIndex] = simBody.position;
                currentIndex++;
            }

            simulationData.physicsScene.Simulate(Time.fixedDeltaTime);
           
            Vector3 dir = (simBody.position - lastPos).normalized;
            if(Physics.Raycast(simBody.position, dir, out RaycastHit hit, 1f, drawSettings.collisionMask))
            {
                particleSystemController.SetParticle(hit);
                particleSystemController.DisplayParticles();
            }

            lastPos = simBody.position;

            if (Time.realtimeSinceStartup - startTime >= simulationSettings.simulationSpeed)
            {
                yield return null;

                startTime = Time.realtimeSinceStartup;
            }
        }
    }
}
