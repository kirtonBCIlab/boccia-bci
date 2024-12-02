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
        // Store the starting rotation
        Quaternion startRotation = rotationShaft.transform.localRotation;
        // Store the target rotation based on the model.RampRotation value
        Quaternion targetRotation = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);

        // Calculate the angle between the start and target rotations
        float rotationAngle = Quaternion.Angle(startRotation, targetRotation);

        // Get the scaled rotation speed and convert it to degrees per second
        float scaledSpeed = _model.ScaleRotationSpeed(_rotationSpeed);
        float convertedSpeed = scaledSpeed * 360f;

        // Calculate the total time it will take to rotate
        float totalTime = rotationAngle / convertedSpeed;
        // Variable to store the current time
        float elapsedTime = 0f;

        // Rotate the ramp
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            // Interpolation factor
            float normalizedProgress = Mathf.Clamp01(elapsedTime / totalTime);

            // Interpolate between the start and target rotation
            rotationShaft.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, normalizedProgress);

            yield return null;
        }

        // After the rotation is complete, ensure the ramp is set to the target rotation
        rotationShaft.transform.localRotation = targetRotation;
    }

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

            // Interpolation factor
            float normalizedProgress = Mathf.Clamp01(elapsedTime / totalTime);

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

