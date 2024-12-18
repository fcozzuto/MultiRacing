using UnityEngine;

public class AICarController : MonoBehaviour
{
    public Transform[] waypoints;    // List of waypoints for the AI to follow
    public float maxSpeed = 15f;     // Max speed of the AI car
    public float acceleration = 5f; // Acceleration rate
    public float braking = 10f;     // Braking rate
    public float turnSpeed = 2f;    // Steering speed
    public float distanceThreshold = 2f;  // Distance threshold to the next waypoint
    public float lookAheadDistance = 10f; // Distance to anticipate waypoints

    private int currentWaypointIndex = 0;
    private Rigidbody rb;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        FollowWaypoints();
    }

    void FollowWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Determine the current and look-ahead waypoints
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Transform lookAheadWaypoint = waypoints[(currentWaypointIndex + 1) % waypoints.Length];

        Vector3 directionToWaypoint = (targetWaypoint.position - transform.position).normalized;
        Vector3 directionToLookAhead = (lookAheadWaypoint.position - transform.position).normalized;

        // Smoothly steer toward the look-ahead waypoint
        Vector3 targetDirection = Vector3.Lerp(directionToWaypoint, directionToLookAhead, 0.5f);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Adjust speed based on distance to the current waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distanceToWaypoint < distanceThreshold)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - braking * Time.deltaTime); // Slow down near the waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Advance to the next waypoint
        }
        else
        {
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime); // Accelerate
        }

        // Apply movement
        Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }
}