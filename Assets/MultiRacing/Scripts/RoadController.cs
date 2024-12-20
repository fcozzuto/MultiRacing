using UnityEngine;
using System.Collections.Generic;

public class RoadController : MonoBehaviour
{
    private Dictionary<GameObject, Vector3> lastContactPoints = new Dictionary<GameObject, Vector3>();

    private void OnCollisionStay(Collision collision)
    {
        // Check if the object in contact has a Rigidbody (only cars in your game have this)
        Rigidbody rb = collision.rigidbody;
        if (rb != null && (rb.GetComponent<PrometeoCarController>() || rb.GetComponent<AIPrometeoCarController>()))
        {
            // Update the last contact point for the car
            lastContactPoints[rb.gameObject] = collision.contacts[0].point;
            Debug.Log($"Updated last contact point for {rb.gameObject.name} to {collision.contacts[0].point}");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Check if the object leaving the track has a Rigidbody (only cars in your game have this)
        Rigidbody rb = collision.rigidbody;
        if (rb != null && (rb.GetComponent<PrometeoCarController>() || rb.GetComponent<AIPrometeoCarController>()))
        {
            if (lastContactPoints.TryGetValue(rb.gameObject, out Vector3 lastContactPoint))
            {
                Debug.Log($"Resetting position for {rb.gameObject.name} at last contact point {lastContactPoint}");
                ResetCarPosition(rb.gameObject, lastContactPoint); // Use the last contact point
                lastContactPoints.Remove(rb.gameObject); // Clean up the dictionary entry
            }
        }
    }

    private void ResetCarPosition(GameObject car, Vector3 collisionExitPoint)
    {
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (!rb) return;

        // Stop the car's motion
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calculate reset position and rotation
        Transform lastWaypoint = GetLastWaypoint(car);

        // Determine reset position and rotation
        Vector3 resetPosition;
        Quaternion resetRotation;
        if (car.GetComponent<PrometeoCarController>())
        {
            // For player car, respawn at collision exit point with offset
            resetPosition = collisionExitPoint + car.transform.forward * 5f;
            resetRotation = Quaternion.LookRotation(car.transform.forward, Vector3.up);
        }
        else
        {
            // For AI cars, respawn at the last waypoint
            resetPosition = lastWaypoint.position - lastWaypoint.forward * 5f; // Slightly behind the waypoint
            resetRotation = Quaternion.LookRotation(lastWaypoint.forward, Vector3.up);
        }

        // Reset car's position and rotation
        car.transform.position = resetPosition;
        car.transform.rotation = resetRotation;

        // Optional: Show visual feedback for reset
        ShowResetEffect(resetPosition);

        Debug.Log($"{car.name} reset to position: {resetPosition}, rotation: {resetRotation.eulerAngles}");
    }

    private Transform GetLastWaypoint(GameObject car)
    {
        // Fetch the last valid waypoint for AI or Player
        var aiController = car.GetComponent<AIPrometeoCarController>();
        if (aiController != null) return aiController.currentWaypoint;

        return null; // Adjust this if Player's waypoint logic is needed
    }

    private void ShowResetEffect(Vector3 position)
    {
        // Example: Add visual or audio feedback for reset
        GameObject effect = Resources.Load<GameObject>("CarResetEffect"); // Load a prefab for the effect
        if (effect != null)
        {
            Instantiate(effect, position, Quaternion.identity);
        }
    }
}