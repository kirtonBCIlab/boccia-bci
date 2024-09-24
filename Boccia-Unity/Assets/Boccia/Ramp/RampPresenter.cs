using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class RampPresenter : MonoBehaviour
{
    // TODO - This is a dummy example, replace with real ramp prefab object, etc
    public GameObject ramp;
    public GameObject ball;

    private BocciaModel model;

    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

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
        ramp.transform.rotation = Quaternion.AngleAxis(model.RampRotation, Vector3.up);
        ramp.transform.rotation *= Quaternion.AngleAxis(model.RampElevation, Vector3.right);

        // For lower rate changes, update when model sends change event
        ball.GetComponent<Renderer>().material.color = model.BallColor;
    }
}

