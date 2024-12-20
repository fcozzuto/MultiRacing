using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshAICarController : MonoBehaviour
{
    public Transform currentWaypoint;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;

    // Copy relevant stats from PrometeoCarController
    public int maxSpeed;
    public int accelerationMultiplier;
    public int brakeForce;
    public int maxSteeringAngle;
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    private Rigidbody carRigidbody;
    private float throttleAxis = 0f;

    // Sounds
    public AudioSource engineSound;
    public AudioSource tireSound;
    private float enginePitch = 0f;


    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Racing")
        {
            carRigidbody = GetComponent<Rigidbody>();
            agent = GetComponent<NavMeshAgent>();

            // Sync NavMeshAgent speed with car dynamics
            agent.speed = maxSpeed;
            agent.acceleration = accelerationMultiplier;
            agent.angularSpeed = maxSteeringAngle;
            //agent.updatePosition = false;
            //agent.updateRotation = false;

            currentWaypoint = waypoints[currentWaypointIndex];
            if (currentWaypoint != null)
            {
                agent.SetDestination(currentWaypoint.position);
            }
            else
            {
                Debug.LogError("No waypoint assigned to AI car.");
            }
            // Initialize engine sound
            if (engineSound != null)
            {
                engineSound.loop = true;
                engineSound.Play();
                engineSound.volume = 0.25f;
            }

            // Initialize tire sound
            if (tireSound != null)
            {
                tireSound.loop = true;
                tireSound.volume = 0f;
                tireSound.Play();
            }
        }
    }

    void FixedUpdate()
    {
        if (agent.pathPending || currentWaypoint == null) return;

        // Drive toward the NavMeshAgent's next position
        Vector3 targetPosition = agent.nextPosition;

        NavigateToPosition(targetPosition);
        UpdateSoundEffects();
        UpdateWheelMeshes();

        // Check if we are near the current waypoint and move to the next
        if (Vector3.Distance(transform.position, currentWaypoint.position) < agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            currentWaypoint = waypoints[currentWaypointIndex];
            agent.SetDestination(currentWaypoint.position);
        }
        /*
                    Waypoint waypoint = currentWaypoint.GetComponent<Waypoint>();
                    if (waypoint != null)
                    {
                        currentWaypoint = waypoint.GetNextWaypoint();
                        agent.SetDestination(currentWaypoint.position);
                    }
        */
    }

    private void NavigateToPosition(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Calculate steering angle
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        float steerAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        steerAngle = Mathf.Clamp(steerAngle, -maxSteeringAngle, maxSteeringAngle);

        // Apply steering to front wheels
        frontLeftCollider.steerAngle = steerAngle;
        frontRightCollider.steerAngle = steerAngle;

        // Adjust speed dynamically
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < 5f) // Slow down near targets
        {
            throttleAxis = Mathf.Max(0, throttleAxis - brakeForce * Time.fixedDeltaTime);
        }
        else
        {
            throttleAxis = Mathf.Min(1, throttleAxis + accelerationMultiplier * Time.fixedDeltaTime);
        }

        // Apply throttle
        float motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        rearLeftCollider.motorTorque = motorTorque;
        rearRightCollider.motorTorque = motorTorque;

    }

    private void UpdateWheelMeshes()
    {
        UpdateWheelMesh(frontLeftCollider, frontLeftMesh.transform);
        UpdateWheelMesh(frontRightCollider, frontRightMesh.transform);
        UpdateWheelMesh(rearLeftCollider, rearLeftMesh.transform);
        UpdateWheelMesh(rearRightCollider, rearRightMesh.transform);
    }

    private void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelMesh)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelMesh.position = position;
        wheelMesh.rotation = rotation;
    }
    private void UpdateSoundEffects()
    {
        if (engineSound != null)
        {
            enginePitch = Mathf.Lerp(0.5f, 1f, throttleAxis);
            engineSound.pitch = enginePitch;
        }

        if (tireSound != null)
        {
            // Adjust tire sound volume based on steering intensity
            float steeringIntensity = Mathf.Abs(frontLeftCollider.steerAngle / maxSteeringAngle);
            tireSound.volume = Mathf.Lerp(0f, 0.5f, steeringIntensity);
        }
    }
/*
    public void SetNextWaypoint(Transform nextWaypoint)
    {
        currentWaypoint = nextWaypoint;
        agent.SetDestination(currentWaypoint.position);
    }
*/}