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

    public float minElevation = 0.0026f; 
    public float maxElevation = 0.43f; 

    private BocciaModel _model;

    [SerializeField] private float _elevationSpeed;
    [SerializeField] private float _rotationSpeed;

    private Vector3 elevationDirection; // Vector to define the direction of motion of the elevationMechanism visualization

    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;

        // Convert rampAdapter z-axis to local space of elevationMechanism parent to get the direction for the elevationMechanism visualization
        Vector3 rampDirection = rampAdapter.transform.forward;
        elevationDirection = elevationMechanism.transform.parent.InverseTransformDirection(rampDirection) * -1;

        // initialize ramp to saved data
        ModelChanged();
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
        StartCoroutine(RotationVisualization());
        StartCoroutine(ElevationVisualization());
    }

    private IEnumerator RotationVisualization()
    {
        _model.SetRampMoving(true);
        // Smoothly show the rotatation of the ramp to the new position
        Quaternion currentRotation = rotationShaft.transform.localRotation;
        //Debug.Log("Current Rotation: " + currentRotation.eulerAngles);
        Quaternion targetQuaternion = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, _model.RampRotation, rotationShaft.transform.localEulerAngles.z);
        //Debug.Log($"model.RampRotation value: {model.RampRotation}");

        while (Quaternion.Angle(currentRotation, targetQuaternion) > 0.01f)
        {
            currentRotation = Quaternion.Lerp(currentRotation, targetQuaternion, _rotationSpeed * Time.deltaTime);
            rotationShaft.transform.localRotation = currentRotation;
            yield return null;
        }

        rotationShaft.transform.localRotation = targetQuaternion;
        _model.SetRampMoving(false);
    }

    private IEnumerator ElevationVisualization()
    {
        _model.SetRampMoving(true);
        Vector3 currentElevation = elevationMechanism.transform.localPosition;
        //Debug.Log($"model.RampElevation value: {model.RampElevation}");
        float elevationScalar = minElevation + (_model.RampElevation / 100f) * (maxElevation - minElevation); // Convert percent elevation to its scalar value
        Vector3 targetElevation = elevationDirection * elevationScalar;

        while (Vector3.Distance(currentElevation, targetElevation) > 0.001f)
        {
            currentElevation = Vector3.Lerp(currentElevation, targetElevation, _elevationSpeed * Time.deltaTime);
            elevationMechanism.transform.localPosition = currentElevation;
            yield return null;
        }

        elevationMechanism.transform.localPosition = targetElevation;
        _model.SetRampMoving(false);
    }

}

