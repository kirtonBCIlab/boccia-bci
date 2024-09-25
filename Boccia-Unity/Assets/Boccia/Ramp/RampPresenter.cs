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
    private Animator barAnimation; // Drop Bar visualization
    public GameObject dropBar; // Note: This refers to the drop bar axis corrector
    public GameObject ball;

    private BocciaModel model;

    public float rotationSpeed = 10.0f;
    public float elevationSpeed = 5.0f;
    public float minElevation = 0.0026f; 
    public float maxElevation = 0.43f; 

    private Vector3 elevationDirection; // Vector to define the direction of motion of the elevationMechanism visualization

    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // Convert rampAdapter z-axis to local space of elevationMechanism parent to get the direction for the elevationMechanism visualization
        Vector3 rampDirection = rampAdapter.transform.forward;
        elevationDirection = elevationMechanism.transform.parent.InverseTransformDirection(rampDirection) * -1;

        // Initialize the Drop Bar animation
        barAnimation = dropBar.GetComponent<Animator>();

        // initialize ramp to saved data
        ModelChanged();
    }


    private void OnDisable()
    {
        BocciaModel.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        // Ramp is a digital twin, so we just match visualization with model data
        StartCoroutine(RotationVisualization());
        StartCoroutine(ElevationVisualization());
        
        // For lower rate changes, update when model sends change event
        ball.GetComponent<Renderer>().material.color = model.BallColor;
    }

    private IEnumerator RotationVisualization()
    {
        // Smoothly show the rotatation of the ramp to the new position
        Quaternion currentRotation = rotationShaft.transform.localRotation;
        //Debug.Log("Current Rotation: " + currentRotation.eulerAngles);
        Quaternion targetQuaternion = Quaternion.Euler(rotationShaft.transform.localEulerAngles.x, model.RampRotation, rotationShaft.transform.localEulerAngles.z);

        while (Quaternion.Angle(currentRotation, targetQuaternion) > 0.01f)
        {
            currentRotation = Quaternion.Lerp(currentRotation, targetQuaternion, rotationSpeed * Time.deltaTime);
            rotationShaft.transform.localRotation = currentRotation;
            yield return null;
        }

        rotationShaft.transform.localRotation = targetQuaternion;
    }

    private IEnumerator ElevationVisualization()
    {
        Vector3 currentElevation = elevationMechanism.transform.localPosition;
        float elevationScalar = minElevation + (model.RampElevation / 100f) * (maxElevation - minElevation); // Convert percent elevation to its scalar value
        Vector3 targetElevation = elevationDirection * elevationScalar;

        while (Vector3.Distance(currentElevation, targetElevation) > 0.001f)
        {
            currentElevation = Vector3.Lerp(currentElevation, targetElevation, elevationSpeed * Time.deltaTime);
            elevationMechanism.transform.localPosition = currentElevation;
            yield return null;
        }

        elevationMechanism.transform.localPosition = targetElevation;
    }

}

