using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class RampPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public GameObject rotationShaft; // Shaft and Ramp child component of the Boccia Ramp GameObject (the ramp parts that will rotate in the visualization)
    public GameObject elevationMechanism; // Elevation Mechanism child component of Shaft and Ramp (the ramp parts that will change elevation in the visualization)
    public GameObject rampAdapter; // To define direction of movement for elevationMechanism

    // For visualization only, to prevent elevation mechanism from extending beyond the ramp
    public float ElevationVisualizationMin = 0.0026f;
    public float ElevationVisualizationMax = 0.43f;

    private BocciaModel _model;

    [SerializeField] private float _elevationSpeed;
    [SerializeField] private float _rotationSpeed;
    
    private Vector3 elevationDirection; // Vector to define the direction of motion of the elevationMechanism visualization

    private BocciaGameMode _lastPlayMode;

    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;
        _model.NavigationChanged += NavigationChanged;

        // Convert rampAdapter z-axis to local space of elevationMechanism parent to get the direction for the elevationMechanism visualization
        Vector3 rampDirection = rampAdapter.transform.forward;
        elevationDirection = elevationMechanism.transform.parent.InverseTransformDirection(rampDirection) * -1;

        // initialize ramp to saved data
        ModelChanged();

        // Initialize the last play mode as virtual play
        _lastPlayMode = BocciaGameMode.Virtual;
    }

    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
    }


    private void ModelChanged()
    {
        // Pull elevation and rotation speeds from _model
        // As GameOptionsMenuPresenter uses BocciaModel.SetGameOption() to update model values
        // from the UI, which triggers BocciaModel.SendRampChangeEvent(), which this method will respond to,
        // Then, this method will run any time the User changes these values, ensuring the
        // elevation and rotation speeds are kept in sync with the model
        _elevationSpeed = _model.GameOptions.ElevationSpeed;
        _rotationSpeed = _model.GameOptions.RotationSpeed;

        // Ramp is a digital twin, so we just match visualization with model data
        //Debug.Log(model.RampRotation);
        StartCoroutine(RampVisualization());
    }

    private IEnumerator RampVisualization()
    {
        // Wait for ramp to stop moving before visualizing
        while (_model.IsRampMoving)
        {
            yield return null;
        }

        // Visualize the ramp moving, first rotate, then elevate
        _model.SetRampMoving(true);
        yield return StartCoroutine(RotationVisualization());
        yield return StartCoroutine(ElevationVisualization());
        _model.SetRampMoving(false);
    }

    private void NavigationChanged()
    {
        // Reset the ramp if the play mode changes
        ResetRampWhenPlayModeChanges();
    }

    private IEnumerator RotationVisualization()
    {
        // Starting and ending rotation points
        Quaternion startRotation = rotationShaft.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);
        
        float startAngle = rotationShaft.transform.localEulerAngles.y;
        float currentAngle = startAngle;
        float targetAngle = _model.RampRotation;
        float deltaAngle = Mathf.DeltaAngle(startAngle, targetAngle);
        float angle = Mathf.Abs(deltaAngle);
        float direction = Mathf.Sign(deltaAngle);
        Debug.Log("Rotation angle: " + angle);

        
        // Convert speed to [deg/sec] and acceleration to [deg/sec^2]
        float maxRotationSpeed = _model.ScaleRotationSpeed(_rotationSpeed);
        float acceleration = _model.ScaleRotationAcceleration();

        // Calculate movement times
        float accelTime = maxRotationSpeed / acceleration;
        float angleInAccelPhase = 0.5f * acceleration * Mathf.Pow(accelTime, 2);

        float angleInConstantPhase = Mathf.Max(0, angle - (2 * angleInAccelPhase));
        float constantTime = angleInConstantPhase / maxRotationSpeed;
        
        // If the angle is too small to reach constant speed, adjust accelTime and decelTime
        if (angleInConstantPhase == 0)
        {
            accelTime = Mathf.Sqrt(angle / acceleration);
            constantTime = 0;
        }

        float totalTime = (2 * accelTime) + constantTime;
        Debug.Log("Total time to rotate: " + totalTime);
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float deltaAngleThisFrame;

            // Acceleration phase
            if (elapsedTime < accelTime)
            {
                float currentSpeed = acceleration * elapsedTime * direction;
                deltaAngleThisFrame = currentSpeed * Time.deltaTime;
            }
            // Constant speed phase
            else if (elapsedTime < accelTime + constantTime)
            {
                deltaAngleThisFrame = maxRotationSpeed * Time.deltaTime * direction;
            }
            // Deceleration phase
            else
            {
                float timeToEnd = totalTime - elapsedTime;
                float currentSpeed = acceleration * timeToEnd * direction;
                deltaAngleThisFrame = currentSpeed * Time.deltaTime;
            }

            currentAngle += deltaAngleThisFrame * direction;
            // currentAngle = Mathf.Clamp(currentAngle, 0, angle);

            float t = currentAngle / angle;
            rotationShaft.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            
            yield return null;
            // Debug.Log("Current angle: " + currentAngle);
        }

        

        // Ensure the final rotation is exactly the end rotation
        // rotationShaft.transform.localRotation = endRotation;
    }

    // private float SmoothInterpolationFactor(
    //     float t,
    //     float totalTime,
    //     float timeToMaxSpeed,
    //     float constantSpeedRatio,
    //     float maxRotationSpeed,
    //     float acceleration
    //     )
    // {
    //     float accelerationPhase = timeToMaxSpeed / totalTime;
    //     float decelerationPhase = accelerationPhase;
    //     float constantSpeedPhase = constantSpeedRatio;

    //     // Acceleration phase
    //     if (t < accelerationPhase)
    //     {            
    //         return 0.5f * acceleration * t * t / (accelerationPhase * accelerationPhase);
    //     }
    //     // Constant speed phase
    //     else if (t < accelerationPhase + constantSpeedPhase)
    //     {
    //         return accelerationPhase * 0.5f + (t - accelerationPhase);
    //     }
    //     // Deceleration phase
    //     else
    //     {            
    //         float decelerationT = (t - (accelerationPhase + constantSpeedPhase)) / decelerationPhase;
    //         return (accelerationPhase * 0.5f + constantSpeedPhase) 
    //                 + (decelerationT - 0.5f * acceleration * decelerationT * decelerationT / (maxRotationSpeed * maxRotationSpeed))
    //                 * decelerationPhase;
    //     }
    // }
    // private IEnumerator RotationVisualization()
    // {
    //     // Compute starting, end, and trajectory rotation
    //     Quaternion startRotation = rotationShaft.transform.localRotation;
    //     Quaternion targetRotation = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);
    //     float rotationAngle = Quaternion.Angle(startRotation, targetRotation);

    //     // Convert speed to [deg/sec] and acceleration to [deg/sec^2]
    //     float scaledSpeed = _model.ScaleRotationSpeed(_rotationSpeed);
    //     float scaledAcceleration = _model.ScaleRotationAcceleration();
        
    //     // Calculate the time to reach maximum speed
    //     float timeToMaxSpeed = scaledSpeed / scaledAcceleration;

    //     // Calculate the distance covered during acceleration and deceleration
    //     float distanceToMaxSpeed = 0.5f * scaledAcceleration * Mathf.Pow(timeToMaxSpeed, 2);

    //     // Check if the rotation can reach maximum speed
    //     float totalTime;
    //     if (rotationAngle < 2 * distanceToMaxSpeed)
    //     {
    //         // If the rotation angle is too small to reach max speed, calculate the time needed for a triangular velocity profile
    //         totalTime = Mathf.Sqrt(4 * rotationAngle / scaledAcceleration);
    //     }
    //     else
    //     {
    //         // If the rotation angle is large enough, calculate the time needed for a trapezoidal velocity profile
    //         float distanceAtConstantSpeed = rotationAngle - 2 * distanceToMaxSpeed;
    //         float timeAtConstantSpeed = distanceAtConstantSpeed / scaledSpeed;
    //         totalTime = 2 * timeToMaxSpeed + timeAtConstantSpeed;
    //     }

    //     Debug.Log("Rotation angle: " + rotationAngle);
    //     Debug.Log("Scaled speed: " + scaledSpeed);
    //     Debug.Log("Total time to rotate: " + totalTime);

    //     // Variable to store the current time
    //     float elapsedTime = 0f;

    //     float currentSpeed = 0f;
    //     float currentAngle = 0f;

    //     // Rotate the ramp
    //     while (elapsedTime < totalTime)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float t = elapsedTime / totalTime;

    //         // Calculate the current speed with acceleration and deceleration
    //         if (t < 0.5f)
    //         {
    //             // Accelerate
    //             currentSpeed += scaledAcceleration * Time.deltaTime;
    //         }
    //         else
    //         {
    //             // Decelerate
    //             currentSpeed -= scaledAcceleration * Time.deltaTime;
    //         }
    //         currentSpeed = Mathf.Max(currentSpeed, 0f);

    //         // Calculate the current angle
    //         currentAngle += currentSpeed * Time.deltaTime;

    //         // Interpolate between the start and target rotation
    //         rotationShaft.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, currentAngle / rotationAngle);

    //         yield return null;
    //     }
    // }

    // private IEnumerator RotationVisualization()
    // {
    //     // Store the starting rotation
    //     Quaternion startRotation = rotationShaft.transform.localRotation;
    //     // Store the target rotation based on the model.RampRotation value
    //     Quaternion targetRotation = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);

    //     // Calculate the angle between the start and target rotations
    //     float rotationAngle = Quaternion.Angle(startRotation, targetRotation);

    //     // Get the scaled rotation speed and convert it to degrees per second
    //     float scaledSpeed = _model.ScaleRotationSpeed(_rotationSpeed);

    //     // Define the acceleration in steps/sec² and scale it
    //     float scaledAcceleration = _model.ScaleRotationAcceleration();
    //     Debug.Log("Scaled acceleration: " + scaledAcceleration);

    //     // Calculate the total time it will take to rotate
    //     float totalTime = rotationAngle / scaledSpeed;
    //     Debug.Log("Rotation angle: " + rotationAngle);
    //     Debug.Log("Scaled speed: " + scaledSpeed);
    //     Debug.Log("Total time to rotate: " + totalTime);
    //     // Variable to store the current time
    //     float elapsedTime = 0f;

    //     float currentSpeed = 0f;
    //     float currentAngle = 0f;

    //     // Rotate the ramp
    //     while (elapsedTime < totalTime)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float t = elapsedTime / totalTime;
    //         // float easeInOutFactor = t * t * (3f - 2f * t); // Smoothstep function
    //         // Calculate the current speed with acceleration and deceleration
    //         if (t < 0.5f)
    //         {
    //             // Accelerate
    //             currentSpeed += scaledAcceleration * Time.deltaTime;
    //         }
    //         else
    //         {
    //             // Decelerate
    //             currentSpeed -= scaledAcceleration * Time.deltaTime;
    //         }
    //         currentSpeed = Mathf.Max(currentSpeed, 0f);

    //         // Calculate the current speed with acceleration
    //         // float currentSpeed = scaledSpeed * easeInOutFactor;
    //         // float currentAngle = currentSpeed * elapsedTime + 0.5f * scaledAcceleration * Mathf.Pow(elapsedTime, 2);
    //         currentAngle += currentSpeed * Time.deltaTime;

    //         // Interpolate between the start and target rotation
    //         rotationShaft.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, currentAngle / _model.RampRotation);

    //         yield return null;
    //     }

    //     // After the rotation is complete, ensure the ramp is set to the target rotation
    //     rotationShaft.transform.localRotation = targetRotation;
    // }

    private IEnumerator ElevationVisualization()
    {
        // Store the starting elevation
        Vector3 startElevation = elevationMechanism.transform.localPosition;

        // Convert percent elevation from model.RampElevation to its scalar value
        float elevationScalar = ElevationVisualizationMin + (_model.RampElevation / 100f) * (ElevationVisualizationMax - ElevationVisualizationMin);

        // Make sure the elevation is within the min and max elevation bounds
        elevationScalar = Mathf.Clamp(elevationScalar, ElevationVisualizationMin, ElevationVisualizationMax);

        // Store the target elevation
        Vector3 targetElevation = elevationDirection * elevationScalar;

        // Calculate the distance between the start and target elevations
        float elevationDistance = Vector3.Distance(startElevation, targetElevation);

        // Get the scaled elevation speed and convert it to m/s (from inches/s)
        float scaledSpeed = _model.ScaleElevationSpeed(_elevationSpeed);
        float convertedSpeed = scaledSpeed * 0.0254f;

        // Calculate the total time it will take to elevate
        float totalTime = elevationDistance / convertedSpeed;
        // Variable to store the time
        float elapsedTime = 0f;

        // Elevate the ramp
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            // Interpolation factor to smooth out the elevation
            float normalizedProgress = Mathf.SmoothStep(0f, 1f, (Mathf.Clamp01(elapsedTime / totalTime)));

            // Interpolate between the start and target elevation
            elevationMechanism.transform.localPosition = Vector3.Lerp(startElevation, targetElevation, normalizedProgress);

            yield return null;
        }

        // After the elevation is complete, ensure the ramp is set to the target elevation
        elevationMechanism.transform.localPosition = targetElevation;
    }

    private void ResetRampWhenPlayModeChanges()
    {
        BocciaGameMode currentPlayMode = _model.GameMode;
        bool currentlyInPlayMode = (currentPlayMode == BocciaGameMode.Virtual) || (currentPlayMode == BocciaGameMode.Play);

        if (currentlyInPlayMode && _lastPlayMode != currentPlayMode)
        {
            Debug.Log("Resetting ramp position");
            _model.ResetRampPosition();
            _lastPlayMode = currentPlayMode;
        }
    }

}

