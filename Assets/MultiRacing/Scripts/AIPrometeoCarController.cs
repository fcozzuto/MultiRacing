using UnityEngine;
using UnityEngine.SceneManagement;

public class AIPrometeoCarController : MonoBehaviour
{
    public Transform currentWaypoint;
    private Rigidbody carRigidbody;

    // Copy fields from PrometeoCarController
    public int maxSpeed;
    public int maxReverseSpeed;
    public int accelerationMultiplier;
    public int brakeForce;
    public int maxSteeringAngle;
    public float steeringSpeed;
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    // Anti-roll bar settings
    public float antiRollForce = 20000f;

    // Sounds
    public AudioSource engineSound;
    public AudioSource tireSound;

    private float throttleAxis = 0f;
    private float enginePitch = 0f;
    private float steerAngle = 0f;
    private float curveSharpness = 0f;

    private float lookAheadDistance = 10f; // Distance to look ahead for detecting sharp turns

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Racing")
        {
            carRigidbody = GetComponent<Rigidbody>();
            if (currentWaypoint == null)
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
        if (currentWaypoint != null)
        {
            NavigateToWaypoint();
        }
        ApplyAntiRollBars();
        UpdateSoundEffects();
        UpdateWheelMeshes();
    }

    private void NavigateToWaypoint()
    {
        if (!currentWaypoint) return;

        // Calculate direction to the waypoint
        Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;

        // Calculate steering angle
        Vector3 localTarget = transform.InverseTransformPoint(currentWaypoint.position);
        steerAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        steerAngle = Mathf.Clamp(steerAngle, -maxSteeringAngle, maxSteeringAngle);

        // Apply steering to front wheels
        if (frontLeftCollider != null) frontLeftCollider.steerAngle = steerAngle;
        if (frontRightCollider != null) frontRightCollider.steerAngle = steerAngle;

        // Adjust speed dynamically based on sharpness
        curveSharpness = Mathf.Abs(steerAngle) / maxSteeringAngle;
        float adjustedMaxSpeed = Mathf.Lerp(maxSpeed * 0.5f, maxSpeed, 1f - curveSharpness);

        // Apply deceleration logic
        DecelerateCar(curveSharpness, adjustedMaxSpeed);

        // Adjust speed for upcoming sharp turns
        AdjustSpeedForSharpTurns();

        // Apply throttle
        ApplyThrottle();
    }

    private void AdjustSpeedForSharpTurns()
    {
        if (currentWaypoint == null) return;

        // Look ahead to the next waypoint
        Transform nextWaypoint = GetNextWaypoint();
        if (nextWaypoint == null) return;

        // Calculate the angle between the current direction and the direction to the next waypoint
        Vector3 currentDirection = transform.forward;
        Vector3 nextDirection = (nextWaypoint.position - currentWaypoint.position).normalized;
        float angleBetweenDirections = Vector3.Angle(currentDirection, nextDirection);
        float currentSpeed = carRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        // If the angle is sharp, reduce speed
        if (angleBetweenDirections > 30f)
        {
            Brake();
            throttleAxis = Mathf.Max(0, throttleAxis - brakeForce * Time.fixedDeltaTime * 9f); // Reduce speed more aggressively
        }
        else 
        {
            ReleaseBreaks();
            throttleAxis = Mathf.Min(1, throttleAxis + accelerationMultiplier * Time.fixedDeltaTime);
            ApplyThrottle();
        }
    }

    private Transform GetNextWaypoint()
    {
        return currentWaypoint.GetComponent<Waypoint>().nextWaypoint;
    }

    private void DecelerateCar(float sharpness, float targetSpeed)
    {
        if (carRigidbody == null) return;
        float currentSpeed = carRigidbody.velocity.magnitude * 3.6f; // Convert to km/h

        if (sharpness > 0.5f && currentSpeed > 30f)
        {
            Debug.Log($"Current Speed is: {currentSpeed}, while Target Speed is: {targetSpeed}");
            // Apply braking if curve is sharp or speed exceeds target
            ThrottleOff();
            Brake();
        }
        else
        {
            // Release brakes and accelerate
            ReleaseBreaks();
            throttleAxis = Mathf.Min(1, throttleAxis + accelerationMultiplier * Time.fixedDeltaTime);
            ApplyThrottle();
        }
    }

    private void ReleaseBreaks()
    {
        if (frontLeftCollider != null) frontLeftCollider.brakeTorque = 0;
        if (frontRightCollider != null) frontRightCollider.brakeTorque = 0;
        if (rearLeftCollider != null) rearLeftCollider.brakeTorque = 0;
        if (rearRightCollider != null) rearRightCollider.brakeTorque = 0;
    }

    private void Brake()
    {
        if (frontLeftCollider != null) frontLeftCollider.brakeTorque = brakeForce;
        if (frontRightCollider != null) frontRightCollider.brakeTorque = brakeForce;
        if (rearLeftCollider != null) rearLeftCollider.brakeTorque = brakeForce;
        if (rearRightCollider != null) rearRightCollider.brakeTorque = brakeForce;
        if (carRigidbody.velocity.magnitude > 15f)
        {
            carRigidbody.velocity *= (1f / (1f + (0.025f / 2f)));
        }
        else
        {
            ReleaseBreaks();
            ApplyThrottle();
        }
    }

    private void ApplyThrottle()
    {
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        float motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        if (frontLeftCollider != null) frontLeftCollider.motorTorque = motorTorque;
        if (frontRightCollider != null) frontRightCollider.motorTorque = motorTorque;
        if (rearLeftCollider != null) rearLeftCollider.motorTorque = motorTorque;
        if (rearRightCollider != null) rearRightCollider.motorTorque = motorTorque;
    }

    public void ThrottleOff()
    {
        if (frontLeftCollider != null) frontLeftCollider.motorTorque = 0;
        if (frontRightCollider != null) frontRightCollider.motorTorque = 0;
        if (rearLeftCollider != null) rearLeftCollider.motorTorque = 0;
        if (rearRightCollider != null) rearRightCollider.motorTorque = 0;
    }

    private void ApplyAntiRollBars()
    {
        if (frontLeftCollider != null && frontRightCollider != null)
            ApplyAntiRoll(frontLeftCollider, frontRightCollider);
        if (rearLeftCollider != null && rearRightCollider != null)
            ApplyAntiRoll(rearLeftCollider, rearRightCollider);
    }

    private void ApplyAntiRoll(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit leftHit;
        WheelHit rightHit;
        float leftTravel = 1f;
        float rightTravel = 1f;

        bool isLeftGrounded = leftWheel.GetGroundHit(out leftHit);
        bool isRightGrounded = rightWheel.GetGroundHit(out rightHit);

        if (isLeftGrounded)
        {
            leftTravel = (-leftWheel.transform.InverseTransformPoint(leftHit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }
        if (isRightGrounded)
        {
            rightTravel = (-rightWheel.transform.InverseTransformPoint(rightHit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        float antiRollForceApplied = (leftTravel - rightTravel) * antiRollForce;

        if (isLeftGrounded)
        {
            carRigidbody.AddForceAtPosition(leftWheel.transform.up * -antiRollForceApplied, leftWheel.transform.position);
        }
        if (isRightGrounded)
        {
            carRigidbody.AddForceAtPosition(rightWheel.transform.up * antiRollForceApplied, rightWheel.transform.position);
        }
    }

    private void UpdateWheelMeshes()
    {
        UpdateWheelMesh(frontLeftCollider, frontLeftMesh?.transform);
        UpdateWheelMesh(frontRightCollider, frontRightMesh?.transform);
        UpdateWheelMesh(rearLeftCollider, rearLeftMesh?.transform);
        UpdateWheelMesh(rearRightCollider, rearRightMesh?.transform);
    }

    private void UpdateWheelMesh(WheelCollider wheelCollider, Transform wheelMesh)
    {
        if (wheelCollider == null || wheelMesh == null) return;

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
            enginePitch = Mathf.Lerp(0f, 0.5f, throttleAxis);
            engineSound.pitch = enginePitch;
        }

        if (tireSound != null)
        {
            // Adjust tire sound volume based on steering intensity
            float steeringIntensity = Mathf.Abs(frontLeftCollider.steerAngle / maxSteeringAngle);
            tireSound.volume = Mathf.Lerp(0f, 0.25f, steeringIntensity);
        }
    }

    public void SetNextWaypoint(Transform nextWaypoint)
    {
        currentWaypoint = nextWaypoint;
    }
}