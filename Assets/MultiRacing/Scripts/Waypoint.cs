using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public Transform nextWaypoint; // Reference to the next waypoint in the sequence
    protected Transform firstWaypoint; // Reference to the first waypoint in the sequence

    private void OnTriggerEnter(Collider other)
    {
        AIPrometeoCarController aiCar = other.gameObject.GetComponentInParent<AIPrometeoCarController>();
        if (aiCar != null)
        {
            aiCar.SetNextWaypoint(nextWaypoint);
            Debug.Log($"Waypoint {name} reached by {aiCar.name}");
        }
    }

    private void Start()
    {
        firstWaypoint = GameManager.Instance.waypoints[0];
    }

    private void OnDrawGizmos()
    {
        if (nextWaypoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextWaypoint.position);
        }
    }
}