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
        Quaternion startQuaternion = rotationShaft.transform.localRotation;
        Quaternion endQuaternion = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);

        // Calculate the angle between the start and target rotations
        float startAngle = rotationShaft.transform.localEulerAngles.y;
        float targetAngle = _model.RampRotation;
        float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(startAngle, targetAngle));
        float direction = Mathf.Sign(deltaAngle);
        
        // Convert speed to [deg/sec] and acceleration to [deg/sec^2]
        float maxRotationSpeed = _model.ScaleRotationSpeed(_rotationSpeed);
        float acceleration = _model.ScaleRotationAcceleration();

        // Calculate movement times
        float accelTime = maxRotationSpeed / acceleration;
        float angleInAccelPhase = 0.5f * acceleration * Mathf.Pow(accelTime, 2);

        float angleInConstantPhase = Mathf.Max(0, deltaAngle - (2 * angleInAccelPhase));
        float constantTime = angleInConstantPhase / maxRotationSpeed;
        
        // If the deltaAngle is too small to reach constant speed, adjust accelTime and decelTime
        if (angleInConstantPhase == 0)
        {
            accelTime = Mathf.Sqrt(deltaAngle / acceleration);
            constantTime = 0;
        }

        float totalTime = (2 * accelTime) + constantTime;
        
        // Initialize relative movement and time variables
        float currentAngle = 0f; 
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

            currentAngle += deltaAngleThisFrame;

            float t = currentAngle / deltaAngle;
            rotationShaft.transform.localRotation = Quaternion.Slerp(startQuaternion, endQuaternion, t);
            
            // Update the current rotation angle in the model if it has changed significantly
            float newRotationAngle = rotationShaft.transform.localEulerAngles.y;
            if (Mathf.Abs(newRotationAngle - _model.CurrentRotationAngle) > 1f) // Adjust the threshold as needed
            {
                _model.CurrentRotationAngle = newRotationAngle;
            }
            
            yield return null;
        }

        // Ensure the final rotation matches exactly
        rotationShaft.transform.localRotation = endQuaternion;
        _model.CurrentRotationAngle = rotationShaft.transform.localEulerAngles.y;
    }

    private IEnumerator ElevationVisualization()
    {
        // Store the starting elevation
        Vector3 startElevation = elevationMechanism.transform.localPosition;

        // Convert percent elevation from model.RampElevation to its scalar value
        float elevationScalar = ElevationVisualizationMin + (_model.RampElevation / 100f) * (ElevationVisualizationMax - ElevationVisualizationMin);
        elevationScalar = Mathf.Clamp(elevationScalar, ElevationVisualizationMin, ElevationVisualizationMax);

        // Store the target elevation
        Vector3 targetElevation = elevationDirection * elevationScalar;

        // Calculate the distance between the start and target elevations
        float elevationDistance = Vector3.Distance(startElevation, targetElevation);

        // Get the 8-bit PWM speed and convert it to m/s
        float scaledSpeed = _model.ScaleElevationSpeed(_elevationSpeed);

        // Calculate the total time it will take to elevate
        float totalTime = elevationDistance / scaledSpeed;
        // Variable to store the time
        float elapsedTime = 0f;

        // Elevate the ramp
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            // Interpolation factor to smooth out the elevation
            // float normalizedProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedTime / totalTime));
            float normalizedProgress = elapsedTime / totalTime;

            // Interpolate between the start and target elevation
            elevationMechanism.transform.localPosition = Vector3.Lerp(startElevation, targetElevation, normalizedProgress);

            yield return null;
        }

        // Ensure the final elevation matches exactly
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

