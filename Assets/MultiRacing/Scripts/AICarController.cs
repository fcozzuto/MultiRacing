using UnityEngine;

public class AICarController : MonoBehaviour
{
    public Transform[] waypoints;  // List of waypoints for the AI to follow
    public float maxSpeed = 15f;   // Max speed of the AI car
    public float turnSpeed = 2f;   // Steering speed
    public float distanceThreshold = 2f;  // Distance threshold to the next waypoint

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        FollowWaypoints();
    }

    void FollowWaypoints()
    {
        // Move toward the current waypoint
        Vector3 targetWaypoint = waypoints[currentWaypointIndex].position;
        Vector3 directionToWaypoint = (targetWaypoint - transform.position).normalized;

        // Calculate movement
        float step = maxSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + directionToWaypoint * step);

        // Rotate the AI car to face the waypoint
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Check if the AI has reached the current waypoint
        if (Vector3.Distance(transform.position, targetWaypoint) < distanceThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;  // Loop through waypoints
        }
    }
}