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

            //brakeForce *= 50;
            //maxSteeringAngle *= 3;
            //accelerationMultiplier *= 2;
            //maxSpeed *= 2;
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

    /*    private void NavigateToWaypoint()
        {
            if (!currentWaypoint) return;

            // Calculate direction to the waypoint
            Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;

            // Calculate steering angle
            Vector3 localTarget = transform.InverseTransformPoint(currentWaypoint.position);
            steerAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // Clamp the steering angle
            steerAngle = Mathf.Clamp(steerAngle, -maxSteeringAngle, maxSteeringAngle);

            // Apply steering to front wheels
            frontLeftCollider.steerAngle = steerAngle;
            frontRightCollider.steerAngle = steerAngle;

            // Adjust speed dynamically based on sharpness
            curveSharpness = Mathf.Abs(steerAngle) / maxSteeringAngle;
            Debug.Log($"Curve sharpness is {curveSharpness}");
            float adjustedMaxSpeed = Mathf.Lerp(maxSpeed * 0.5f, maxSpeed, 1f - curveSharpness);

            // Adjust throttle based on distance to the waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
            if (distanceToWaypoint < 5f) // Slow down near waypoints
            {
                throttleAxis = Mathf.Max(0, throttleAxis - brakeForce * Time.fixedDeltaTime);
            }
            else
            {
                throttleAxis = Mathf.Min(adjustedMaxSpeed / maxSpeed, throttleAxis + accelerationMultiplier * Time.fixedDeltaTime);
            }

            // Apply throttle
            ApplyThrottle();
        }
    */
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
        frontLeftCollider.steerAngle = steerAngle;
        frontRightCollider.steerAngle = steerAngle;

        // Adjust speed dynamically based on sharpness
        curveSharpness = Mathf.Abs(steerAngle) / maxSteeringAngle;
        float adjustedMaxSpeed = Mathf.Lerp(maxSpeed * 0.5f, maxSpeed, 1f - curveSharpness);

        // Apply deceleration logic
        DecelerateCar(curveSharpness, adjustedMaxSpeed);

        // Apply throttle
        ApplyThrottle();
    }

    private void DecelerateCar(float sharpness, float targetSpeed)
    {
        float currentSpeed = carRigidbody.velocity.magnitude * 3.6f; // Convert to km/h
        if (sharpness > 0.75f || currentSpeed > targetSpeed)
        {
            // Apply braking if curve is sharp or speed exceeds target
            throttleAxis = Mathf.Max(0, throttleAxis - brakeForce * Time.fixedDeltaTime);
            rearLeftCollider.brakeTorque = brakeForce;
            rearRightCollider.brakeTorque = brakeForce;
            Debug.Log($"Braking car {gameObject.GetType().Name}");
        }
        else
        {
            // Release brakes and accelerate
            throttleAxis = Mathf.Min(1, throttleAxis + accelerationMultiplier * Time.fixedDeltaTime);
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;
            //Debug.Log($"Accelerating car {gameObject.GetType().Name}");
        }
    }

    private void ApplyThrottle()
    {
        float motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        if (curveSharpness > 0.2f)
        {
            frontLeftCollider.motorTorque = 0;
            frontRightCollider.motorTorque = 0;
        }
        else
        {
            frontLeftCollider.motorTorque = motorTorque;
            frontRightCollider.motorTorque = motorTorque;
        }
        rearLeftCollider.motorTorque = motorTorque;
        rearRightCollider.motorTorque = motorTorque;
    }

    private void ApplyAntiRollBars()
    {
        ApplyAntiRoll(frontLeftCollider, frontRightCollider);
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
            enginePitch = Mathf.Lerp(1f, 3f, throttleAxis);
            engineSound.pitch = enginePitch;
        }

        if (tireSound != null)
        {
            // Adjust tire sound volume based on steering intensity
            float steeringIntensity = Mathf.Abs(frontLeftCollider.steerAngle / maxSteeringAngle);
            tireSound.volume = Mathf.Lerp(0f, 1f, steeringIntensity);
        }
    }

    public void SetNextWaypoint(Transform nextWaypoint)
    {
        currentWaypoint = nextWaypoint;
    }
}