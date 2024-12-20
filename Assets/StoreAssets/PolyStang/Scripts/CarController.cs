using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;
using TMPro;
using Cinemachine;

namespace PolyStang
{
    public class CarController : NetworkBehaviour
    {
        public enum ControlMode
        {
            Keyboard,
            Buttons
        };

        public enum Axel
        {
            Front,
            Rear
        }

        [Serializable]
        public struct Wheel
        {
            public GameObject wheelModel;
            public WheelCollider wheelCollider;
            public GameObject wheelEffectObj;
            public ParticleSystem smokeParticle;
            public Axel axel;
            public GameObject skidSound;
            public int index;
        }

        public ControlMode control;

        [Header("Inputs")]
        public KeyCode brakeKey = KeyCode.Space;

        [Header("Accelerations and deaccelerations")]
        public float maxAcceleration = 30.0f;
        public float brakeAcceleration = 50.0f;
        public float noInputDeacceleration = 10.0f;

        [Header("Steering")]
        public float turnSensitivity = 1.0f;
        public float maxSteerAngle = 30.0f;

        [Header("Speed UI")]
        public TMP_Text speedText;
        public float UISpeedMultiplier = 3.6f;

        [Header("Speed limit")]
        public float frontMaxSpeed = 200;
        public float rearMaxSpeed = 50;
        public float empiricalCoefficient = 0.41f;
        public enum TypeOfSpeedLimit
        {
            noSpeedLimit,
            simple,
            squareRoot
        };
        public TypeOfSpeedLimit typeOfSpeedLimit = TypeOfSpeedLimit.squareRoot;
        private float frontSpeedReducer = 1;
        private float rearSpeedReducer = 1;

        [Header("Skid")]
        public float brakeDriftingSkidLimit = 10f;
        public float lateralFrontDriftingSkidLimit = 0.6f;
        public float lateralRearDriftingSkidLimit = 0.3f;

        [Header("General")]
        public Vector3 _centerOfMass;
        public List<Wheel> wheels;
        private float moveInput;
        private float steerInput;
        private Rigidbody carRb;
        private CarLights carLights;
        private CarSounds carSounds;
        private RaceManager raceManager;

        internal bool canMove;

        private void Start()
        {
            raceManager = GetComponent<RaceManager>();
            carRb = GetComponent<Rigidbody>();
            carRb.centerOfMass = _centerOfMass;

            carLights = GetComponent<CarLights>();
            carSounds = GetComponent<CarSounds>();
        }

        private void Update()
        {
            if (!IsOwner) return; // Only allow the owner to control the car
            if (raceManager.raceStarted == true)
            {
                GetInputs();
                UpdateSpeedUI();
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner) return; // Owner-only actions
            AnimateWheels();
            WheelEffectsCheck();
            CarLightsControl();
            if (raceManager.raceStarted == true)
            {
                SubmitInputsServerRpc(moveInput, steerInput);
            }
        }

        [ServerRpc]
        private void SubmitInputsServerRpc(float move, float steer)
        {
            moveInput = move;
            steerInput = steer;
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            Move();
            Steer();
            BrakeAndDeacceleration();
            SyncVisualsClientRpc();
        }

        [ClientRpc]
        private void SyncVisualsClientRpc()
        {
            AnimateWheels();
        }

        public void MoveInput(float input)
        {
            if (!IsOwner) return;
            moveInput = input;
        }

        public void SteerInput(float input)
        {
            if (!IsOwner) return;
            steerInput = input;
        }

        private void GetInputs()
        {
            if (control == ControlMode.Keyboard)
            {
                moveInput = Input.GetAxis("Vertical");
                steerInput = Input.GetAxis("Horizontal");
            }
        }

        private void Move()
        {
            foreach (var wheel in wheels)
            {
                float currentWheelSpeed = empiricalCoefficient * wheel.wheelCollider.radius * wheel.wheelCollider.rpm;

                if (moveInput > 0 || currentWheelSpeed > 0)
                {
                    ApplySpeedLimit(ref currentWheelSpeed, frontMaxSpeed, ref frontSpeedReducer);
                    wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * frontSpeedReducer * Time.deltaTime;
                }
                else if (moveInput < 0 || currentWheelSpeed < 0)
                {
                    ApplySpeedLimit(ref currentWheelSpeed, -rearMaxSpeed, ref rearSpeedReducer);
                    wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * rearSpeedReducer * Time.deltaTime;
                }
            }
        }

        private void ApplySpeedLimit(ref float currentSpeed, float maxSpeed, ref float speedReducer)
        {
            if (Mathf.Abs(currentSpeed) > Mathf.Abs(maxSpeed))
            {
                currentSpeed = maxSpeed;
            }

            switch (typeOfSpeedLimit)
            {
                case TypeOfSpeedLimit.noSpeedLimit:
                    speedReducer = 1;
                    break;
                case TypeOfSpeedLimit.simple:
                    speedReducer = Mathf.Abs((maxSpeed - currentSpeed) / maxSpeed);
                    break;
                case TypeOfSpeedLimit.squareRoot:
                    speedReducer = Mathf.Sqrt(Mathf.Abs((maxSpeed - currentSpeed) / maxSpeed));
                    break;
            }
        }

        private void Steer()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.axel == Axel.Front)
                {
                    float steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, steerAngle, 0.6f);
                }
            }
        }

        private void BrakeAndDeacceleration()
        {
            foreach (var wheel in wheels)
            {
                if (Input.GetKey(brakeKey))
                {
                    wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
                }
                else if (moveInput == 0)
                {
                    wheel.wheelCollider.brakeTorque = 300 * noInputDeacceleration * Time.deltaTime;
                }
                else
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }
            }
        }

        private void AnimateWheels()
        {
            foreach (var wheel in wheels)
            {
                Quaternion rot;
                Vector3 pos;
                wheel.wheelCollider.GetWorldPose(out pos, out rot);
                wheel.wheelModel.transform.position = pos;
                wheel.wheelModel.transform.rotation = rot;
            }
        }

        private void WheelEffectsCheck()
        {
            foreach (var wheel in wheels)
            {
                WheelHit hit;
                wheel.wheelCollider.GetGroundHit(out hit);
                float lateralDrift = Mathf.Abs(hit.sidewaysSlip);

                if (Input.GetKey(brakeKey) && wheel.axel == Axel.Rear && carRb.velocity.magnitude >= brakeDriftingSkidLimit)
                {
                    EffectCreate(wheel);
                }
                else if (wheel.axel == Axel.Front && lateralDrift > lateralFrontDriftingSkidLimit)
                {
                    EffectCreate(wheel);
                }
                else if (wheel.axel == Axel.Rear && lateralDrift > lateralRearDriftingSkidLimit)
                {
                    EffectCreate(wheel);
                }
                else
                {
                    wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
                    carSounds.StopSkidSound(wheel.skidSound, wheel.index);
                }
            }
        }

        private void EffectCreate(Wheel wheel)
        {
            wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
            wheel.smokeParticle.Emit(1);
            carSounds.PlaySkidSound(wheel.skidSound);
        }

        private void CarLightsControl()
        {
            if (Input.GetKey(brakeKey))
            {
                carLights.RearRedLightsOn();
            }
            else
            {
                carLights.RearRedLightsOff();
            }

            if (moveInput < 0f)
            {
                carLights.RearWhiteLightsOn();
            }
            else
            {
                carLights.RearWhiteLightsOff();
            }
        }

        private void UpdateSpeedUI()
        {
            if (speedText == null) return;

            int roundedSpeed = (int)Mathf.Round(carRb.velocity.magnitude * UISpeedMultiplier);
            speedText.text = roundedSpeed.ToString();
        }
    }
}
