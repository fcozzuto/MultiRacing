using UnityEngine;

public class AICarController : MonoBehaviour
{
    public Transform currentWaypoint; // Current waypoint to follow
    public Transform nextWaypoint;    // Next waypoint for sharp turn detection
    public float maxSpeed = 25f;      // Max speed of the AI car
    public float acceleration = 3f;  // Acceleration rate
    public float braking = 30f;      // Braking rate
    public float turnSpeed = 20f;     // Steering sensitivity
    public float sharpTurnSlowdown = 50f; // Additional slowdown for sharp turns

    private Rigidbody rb;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (currentWaypoint == null)
        {
            Debug.LogError($"Current waypoint not assigned to AI Car: {gameObject.name}");
        }
    }

    void FixedUpdate()
    {
        FollowWaypoint();
    }

    void FollowWaypoint()
    {
        if (currentWaypoint == null) return;

        // Calculate direction to the waypoint
        Vector3 directionToWaypoint = (currentWaypoint.transform.position - transform.position).normalized;

        // Smoothly rotate towards the waypoint
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

        // Adjust speed based on distance and sharp turn detection
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);
        float targetSpeed = CalculateTargetSpeed(distanceToWaypoint);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        // Move forward with current speed
        Vector3 forwardMovement = transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        // Switch to the next waypoint if close enough
        if (distanceToWaypoint < 1f)
        {
            SetNextWaypoint(nextWaypoint);
        }
    }

    float CalculateTargetSpeed(float distanceToWaypoint)
    {
        float targetSpeed = maxSpeed;

        // Slow down near the waypoint
        if (distanceToWaypoint < 3f) // Adjust this threshold based on track size
        {
            targetSpeed -= braking * Time.fixedDeltaTime;
        }

        // Check if the turn angle is sharp and apply additional slowdown
        if (nextWaypoint != null)
        {
            Vector3 toCurrentWaypoint = (currentWaypoint.position - transform.position).normalized;
            Vector3 toNextWaypoint = (nextWaypoint.position - currentWaypoint.position).normalized;

            float angle = Vector3.Angle(toCurrentWaypoint, toNextWaypoint);

            // If the angle is sharp (e.g., < 60 degrees), slow down
            if (angle < 60f)
            {
                targetSpeed -= sharpTurnSlowdown;
                Debug.Log($"Sharp turn detected. Angle: {angle:F2} degrees. Slowing down.");
            }
        }

        return Mathf.Clamp(targetSpeed, 0, maxSpeed);
    }

    public void SetNextWaypoint(Transform nextWaypoint)
    {
        currentWaypoint = nextWaypoint;
        this.nextWaypoint = currentWaypoint != null ? currentWaypoint.GetComponent<Waypoint>()?.nextWaypoint : null;

        Debug.Log($"AI Car {name} reached waypoint, moving to next: {currentWaypoint?.name}");
    }
}