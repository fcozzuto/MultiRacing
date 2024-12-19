using UnityEngine;

public class AICarController : MonoBehaviour
{
    public Transform[] waypoints;     // List of waypoints for the AI to follow
    public float maxSpeed = 10f;      // Max speed of the AI car
    public float acceleration = 1f;  // Acceleration rate
    public float braking = 10f;      // Braking rate
    public float turnSpeed = 2f;     // Steering sensitivity
    public float distanceThreshold = 3f;  // Distance to switch to the next waypoint

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints not assigned to AI Car.");
        }
    }

    void FixedUpdate()
    {
        FollowWaypoints();
    }

    void FollowWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Get the current and next waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        // Adjust speed based on distance to the current waypoint
        if (distanceToWaypoint < distanceThreshold)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - braking * Time.fixedDeltaTime);
            if (distanceToWaypoint < 1f) // Closer threshold for switching waypoints
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
        else
        {
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.fixedDeltaTime);
        }

        // Calculate direction to the waypoint
        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;

        // Smoothly rotate towards the target
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

        // Move forward with current speed
        Vector3 forwardMovement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);
    }
}